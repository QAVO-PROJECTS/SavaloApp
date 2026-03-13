using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.GoalSection;

public class UpdateGoalSectionDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public IFormFile? Icon { get; set; }
}