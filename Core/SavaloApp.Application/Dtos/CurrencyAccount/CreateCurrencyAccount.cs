using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.CurrencyAccount;

public class CreateCurrencyAccount
{
    public string Name { get; set; }
    public IFormFile Icon { get; set; }
}