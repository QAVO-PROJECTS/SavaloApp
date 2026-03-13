using SavaloApp.Application.Dtos.AdminAuth;

namespace SavaloApp.Application.Abstracts.Services;

public interface IAdminAuthService
{
    Task<AdminLoginResponseDto> LoginAsync(AdminLoginRequestDto dto);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<AdminLoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
}