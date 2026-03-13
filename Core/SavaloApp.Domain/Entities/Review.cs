using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class Review:BaseEntity
{
    public int Rating { get; set; }
    public string UserId { get; set; }
    public User? User { get; set; }
    public string? Text { get; set; }
}