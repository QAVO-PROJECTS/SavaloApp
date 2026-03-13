using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class RefreshToken:BaseEntity
{
    public string UserId { get; set; }
    public User? User { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }    
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
   
}