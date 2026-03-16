using Microsoft.AspNetCore.Http;

namespace SavaloApp.Application.Dtos.UserAuth;

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
}