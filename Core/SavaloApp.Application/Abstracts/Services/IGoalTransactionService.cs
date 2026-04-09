using SavaloApp.Application.Dtos.GoalTransaction;

namespace SavaloApp.Application.Abstracts.Services;

public interface IGoalTransactionService
{
    Task<List<GoalTransactionDto>> GetAllAsync(string userId);
    Task<List<GoalTransactionDto>> GetAllForGoalAsync(string goalId, string userId);
    Task<GoalTransactionDto> GetByIdAsync(string id, string userId);
    Task<GoalTransactionDto> CreateAsync(CreateGoalTransactionDto dto, string userId);
    Task DeleteAsync(string id, string userId);
}