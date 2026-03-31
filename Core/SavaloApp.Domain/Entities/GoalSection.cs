using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class GoalSection:BaseEntity
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public List<Goal>? Goals { get; set; }
}