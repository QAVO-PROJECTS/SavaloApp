using SavaloApp.Application.Dtos.CategoryTransaction;

namespace SavaloApp.Application.Abstracts.Services;

public interface ICategoryTransactionService
{
    Task<List<CategoryTransactionDto>> GetAllAsync(string userId);
    Task<List<CategoryTransactionDto>> GetAllForCategoryAsync(string categoryId,string userId);
    Task<CategoryTransactionDto> GetByIdAsync(string id, string userId);
    Task<CategoryTransactionDto> CreateAsync(CreateCategoryTransactionDto dto, string userId);

    Task DeleteAsync(string id, string userId);
}