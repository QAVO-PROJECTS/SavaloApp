namespace SavaloApp.Application.Abstracts.Services;

public interface IOtpSenderService
{
    Task<bool> SendOtpAsync(string phoneNumber, string otp);
}