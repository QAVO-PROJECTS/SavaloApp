using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.CurrencyAccount;

public class CreateCurrencyAccountDto
{
    public string Name { get; set; }
    public IFormFile Icon { get; set; }
}