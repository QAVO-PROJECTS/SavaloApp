using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.CategorySection;

public class CreateCategorySectionDto
{
    public string Name { get; set; }
    public IFormFile Icon { get; set; }
}