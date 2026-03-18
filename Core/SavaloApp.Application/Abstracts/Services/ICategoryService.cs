using SavaloApp.Application.Dtos.Category;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(string userId);
    Task<CategoryDto> GetByIdAsync(string id, string userId);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, string userId);
    Task UpdateAsync(UpdateCategoryDto dto, string userId);
    Task DeleteAsync(string id, string userId);
}