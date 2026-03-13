using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class UserAuthService : IUserAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ITempTokenService _tempTokenService;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly ISmsService _smsService;
    private readonly IOtpCodeReadRepository _otpCodeReadRepository;
    private readonly IOtpCodeWriteRepository _otpCodeWriteRepository;
    private readonly ILogger<UserAuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOtpSenderService _otpSenderService;

    public UserAuthService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        ITempTokenService tempTokenService,
        IAppleTokenValidator appleTokenValidator,
        ISmsService smsService,
        IOtpCodeReadRepository otpCodeReadRepository,
        IOtpCodeWriteRepository otpCodeWriteRepository,
        ILogger<UserAuthService> logger,
        IConfiguration configuration,
        IOtpSenderService otpSenderService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _tempTokenService = tempTokenService;
        _appleTokenValidator = appleTokenValidator;
        _smsService = smsService;
        _otpCodeReadRepository = otpCodeReadRepository;
        _otpCodeWriteRepository = otpCodeWriteRepository;
        _logger = logger;
        _configuration = configuration;
        _otpSenderService = otpSenderService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existByPhone = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber);
        if (existByPhone != null)
            throw new GlobalAppException("PHONE_NUMBER_ALREADY_EXISTS");

        var existByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existByEmail != null)
            throw new GlobalAppException("EMAIL_ALREADY_EXISTS");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.PhoneNumber,
            PhoneNumber = dto.PhoneNumber,
            PhoneNumberConfirmed = false,
            EmailConfirmed = true,
            Language = "az",
            TimeZone = "Asia/Baku",
            AccountType = "Customer"
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            throw new GlobalAppException("USER_CREATE_FAILED");

        await EnsureCustomerRoleAsync();
        var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!addRoleResult.Succeeded)
            throw new GlobalAppException("ROLE_ASSIGN_FAILED");

        await CreateAndSendOtpAsync(dto.PhoneNumber);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber!,
            OtpSent = true,
            Message = "REGISTER_SUCCESS"
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new GlobalAppException("INVALID_CREDENTIALS");

        if (!user.PhoneNumberConfirmed)
            throw new GlobalAppException("PHONE_NOT_CONFIRMED");

        return await BuildLoginResponseAsync(user);
    }

    public async Task<bool> SendOtpAsync(SendOtpRequestDto dto)
    {
        var user = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        await CreateAndSendOtpAsync(dto.PhoneNumber);
        return true;
    }

    public async Task<LoginResponseDto> VerifyUserAsync(VerifyOtpRequestDto dto)
    {
        await VerifyOtpCoreAsync(dto.PhoneNumber, dto.Code);

        var user = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        user.PhoneNumberConfirmed = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new GlobalAppException("USER_CONFIRM_FAILED");

        return await BuildLoginResponseAsync(user);
    }

 public async Task<SocialAuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto)
{
    try
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.IdToken))
            throw new GlobalAppException("INVALID_GOOGLE_TOKEN");

        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                UserName = payload.Email,
                FullName = $"{payload.GivenName} {payload.FamilyName}".Trim(),
                GoogleProviderId = payload.Subject,
                OAuthProvider = "Google",
                EmailConfirmed = true,
                AccountType = "basic",
                Language = "az",
                TimeZone = "Asia/Baku",
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                _logger.LogWarning("Google user create failed: {Errors}", errors);
                throw new GlobalAppException($"USER_CREATE_FAILED: {errors}");
            }
            if (!createResult.Succeeded)
                throw new GlobalAppException("USER_CREATE_FAILED");
            

            var roleExists = await _roleManager.RoleExistsAsync("Customer");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));
                if (!roleResult.Succeeded)
                    throw new GlobalAppException("ROLE_CREATE_FAILED");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!addRoleResult.Succeeded)
                throw new GlobalAppException("ROLE_ASSIGN_FAILED");
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber) || !user.PhoneNumberConfirmed)
        {
            var tempToken = await _tempTokenService.GenerateTempTokenAsync(user, "Google");

            return new SocialAuthResponseDto
            {
                RequiresPhoneVerification = true,
                TempToken = tempToken
            };
        }

        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new SocialAuthResponseDto
        {
            RequiresPhoneVerification = false,
            Auth = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            }
        };
    }
    catch (GlobalAppException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Google login zamanı xəta baş verdi");
        throw new GlobalAppException("GOOGLE_LOGIN_FAILED");
    }
}

public async Task<SocialAuthResponseDto> AppleLoginAsync(AppleLoginRequestDto dto)
{
    try
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.IdentityToken))
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var principal = await _appleTokenValidator.ValidateIdentityTokenAsync(dto.IdentityToken);

        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var sub = principal.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(sub))
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        User? user = null;

        if (!string.IsNullOrWhiteSpace(email))
            user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
           
            user = new User
            {
                Email = email,
                UserName = !string.IsNullOrWhiteSpace(email) ? email : $"apple_{sub}",
                FullName = "Apple User",
                AppleProviderId = sub,
                OAuthProvider = "Apple",
                EmailConfirmed = true,
                AccountType = "basic",
                Language = "az",
                TimeZone = "Asia/Baku",
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new GlobalAppException("USER_CREATE_FAILED");

            var roleExists = await _roleManager.RoleExistsAsync("Customer");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));
                if (!roleResult.Succeeded)
                    throw new GlobalAppException("ROLE_CREATE_FAILED");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!addRoleResult.Succeeded)
                throw new GlobalAppException("ROLE_ASSIGN_FAILED");
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber) || !user.PhoneNumberConfirmed)
        {
            var tempToken = await _tempTokenService.GenerateTempTokenAsync(user, "Apple");

            return new SocialAuthResponseDto
            {
                RequiresPhoneVerification = true,
                TempToken = tempToken
            };
        }

        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new SocialAuthResponseDto
        {
            RequiresPhoneVerification = false,
            Auth = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            }
        };
    }
    catch (GlobalAppException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Apple login zamanı xəta baş verdi");
        throw new GlobalAppException("APPLE_LOGIN_FAILED");
    }
}

public async Task<bool> CompleteSocialPhoneAsync(CompleteSocialPhoneRequestDto dto)
{
    var principal = _tempTokenService.ValidateTempToken(dto.TempToken);
    if (principal == null)
        throw new GlobalAppException("INVALID_TEMP_TOKEN");

    var userId = principal.FindFirst("userId")?.Value;
    if (string.IsNullOrWhiteSpace(userId))
        throw new GlobalAppException("INVALID_TEMP_TOKEN");

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        throw new GlobalAppException("USER_NOT_FOUND");

    var existByPhone = _userManager.Users.FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber && x.Id != user.Id);
    if (existByPhone != null)
        throw new GlobalAppException("PHONE_NUMBER_ALREADY_EXISTS");

    var oldCodes = await _otpCodeReadRepository.GetAllAsync(x => x.PhoneNumber == dto.PhoneNumber && !x.IsUsed);

    foreach (var oldCode in oldCodes)
    {
        oldCode.IsUsed = true;
        await _otpCodeWriteRepository.UpdateAsync(oldCode);
    }

    var otpCode = GenerateOtpCode();

    var otpEntity = new OtpCode
    {
        PhoneNumber = dto.PhoneNumber,
        Code = otpCode,
        ExpireAt = DateTime.UtcNow.AddMinutes(3),
        IsUsed = false,
        AttemptCount = 0
    };

    await _otpCodeWriteRepository.AddAsync(otpEntity);
    await _otpCodeWriteRepository.CommitAsync();

    var sent = await _smsService.SendOtpAsync(dto.PhoneNumber, otpCode);
    if (!sent)
        throw new GlobalAppException("OTP_SEND_FAILED");

    return true;
}

public async Task<LoginResponseDto> VerifySocialPhoneOtpAsync(VerifySocialPhoneOtpRequestDto dto)
{
    var principal = _tempTokenService.ValidateTempToken(dto.TempToken);
    if (principal == null)
        throw new GlobalAppException("INVALID_TEMP_TOKEN");

    var userId = principal.FindFirst("userId")?.Value;
    if (string.IsNullOrWhiteSpace(userId))
        throw new GlobalAppException("INVALID_TEMP_TOKEN");

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
        throw new GlobalAppException("USER_NOT_FOUND");

    var otp = await _otpCodeReadRepository.GetAsync(x => x.PhoneNumber == dto.PhoneNumber && !x.IsUsed);

    if (otp == null)
        throw new GlobalAppException("OTP_NOT_FOUND");

    if (otp.ExpireAt < DateTime.UtcNow)
        throw new GlobalAppException("OTP_EXPIRED");

    if (otp.AttemptCount >= 5)
        throw new GlobalAppException("OTP_MAX_ATTEMPT_EXCEEDED");

    if (otp.Code != dto.Code)
    {
        otp.AttemptCount++;
        await _otpCodeWriteRepository.UpdateAsync(otp);
        await _otpCodeWriteRepository.CommitAsync();
        throw new GlobalAppException("INVALID_OTP");
    }

    otp.IsUsed = true;
    await _otpCodeWriteRepository.UpdateAsync(otp);

    user.PhoneNumber = dto.PhoneNumber;
    user.PhoneNumberConfirmed = true;

    var updateResult = await _userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
        throw new GlobalAppException("USER_CONFIRM_FAILED");

    await _otpCodeWriteRepository.CommitAsync();

    var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
    var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

    return new LoginResponseDto
    {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresIn = 3600
    };
}
    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new GlobalAppException("REFRESH_TOKEN_REQUIRED");

        return await _tokenService.RefreshTokenAsync(refreshToken, ipAddress);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new GlobalAppException("REFRESH_TOKEN_REQUIRED");

        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);
    }

    private async Task<LoginResponseDto> BuildLoginResponseAsync(User user)
    {
        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600
        };
    }

    private async Task EnsureCustomerRoleAsync()
    {
        var roleExists = await _roleManager.RoleExistsAsync("Customer");
        if (!roleExists)
        {
            var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));
            if (!roleCreateResult.Succeeded)
                throw new GlobalAppException("ROLE_CREATE_FAILED");
        }
    }

    private async Task CreateAndSendOtpAsync(string phoneNumber)
    {
        var oldCodes = await _otpCodeReadRepository.GetAllAsync(x => x.PhoneNumber == phoneNumber && !x.IsUsed);
        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
            await _otpCodeWriteRepository.UpdateAsync(oldCode);
        }

        var otpCode = GenerateOtpCode();

        var otpEntity = new OtpCode
        {
            PhoneNumber = phoneNumber,
            Code = otpCode,
            ExpireAt = DateTime.UtcNow.AddMinutes(3),
            IsUsed = false,
            AttemptCount = 0
        };

        await _otpCodeWriteRepository.AddAsync(otpEntity);
        await _otpCodeWriteRepository.CommitAsync();
        var sent=await _otpSenderService.SendOtpAsync(phoneNumber, otpCode);

        // var sent = await _smsService.SendOtpAsync(phoneNumber, otpCode);
         if (!sent)
           throw new GlobalAppException("OTP_SEND_FAILED");
    }

    private async Task VerifyOtpCoreAsync(string phoneNumber, string code)
    {
        var otp = await _otpCodeReadRepository.GetAsync(x => x.PhoneNumber == phoneNumber && !x.IsUsed);

        if (otp == null)
            throw new GlobalAppException("OTP_NOT_FOUND");

        if (otp.ExpireAt < DateTime.UtcNow)
            throw new GlobalAppException("OTP_EXPIRED");

        if (otp.AttemptCount >= 5)
            throw new GlobalAppException("OTP_MAX_ATTEMPT_EXCEEDED");

        if (otp.Code != code)
        {
            otp.AttemptCount++;
            await _otpCodeWriteRepository.UpdateAsync(otp);
            await _otpCodeWriteRepository.CommitAsync();
            throw new GlobalAppException("INVALID_OTP");
        }

        otp.IsUsed = true;
        await _otpCodeWriteRepository.UpdateAsync(otp);
        await _otpCodeWriteRepository.CommitAsync();
    }

    private bool IsSameSocialUser(User user, ClaimsPrincipal principal)
    {
        var provider = principal.FindFirstValue("provider");
        var providerUserId = principal.FindFirstValue("provider_user_id");

        return (provider == "Google" && user.GoogleProviderId == providerUserId)
            || (provider == "Apple" && user.AppleProviderId == providerUserId);
    }

    private static string GenerateOtpCode()
        => System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
}