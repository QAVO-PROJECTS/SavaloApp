namespace SavaloApp.Application.Dtos.AdminAuth;

public class AdminLoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
}