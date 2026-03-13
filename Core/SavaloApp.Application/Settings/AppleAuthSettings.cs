namespace SavaloApp.Application.Settings;

public class AppleAuthSettings
{
    public string ValidIssuer { get; set; } = "https://appleid.apple.com";
    public string Audience { get; set; } = null!;
    public string JwksUrl { get; set; } = "https://appleid.apple.com/auth/keys";
}