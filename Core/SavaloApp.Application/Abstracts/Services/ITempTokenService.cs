using System.Security.Claims;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Abstracts.Services;

public interface ITempTokenService
{
    Task<string> GenerateTempTokenAsync(User user, string provider);
    ClaimsPrincipal? ValidateTempToken(string token);
}