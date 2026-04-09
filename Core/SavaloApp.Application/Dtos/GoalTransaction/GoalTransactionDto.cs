using SavaloApp.Application.Dtos.Goal;

namespace SavaloApp.Application.Dtos.GoalTransaction;

public class GoalTransactionDto
{
    public string Id { get; set; }
    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
    public GoalDto Goal { get; set; }
}