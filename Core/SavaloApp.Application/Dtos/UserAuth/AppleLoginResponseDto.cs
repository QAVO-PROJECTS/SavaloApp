namespace SavaloApp.Application.Dtos.UserAuth;

public class AppleLoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public AppleUserDto User { get; set; }
}

public class AppleUserDto
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public string FullName { get; set; }
    public string OAuthProvider { get; set; }
    public IList<string> Roles { get; set; }
}