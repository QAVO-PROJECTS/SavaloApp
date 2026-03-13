using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SavaloApp.Application.Abstracts.Repositories.RefreshTokens;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Dtos.AdminAuth;
using SavaloApp.Application.Dtos.UserAuth;
using SavaloApp.Application.GlobalException;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Persistance.Concretes.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly IRefreshTokenReadRepository _refreshTokenReadRepository;
    private readonly IRefreshTokenWriteRepository _refreshTokenWriteRepository;
    private readonly SigningCredentials _signingCredentials;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _signingKey;

    public TokenService(
        IConfiguration configuration,
        UserManager<User> userManager,
        IRefreshTokenReadRepository refreshTokenReadRepository,
        IRefreshTokenWriteRepository refreshTokenWriteRepository)
    {
        _configuration = configuration;
        _userManager = userManager;
        _refreshTokenReadRepository = refreshTokenReadRepository;
        _refreshTokenWriteRepository = refreshTokenWriteRepository;

        _issuer = _configuration["Jwt:Issuer"] ?? throw new Exception("Jwt:Issuer NOT_FOUND");
        _audience = _configuration["Jwt:Audience"] ?? throw new Exception("Jwt:Audience NOT_FOUND");
        _signingKey = _configuration["Jwt:SigningKey"] ?? throw new Exception("Jwt:SigningKey NOT_FOUND");

   

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public async Task<string> GenerateAccessTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
//
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var userClaims = await _userManager.GetClaimsAsync(user);
  
        var expires = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, string? ipAddress = null)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var entity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = ipAddress
        };

        await _refreshTokenWriteRepository.AddAsync(entity);
        await _refreshTokenWriteRepository.CommitAsync();

        return refreshToken;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey)),
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("INACTIVE_TOKEN_ALGORITHM");
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<AdminLoginResponseDto> RefreshTokenAsyncForAdmin(string refreshToken, string? ipAddress = null)
    {
        var tokenEntity = await _refreshTokenReadRepository
                .GetAsync(x => x.Token == refreshToken)
            ;

        if (tokenEntity == null)
            throw new GlobalAppException("NOT_FOUND_REFRESH_TOKEN");

        if (!tokenEntity.IsActive)
            throw new GlobalAppException("INACTIVE_REFRESH_TOKEN");

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReplacedByToken = newRefreshToken;

        await _refreshTokenWriteRepository.UpdateAsync(tokenEntity);

        var newTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = ipAddress
        };

        await _refreshTokenWriteRepository.AddAsync(newTokenEntity);
        await _refreshTokenWriteRepository.CommitAsync();

        var accessToken = await GenerateAccessTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AdminLoginResponseDto()
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
        
        };
    }
    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var tokenEntity = await _refreshTokenReadRepository
            .GetAsync(x => x.Token == refreshToken)
            ;

        if (tokenEntity == null)
            throw new GlobalAppException("NOT_FOUND_REFRESH_TOKEN");

        if (!tokenEntity.IsActive)
            throw new GlobalAppException("INACTIVE_REFRESH_TOKEN");

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString());
        if (user == null)
            throw new GlobalAppException("USER_NOT_FOUND");

        var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReplacedByToken = newRefreshToken;

        await _refreshTokenWriteRepository.UpdateAsync(tokenEntity);

        var newTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedByIp = ipAddress
        };

        await _refreshTokenWriteRepository.AddAsync(newTokenEntity);
        await _refreshTokenWriteRepository.CommitAsync();

        var accessToken = await GenerateAccessTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
        
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var tokenEntity = await _refreshTokenReadRepository
            .GetAsync(x => x.Token == refreshToken)
          ;

        if (tokenEntity == null)
            throw new GlobalAppException("NOT_FOUND_REFRESH_TOKEN");

        if (!tokenEntity.IsActive)
            throw new GlobalAppException("INACTIVE_REFRESH_TOKEN");

        tokenEntity.RevokedAt = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;

        await _refreshTokenWriteRepository.UpdateAsync(tokenEntity);
        await _refreshTokenWriteRepository.CommitAsync();
    }
}