using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.GlobalException;
using SavaloApp.Application.Settings;
using SavaloApp.Persistance.External.Apple;

namespace SavaloApp.Persistance.Concretes.Services;

public class AppleTokenValidator : IAppleTokenValidator
{
    private const string CacheKey = "apple_jwks_keys";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly AppleAuthSettings _settings;

    public AppleTokenValidator(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<AppleAuthSettings> settings)
    {
        _httpClient = httpClient;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<ClaimsPrincipal> ValidateIdentityTokenAsync(string identityToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityToken))
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(identityToken))
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var jwt = handler.ReadJwtToken(identityToken);
        var kid = jwt.Header.Kid;
        if (string.IsNullOrWhiteSpace(kid))
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var keys = await GetSigningKeysAsync(cancellationToken);
        var key = keys.FirstOrDefault(x => x.KeyId == kid);

        if (key == null)
        {
            _cache.Remove(CacheKey);
            keys = await GetSigningKeysAsync(cancellationToken, true);
            key = keys.FirstOrDefault(x => x.KeyId == kid);
        }

        if (key == null)
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        try
        {
            var principal = handler.ValidateToken(identityToken, parameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken validatedJwt)
                throw new GlobalAppException("INVALID_APPLE_TOKEN");

            if (!string.Equals(validatedJwt.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
                throw new GlobalAppException("INVALID_APPLE_TOKEN");

            return principal;
        }
        catch
        {
            throw new GlobalAppException("INVALID_APPLE_TOKEN");
        }
    }

    private async Task<List<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        if (!forceRefresh && _cache.TryGetValue(CacheKey, out List<SecurityKey>? cached) && cached is not null)
            return cached;

        using var response = await _httpClient.GetAsync(_settings.JwksUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var jwks = JsonSerializer.Deserialize<AppleJwksResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (jwks?.Keys == null || jwks.Keys.Count == 0)
            throw new GlobalAppException("INVALID_APPLE_TOKEN");

        var keys = jwks.Keys.Select(CreateKey).ToList();
        _cache.Set(CacheKey, keys, TimeSpan.FromHours(12));
        return keys;
    }

    private static SecurityKey CreateKey(AppleJwk jwk)
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
            Exponent = Base64UrlEncoder.DecodeBytes(jwk.E)
        });

        return new RsaSecurityKey(rsa) { KeyId = jwk.Kid };
    }
}