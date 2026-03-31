using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class Goal : BaseEntity
{
    public string Name { get; set; }
    public decimal? Amount { get; set; }
    public Guid? GoalSectionId { get; set; }
    public GoalSection? GoalSection { get; set; }

    public Guid CurrencyAccountId { get; set; }
    public CurrencyAccount? CurrencyAccount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string ColorCode { get; set; }

    public Guid? IconId { get; set; }
    public Icon? Icon { get; set; }

    public List<GoalTransaction>? GoalTransactions { get; set; }
}