using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.GoalSection;

public class CreateGoalSectionDto
{
    public string Name { get; set; }
    public IFormFile Icon { get; set; }
}