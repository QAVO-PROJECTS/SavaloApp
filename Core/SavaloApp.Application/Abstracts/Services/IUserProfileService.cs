

using SavaloApp.Application.Dtos.UserAuth;

namespace SavaloApp.Application.Abstracts.Services;

public interface IUserProfileService
{
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
}