using SavaloApp.Application.Dtos.CategorySection;

namespace SavaloApp.Application.Abstracts.Services;

public interface ICategorySectionService
{
    Task<CategorySectionDto> GetCategorySectionAsync(string id);
    Task<List<CategorySectionDto>> GetAllCategorySectionsAsync();
    Task<CategorySectionDto> CreateCategorySectionAsync(CreateCategorySectionDto dto);
    Task<CategorySectionDto> UpdateCategorySectionAsync(UpdateCategorySectionDto dto);
    Task DeleteCategorySectionAsync(string id);
}