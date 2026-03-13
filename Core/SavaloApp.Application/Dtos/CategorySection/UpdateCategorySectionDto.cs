using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.CategorySection;

public class UpdateCategorySectionDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public IFormFile? Icon { get; set; }
}