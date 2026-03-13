using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.Icon;

public class CreateIconDto
{
    public IFormFile Icon { get; set; }
}