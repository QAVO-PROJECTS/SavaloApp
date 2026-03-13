using System.Security.Claims;

namespace SavaloApp.Application.Abstracts.Services;

public interface IAppleTokenValidator
{
    Task<ClaimsPrincipal> ValidateIdentityTokenAsync(string identityToken, CancellationToken cancellationToken = default);
}