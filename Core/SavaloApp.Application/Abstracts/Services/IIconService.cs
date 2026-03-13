using SavaloApp.Application.Dtos.Icon;

namespace SavaloApp.Application.Abstracts.Services;

public interface IIconService
{
    Task<IconDto> GetIconAsync(string id);
    Task<List<IconDto>> GetAllIconsAsync();
    Task<IconDto> CreateIconAsync(CreateIconDto dto);
    Task DeleteIconAsync(string id);
}