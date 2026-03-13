using SavaloApp.Application.Dtos.GoalSection;

namespace SavaloApp.Application.Abstracts.Services;

public interface IGoalSectionService
{
    Task<GoalSectionDto> GetGoalSectionAsync(string id);
    Task<List<GoalSectionDto>> GetAllGoalSectionsAsync();
    Task<GoalSectionDto> CreateGoalSectionAsync(CreateGoalSectionDto dto);
    Task<GoalSectionDto> UpdateGoalSectionAsync(UpdateGoalSectionDto dto);
    Task DeleteGoalSectionAsync(string id);
}