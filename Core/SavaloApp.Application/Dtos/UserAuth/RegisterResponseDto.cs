namespace SavaloApp.Application.Dtos.UserAuth;


public class RegisterResponseDto
{
    public string UserId { get; set; }
    public string PhoneNumber { get; set; }
    public bool OtpSent { get; set; }
    public string Message { get; set; }
}