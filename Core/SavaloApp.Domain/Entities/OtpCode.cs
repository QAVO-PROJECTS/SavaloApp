using SavaloApp.Domain.Entities.Common;



namespace SavaloApp.Domain.Entities;

public class OtpCode : BaseEntity
{
    public string UserId { get; set; }
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
    public string Purpose { get; set; } // PhoneVerification, EmailVerification
    public string? Target { get; set; } // email üçün istifadə oluna bilər
    public DateTime ExpireAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
}