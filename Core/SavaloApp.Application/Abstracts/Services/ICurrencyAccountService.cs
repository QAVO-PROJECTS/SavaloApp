using SavaloApp.Application.Dtos.CurrencyAccount;

namespace SavaloApp.Application.Abstracts.Services;

public interface ICurrencyAccountService
{
    Task<List<CurrencyAccountDto>> GetAllCurrenciesAsync(string userId);
    Task<CurrencyAccountDto> GetCurrencyAsync(string id, string userId);
    Task<CurrencyAccountDto> CreateCurrencyAsync(CreateCurrencyAccountDto dto, string userId);
    Task UpdateCurrencyAsync(UpdateCurrencyAccountDto dto, string userId);
    Task DeleteCurrencyAsync(string id, string userId);
}