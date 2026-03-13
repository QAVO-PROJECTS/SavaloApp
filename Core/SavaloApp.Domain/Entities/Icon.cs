using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class Icon:BaseEntity
{
    public string IconName { get; set; }
    public List<Category>? Categories { get; set; }
    public List<Goal>? Goals { get; set; }
    
}