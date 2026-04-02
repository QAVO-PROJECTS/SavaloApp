namespace SavaloApp.Application.Dtos.CategoryTransaction;

public class CreateCategoryTransactionDto
{
    public string CategoryId { get; set; }

    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string TransactionName { get; set; }
}