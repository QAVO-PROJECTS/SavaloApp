namespace SavaloApp.Application.Dtos.UserAuth;

public class AppleLoginRequestDto
{
    public string IdentityToken { get; set; }
    public string? FullName { get; set; }
}