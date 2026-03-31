using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.CurrencyAccount;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class UserProfileService : IUserProfileService
{
    private const string AvatarFolder = "avatars";

    private readonly UserManager<User> _userManager;
    private readonly IOtpSenderService _otpSenderService;
    private readonly IOtpCodeReadRepository _otpCodeReadRepository;
    private readonly IOtpCodeWriteRepository _otpCodeWriteRepository;
    private readonly IFileService _fileService;

    public UserProfileService(
        UserManager<User> userManager,
        IOtpSenderService otpSenderService,
        IOtpCodeReadRepository otpCodeReadRepository,
        IOtpCodeWriteRepository otpCodeWriteRepository,
        IFileService fileService)
    {
        _userManager = userManager;
        _otpSenderService = otpSenderService;
        _otpCodeReadRepository = otpCodeReadRepository;
        _otpCodeWriteRepository = otpCodeWriteRepository;
        _fileService = fileService;
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new GlobalAppException("INVALID_USER_ID");

        var user = await _userManager.Users
            .Include(x => x.CurrencyAccounts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        return new UserProfileDto
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.ProfileImage,
            TimeZone = user.TimeZone ?? "Asia/Baku",
            Language = user.Language ?? "az",
            Currencies = user.CurrencyAccounts.Select(x => new CurrencyAccountDto
            {
                Id=x.Id.ToString(),
                Name = x.Name,
                Icon = x.Icon
            }).ToList()
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new GlobalAppException("INVALID_USER_ID");

        if (dto == null)
            throw new GlobalAppException("INVALID_INPUT");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        var phoneChanged = false;
        var newPhone = dto.PhoneNumber?.Trim();

        if (!string.IsNullOrWhiteSpace(newPhone) && user.PhoneNumber != newPhone)
        {
            var existByPhone = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == newPhone && x.Id != user.Id);

            if (existByPhone != null)
                throw new GlobalAppException("PHONE_NUMBER_ALREADY_EXISTS");

            user.PhoneNumber = newPhone;
            user.UserName = newPhone;
            user.PhoneNumberConfirmed = false;
            phoneChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(dto.TimeZone))
            user.TimeZone = dto.TimeZone.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Language))
            user.Language = dto.Language.Trim();

        if (dto.Avatar != null && dto.Avatar.Length > 0)
            user.ProfileImage = await _fileService.UploadFile(dto.Avatar, AvatarFolder);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new GlobalAppException("PROFILE_UPDATE_FAILED");

        if (phoneChanged)
            await CreateAndSendOtpAsync(user.PhoneNumber!);

        return new UserProfileDto
        {
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.ProfileImage,
            TimeZone = user.TimeZone ?? "Asia/Baku",
            Language = user.Language ?? "az"
        };
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new GlobalAppException("INVALID_USER_ID");

        if (dto == null)
            throw new GlobalAppException("INVALID_INPUT");

        if (string.IsNullOrWhiteSpace(dto.OldPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword) ||
            string.IsNullOrWhiteSpace(dto.ConfirmPassword))
            throw new GlobalAppException("INVALID_INPUT");

        if (dto.NewPassword != dto.ConfirmPassword)
            throw new GlobalAppException("PASSWORD_CONFIRM_NOT_MATCH");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        if (!result.Succeeded)
            throw new GlobalAppException("PASSWORD_CHANGE_FAILED");
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

        var sent = await _otpSenderService.SendOtpAsync(phoneNumber, otpCode);
        if (!sent)
            throw new GlobalAppException("OTP_SEND_FAILED");
    }

    private static string GenerateOtpCode()
        => System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
}          