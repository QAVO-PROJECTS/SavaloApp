namespace SavaloApp.Application.Dtos.CategoryTransaction;

public class UpdateCategoryTransactionDto
{
    public string Id { get; set; }
    public string? CategoryId { get; set; }

    public bool? TransactionType { get; set; }
    public decimal? Amount { get; set; }
    public string? TransactionName { get; set; }
}