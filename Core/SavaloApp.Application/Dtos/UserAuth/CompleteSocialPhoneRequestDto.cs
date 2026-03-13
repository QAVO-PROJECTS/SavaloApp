namespace SavaloApp.Application.Dtos.UserAuth;

public class CompleteSocialPhoneRequestDto
{
    public string TempToken { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}