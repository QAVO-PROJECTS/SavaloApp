

namespace SavaloApp.Application.Dtos.UserAuth;

public class SocialAuthResponseDto
{
    public bool RequiresPhoneVerification { get; set; }
    public string? TempToken { get; set; }
    public LoginResponseDto? Auth { get; set; }
}