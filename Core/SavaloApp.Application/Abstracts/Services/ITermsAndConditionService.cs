using SavaloApp.Application.Dtos.TermsAndCondition;

namespace SavaloApp.Application.Abstracts.Services;

public interface ITermsAndConditionService
{
    Task<TermsAndConditionDto> GetAsync();
    Task CreateAsync(CreateTermsAndConditionDto dto);
    Task UpdateAsync(UpdateTermsAndConditionDto dto);
    Task DeleteAsync(string id);
}