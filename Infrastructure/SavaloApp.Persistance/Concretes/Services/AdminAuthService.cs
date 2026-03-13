using Microsoft.AspNetCore.Identity;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.AdminAuth;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;


namespace SavaloApp.Persistance.Concretes.Services;

public class AdminAuthService : IAdminAuthService
{
    private const string ADMIN_ROLE = "Admin";


    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public AdminAuthService(
        UserManager<User> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AdminLoginResponseDto> LoginAsync(AdminLoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new GlobalAppException("INVALID_CREDENTIALS");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(ADMIN_ROLE) )
            throw new GlobalAppException("ADMIN_PORTAL_ACCESS_DENIED");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw new GlobalAppException("INVALID_CREDENTIALS");

  


        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);


        await _userManager.UpdateAsync(user);

        return new AdminLoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600
        };
    }
    public async Task<AdminLoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new GlobalAppException("REFRESH_TOKEN_REQUIRED");

        return await _tokenService.RefreshTokenAsyncForAdmin(refreshToken, ipAddress);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new GlobalAppException("REFRESH_TOKEN_REQUIRED");

        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);
    }
    
}