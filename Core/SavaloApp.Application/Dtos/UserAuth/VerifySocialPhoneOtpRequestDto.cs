namespace SavaloApp.Application.Dtos.UserAuth;

public class VerifySocialPhoneOtpRequestDto
{
    public string TempToken { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Code { get; set; } = null!;
}