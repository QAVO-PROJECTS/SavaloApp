using SavaloApp.Domain.Entities.Common;

namespace SavaloApp.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }
    public decimal Amount { get; set; }

    public Guid CurrencyAccountId { get; set; }
    public CurrencyAccount? CurrencyAccount { get; set; }

    public string Repeat { get; set; }
    public DateTime? StartDate { get; set; }

    public string ColorCode { get; set; }

    public Guid IconId { get; set; }
    public Icon? Icon { get; set; }

    public List<CategoryTransaction>? CategoryTransactions { get; set; }
}