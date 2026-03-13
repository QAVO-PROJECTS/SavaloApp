using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class GoalTransaction : BaseEntity
{
    public Guid GoalId { get; set; }
    public Goal? Goal { get; set; }

    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
}