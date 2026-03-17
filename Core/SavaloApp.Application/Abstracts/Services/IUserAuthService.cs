using SavaloApp.Application.Dtos.UserAuth;

namespace SavaloApp.Application.Abstracts.Services;

public interface IUserAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

    Task<bool> SendOtpAsync(SendOtpRequestDto dto);
    Task<LoginResponseDto> VerifyUserAsync(VerifyOtpRequestDto dto);

    Task<SocialAuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto dto);
    Task<SocialAuthResponseDto> AppleLoginAsync(AppleLoginRequestDto dto);

    Task<bool> CompleteSocialPhoneAsync(CompleteSocialPhoneRequestDto dto);
    Task<LoginResponseDto> VerifySocialPhoneOtpAsync(VerifySocialPhoneOtpRequestDto dto);

    Task<bool> StartEmailUpdateAsync(string userId, StartEmailUpdateDto dto);
    Task<bool> VerifyEmailOtpAsync(string userId, VerifyEmailOtpDto dto);

    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null);
}