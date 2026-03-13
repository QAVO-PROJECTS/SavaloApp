using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class CurrencyAccount : BaseEntity
{
    public string Name { get; set; }
    public string Icon { get; set; }

    public string UserId { get; set; }
    public User? User { get; set; }

    public List<Category>? Categories { get; set; }
    public List<Goal>? Goals { get; set; }
}