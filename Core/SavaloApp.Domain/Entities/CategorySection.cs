using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class CategorySection:BaseEntity
{
    public string Name { get; set; }
    public string Icon { get; set; }
}