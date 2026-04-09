namespace SavaloApp.Application.Dtos.GoalTransaction;

public class CreateGoalTransactionDto
{
    public string GoalId { get; set; }
    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
}