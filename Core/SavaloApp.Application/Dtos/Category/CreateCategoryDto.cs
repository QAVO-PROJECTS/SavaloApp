namespace SavaloApp.Application.Dtos.Category;

public class CreateCategoryDto
{
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyAccountId { get; set; }
    public string? CategorySectionId { get; set; }
    public RepeatType? RepeatType { get; set; }
    public  string? StartDate { get; set; }
    public string? ColorCode { get; set; }
    public string? IconId { get; set; }
 
    
}
public enum RepeatType
{
    None,
    Daily,
    Weekly,
    Monthly,
    Yearly
}