namespace SavaloApp.Application.Abstracts.Services;

public interface ISmsService
{
    Task<bool> SendOtpAsync(string phoneNumber, string otpCode);
}