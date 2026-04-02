namespace SavaloApp.Application.Dtos.Category;

public class UpdateCategoryDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public RepeatType? RepeatType { get; set; }
    public  string? StartDate { get; set; }
    public string? ColorCode { get; set; }
    public string? IconId { get; set; }
 
}