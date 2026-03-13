using Microsoft.AspNetCore.Identity;

namespace SavaloApp.Domain.Entities;

public class User:IdentityUser
{
    public string FullName { get; set; }
    public string? GoogleProviderId { get; set; }
    public string? AppleProviderId { get; set; }   // sub
    public string? OAuthProvider { get; set; } 
    public string TimeZone { get; set; }
    public string Language { get; set; }
    public string AccountType { get; set; }
    public string? ProfileImage { get; set; }
    public List<CurrencyAccount>? CurrencyAccounts { get; set; }
    public List<Review>? Reviews { get; set; }
    public List<RefreshToken>? RefreshTokens { get; set; }
    
}