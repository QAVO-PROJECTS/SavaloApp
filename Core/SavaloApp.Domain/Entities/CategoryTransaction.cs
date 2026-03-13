using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class CategoryTransaction:BaseEntity
{
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string TransactionName { get; set; }
    
}