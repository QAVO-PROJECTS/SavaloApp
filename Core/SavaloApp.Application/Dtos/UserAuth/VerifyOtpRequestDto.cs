namespace SavaloApp.Application.Dtos.UserAuth;

public class VerifyOtpRequestDto
{
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
}