namespace SavaloApp.Application.Dtos.Category;

public class CategoryDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public string? CategorySectionId { get; set; }
    public string? CategorySectionName { get; set; }
    public string? CategorySectionImage { get; set; }
    public string CurrencyAccountId { get; set; }
    public string RepeatType { get; set; }
    public  string StartDate { get; set; }
    public string ColorCode { get; set; }
    public string IconId { get; set; }
    public decimal TotalTransactionAmount { get; set; }
}