using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SavaloApp.Application.Abstracts.Repositories.CurrencyAccounts;
using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;
using SavaloApp.Domain.HelperEntities;

namespace SavaloApp.Persistance.Concretes.Services;

public class UserAuthService : IUserAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly ITempTokenService _tempTokenService;
    private readonly IAppleTokenValidator _appleTokenValidator;
    private readonly ISmsService _smsService;
    private readonly IOtpSenderService _otpSenderService;
    private readonly IOtpCodeReadRepository _otpCodeReadRepository;
    private readonly IOtpCodeWriteRepository _otpCodeWriteRepository;
    private readonly ILogger<UserAuthService> _logger;
    private readonly IMailService _mailService;
    private readonly ICurrencyAccountWriteRepository _currencyWriteRepo;

    public UserAuthService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService,
        ITempTokenService tempTokenService,
        IAppleTokenValidator appleTokenValidator,
        ISmsService smsService,
        IOtpSenderService otpSenderService,
        IOtpCodeReadRepository otpCodeReadRepository,
        IOtpCodeWriteRepository otpCodeWriteRepository,
        ILogger<UserAuthService> logger,
        IMailService mailService,
        ICurrencyAccountWriteRepository currencyWriteRepo)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _tempTokenService = tempTokenService;
        _appleTokenValidator = appleTokenValidator;
        _smsService = smsService;
        _otpSenderService = otpSenderService;
        _otpCodeReadRepository = otpCodeReadRepository;
        _otpCodeWriteRepository = otpCodeWriteRepository;
        _logger = logger;
        _mailService = mailService;
        _currencyWriteRepo = currencyWriteRepo;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
    {
        var confirmedUserWithSamePhone = _userManager.Users
            .FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber && x.PhoneNumberConfirmed);

        if (confirmedUserWithSamePhone != null)
            throw new GlobalAppException("PHONE_NUMBER_ALREADY_EXISTS");

        var user = new User
        {
            FullName = dto.FullName,
            Email = null,
            UserName = Guid.NewGuid().ToString(),
            PhoneNumber = dto.PhoneNumber,
            PhoneNumberConfirmed = false,
            EmailConfirmed = false,
            Language = "az",
            TimeZone = "Asia/Baku",
            AccountType = "Customer"
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
            _logger.LogWarning("User create failed: {Errors}", errors);
            throw new GlobalAppException("USER_CREATE_FAILED");
        }
        await _currencyWriteRepo.AddAsync(new CurrencyAccount
        {
            Name = "AZN",
            Icon = "https://i.postimg.cc/2jV2Z500/Azeri-manat-symbol-svg.png", // istəsən dəyiş
            UserId = user.Id
        });

        await _currencyWriteRepo.CommitAsync();

        await EnsureCustomerRoleAsync();

        var addRoleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!addRoleResult.Succeeded)
            throw new GlobalAppException("ROLE_ASSIGN_FAILED");

        await CreateAndSendPhoneOtpAsync(user);

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
        var user = _userManager.Users
            .FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber && x.PhoneNumberConfirmed);

        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new GlobalAppException("INVALID_CREDENTIALS");

        return await BuildLoginResponseAsync(user);
    }

    public async Task<bool> SendOtpAsync(SendOtpRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        if (user.PhoneNumberConfirmed)
            throw new GlobalAppException("PHONE_ALREADY_CONFIRMED");

        await CreateAndSendPhoneOtpAsync(user);
        return true;
    }

    public async Task<LoginResponseDto> VerifyUserAsync(VerifyOtpRequestDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        await VerifyOtpCoreAsync(user.Id, "PhoneVerification", dto.Code);

        user.PhoneNumberConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new GlobalAppException("USER_CONFIRM_FAILED");

        var duplicateUsers = _userManager.Users
            .Where(x => x.PhoneNumber == user.PhoneNumber && x.Id != user.Id)
            .ToList();

        foreach (var duplicate in duplicateUsers)
        {
            await DeleteUserWithOtpCodesAsync(duplicate);
        }

        return await BuildLoginResponseAsync(user);
    }

public async Task<SocialAuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto)
{
    try
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.IdToken))
            throw new GlobalAppException("INVALID_GOOGLE_TOKEN");

        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

        User? user = null;

        // 🔍 1. User tap
        if (!string.IsNullOrWhiteSpace(payload.Email))
            user = await _userManager.FindByEmailAsync(payload.Email);
     

        // 🆕 2. USER YOXDUR → CREATE
        if (user == null)
        {
            var picture = payload.Picture;
 

            if (!string.IsNullOrWhiteSpace(picture))
                picture = picture.Replace("s96-c", "s400-c");

            user = new User
            {
                Email = payload.Email,
                UserName = !string.IsNullOrWhiteSpace(payload.Email)
                    ? payload.Email
                    : Guid.NewGuid().ToString(),
                FullName = $"{payload.GivenName} {payload.FamilyName}".Trim(),
                GoogleProviderId = payload.Subject,
                OAuthProvider = "Google",
                EmailConfirmed = true,
                AccountType = "Customer",
                Language = "az",
                TimeZone = "Asia/Baku",
                ProfileImage = picture,
                PhoneNumberConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new GlobalAppException("USER_CREATE_FAILED");
            await _currencyWriteRepo.AddAsync(new CurrencyAccount
            {
                Name = "AZN",
                Icon = "https://i.postimg.cc/2jV2Z500/Azeri-manat-symbol-svg.png", // istəsən dəyiş
                UserId = user.Id
            });

            await _currencyWriteRepo.CommitAsync();

            await EnsureCustomerRoleAsync();

            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
                throw new GlobalAppException("ROLE_ASSIGN_FAILED");
        }
        else
        {
            // 🔄 3. EXISTING USER UPDATE
            var updated = false;

            if (string.IsNullOrWhiteSpace(user.GoogleProviderId))
            {
                user.GoogleProviderId = payload.Subject;
                user.OAuthProvider = "Google";
                updated = true;
            }

            // 🖼 şəkil update (əgər boşdursa)
            if (string.IsNullOrWhiteSpace(user.ProfileImage) && !string.IsNullOrWhiteSpace(payload.Picture))
            {
                var picture = payload.Picture.Replace("s96-c", "s400-c");
                user.ProfileImage = picture;
                updated = true;
            }

            if (updated)
                await _userManager.UpdateAsync(user);
        }

        // ✅ 4. PHONE VARSA → LOGIN
        if (!string.IsNullOrWhiteSpace(user.PhoneNumber) && user.PhoneNumberConfirmed)
        {
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

        // 📲 5. PHONE YOXDUR → TEMP TOKEN
        var tempToken = await _tempTokenService.GenerateTempTokenAsync(user, "Google");

        return new SocialAuthResponseDto
        {
            RequiresPhoneVerification = true,
            TempToken = tempToken
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
                    UserName = !string.IsNullOrWhiteSpace(email)
                        ? email
                        : $"apple_{sub}",
                    FullName = "Apple User",
                    AppleProviderId = sub,
                    OAuthProvider = "Apple",
                    EmailConfirmed = !string.IsNullOrWhiteSpace(email),
                    AccountType = "Customer",
                    Language = "az",
                    TimeZone = "Asia/Baku",
                    PhoneNumberConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new GlobalAppException("USER_CREATE_FAILED");
                await _currencyWriteRepo.AddAsync(new CurrencyAccount
                {
                    Name = "AZN",
                    Icon = "https://i.postimg.cc/2jV2Z500/Azeri-manat-symbol-svg.png", // istəsən dəyiş
                    UserId = user.Id
                });

                await _currencyWriteRepo.CommitAsync();

                await EnsureCustomerRoleAsync();

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

        var confirmedUserWithSamePhone = _userManager.Users
            .FirstOrDefault(x => x.PhoneNumber == dto.PhoneNumber && x.PhoneNumberConfirmed && x.Id != user.Id);

        if (confirmedUserWithSamePhone != null)
            throw new GlobalAppException("PHONE_NUMBER_ALREADY_EXISTS");

        user.PhoneNumber = dto.PhoneNumber;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new GlobalAppException("USER_UPDATE_FAILED");

        await CreateAndSendPhoneOtpAsync(user);
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

        await VerifyOtpCoreAsync(user.Id, "PhoneVerification", dto.Code);

        user.PhoneNumberConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new GlobalAppException("USER_CONFIRM_FAILED");

        var duplicateUsers = _userManager.Users
            .Where(x => x.PhoneNumber == user.PhoneNumber && x.Id != user.Id)
            .ToList();

        foreach (var duplicate in duplicateUsers)
        {
            await DeleteUserWithOtpCodesAsync(duplicate);
        }

        return await BuildLoginResponseAsync(user);
    }
       
    public async Task<bool> StartEmailUpdateAsync(string userId, StartEmailUpdateDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new GlobalAppException("INVALID_EMAIL");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null && existingUser.Id != user.Id)
            throw new GlobalAppException("EMAIL_ALREADY_EXISTS");

        user.PendingEmail = dto.Email;
        user.PendingEmailExpireAt = DateTime.UtcNow.AddHours(1);
        user.EmailConfirmed = false;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new GlobalAppException("USER_UPDATE_FAILED");

        var oldCodes = await _otpCodeReadRepository.GetAllAsync(x =>
            x.UserId == user.Id &&
            x.Purpose == "EmailVerification" &&
            !x.IsUsed);

        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
            await _otpCodeWriteRepository.UpdateAsync(oldCode);
        }

        var otpCode = GenerateOtpCode();

        var otpEntity = new OtpCode
        {
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber!,
            Code = otpCode,
            Purpose = "EmailVerification",
            Target = dto.Email,
            ExpireAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            AttemptCount = 0
        };

        await _otpCodeWriteRepository.AddAsync(otpEntity);
        await _otpCodeWriteRepository.CommitAsync();

        var mailrequest = new MailRequest
        {
            ToEmail = user.PendingEmail,
            Subject = "Email verification",
            Body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Email Verification</title>
                </head>
                <body style='margin:0;padding:0;font-family:Arial, sans-serif;background-color:#f4f4f4;'>

                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#f4f4f4;padding:20px 0;'>
                        <tr>
                            <td align='center'>
                                
                                <table width='400' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:8px;padding:30px;text-align:center;'>

                                    <tr>
                                        <td style='font-size:20px;font-weight:bold;color:#333;'>
                                            Email Verification
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:15px 0;color:#555;font-size:14px;'>
                                            Please use the code below to verify your email address.
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='padding:20px 0;'>
                                            <div style='display:inline-block;
                                                        padding:15px 25px;
                                                        font-size:24px;
                                                        font-weight:bold;
                                                        color:#000;
                                                        background:#f2f2f2;
                                                        border-radius:6px;
                                                        letter-spacing:4px;'>
                                                {otpCode}
                                            </div>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='color:#888;font-size:12px;padding-top:10px;'>
                                            This code will expire in 1 hour.
                                        </td>
                                    </tr>

                                    <tr>
                                        <td style='color:#aaa;font-size:11px;padding-top:20px;'>
                                            If you didn’t request this, you can safely ignore this email.
                                        </td>
                                    </tr>

                                </table>

                            </td>
                        </tr>
                    </table>

                </body>
                </html>
                "
        };
        await _mailService.SendEmailAsync(mailrequest);


        return true;
    }

    public async Task<bool> VerifyEmailOtpAsync(string userId, VerifyEmailOtpDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(user.PendingEmail) || user.PendingEmailExpireAt == null)
            throw new GlobalAppException("EMAIL_UPDATE_NOT_FOUND");

        if (user.PendingEmailExpireAt < DateTime.UtcNow)
        {
            user.PendingEmail = null;
            user.PendingEmailExpireAt = null;
            await _userManager.UpdateAsync(user);
            throw new GlobalAppException("EMAIL_OTP_EXPIRED");
        }

        await VerifyOtpCoreAsync(user.Id, "EmailVerification", dto.Code);

        user.Email = user.PendingEmail;
        user.NormalizedEmail = user.PendingEmail.ToUpper();
        user.EmailConfirmed = true;
        user.PendingEmail = null;
        user.PendingEmailExpireAt = null;

        if (string.IsNullOrWhiteSpace(user.UserName))
            user.UserName = Guid.NewGuid().ToString();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new GlobalAppException("EMAIL_VERIFY_FAILED");

        return true;
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

    private async Task CreateAndSendPhoneOtpAsync(User user)
    {
        var oldCodes = await _otpCodeReadRepository.GetAllAsync(x =>
            x.UserId == user.Id &&
            x.Purpose == "PhoneVerification" &&
            !x.IsUsed);

        foreach (var oldCode in oldCodes)
        {
            oldCode.IsUsed = true;
            await _otpCodeWriteRepository.UpdateAsync(oldCode);
        }

        var otpCode = GenerateOtpCode();

        var otpEntity = new OtpCode
        {
            UserId = user.Id,
            PhoneNumber = user.PhoneNumber!,
            Code = otpCode,
            Purpose = "PhoneVerification",
            ExpireAt = DateTime.UtcNow.AddMinutes(3),
            IsUsed = false,
            AttemptCount = 0
        };

        await _otpCodeWriteRepository.AddAsync(otpEntity);
        await _otpCodeWriteRepository.CommitAsync();

        var sent = await _otpSenderService.SendOtpAsync(user.PhoneNumber!, otpCode);
        if (!sent)
            throw new GlobalAppException("OTP_SEND_FAILED");
    }

    private async Task VerifyOtpCoreAsync(string userId, string purpose, string code)
    {
        var otp = await _otpCodeReadRepository.GetAsync(x =>
            x.UserId == userId &&
            x.Purpose == purpose &&
            !x.IsUsed);

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

    private async Task DeleteUserWithOtpCodesAsync(User user)
    {
        var otpCodes = await _otpCodeReadRepository.GetAllAsync(x => x.UserId == user.Id);

        foreach (var otp in otpCodes)
        {
            await _otpCodeWriteRepository.HardDeleteAsync(otp);
        }

        await _otpCodeWriteRepository.CommitAsync();

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
            throw new GlobalAppException("DUPLICATE_USER_DELETE_FAILED");
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