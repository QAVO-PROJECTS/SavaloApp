using System.Security.Claims;
using SavaloApp.Application.Dtos.AdminAuth;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Abstracts.Services;

public interface ITokenService
{   
    Task<string> GenerateAccessTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync(User user, string? ipAddress = null);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<AdminLoginResponseDto> RefreshTokenAsyncForAdmin(string refreshToken, string? ipAddress = null);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null);
}