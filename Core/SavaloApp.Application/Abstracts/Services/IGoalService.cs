using SavaloApp.Application.Dtos.Goal;

namespace SavaloApp.Application.Abstracts.Services;

public interface IGoalService
{
    Task<List<GoalDto>> GetAllAsync(string userId);
    Task<GoalDto> GetByIdAsync(string id, string userId);
    Task<GoalDto> CreateAsync(CreateGoalDto dto, string userId);
    Task UpdateAsync(UpdateGoalDto dto, string userId);
    Task DeleteAsync(string id, string userId);
}