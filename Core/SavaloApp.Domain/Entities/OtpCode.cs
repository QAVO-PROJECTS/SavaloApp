using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class OtpCode : BaseEntity
{
    public string PhoneNumber { get; set; }
    public string Code { get; set; }
    public DateTime ExpireAt { get; set; }
    public bool IsUsed { get; set; }
    public int AttemptCount { get; set; }
}