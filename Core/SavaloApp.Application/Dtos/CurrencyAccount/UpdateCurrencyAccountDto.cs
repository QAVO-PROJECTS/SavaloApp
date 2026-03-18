using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.CurrencyAccount;

public class UpdateCurrencyAccountDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public IFormFile? Icon { get; set; }
}