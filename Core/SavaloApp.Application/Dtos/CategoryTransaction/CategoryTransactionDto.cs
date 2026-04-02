using SavaloApp.Application.Dtos.Category;

namespace SavaloApp.Application.Dtos.CategoryTransaction;

public class CategoryTransactionDto
{
    public string Id { get; set; }
    public CategoryDto Category { get; set; }
    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string TransactionName { get; set; }
}