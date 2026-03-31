using SavaloApp.Application.Dtos.CurrencyAccount;

namespace SavaloApp.Application.Dtos.UserAuth;


public class UserProfileDto
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string TimeZone { get; set; }
    public string Language { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public List<CurrencyAccountDto> Currencies { get; set; }
}