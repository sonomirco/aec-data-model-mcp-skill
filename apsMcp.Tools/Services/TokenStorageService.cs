using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Autodesk.Authentication.Model;
using ApsMcp.Tools.Models;

namespace ApsMcp.Tools.Services;

public class TokenStorageService(IMemoryCache cache, AuthService authenticator, IServiceProvider serviceProvider)
{
    private readonly IMemoryCache _cache = cache;
    private readonly AuthService _authenticator = authenticator;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private const string TokenKey = "APS_TOKEN";

    public async Task<ThreeLeggedToken?> GetToken(CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue<CachedToken>(TokenKey, out var cachedToken);

        // If the token is close to expiration (within 30 minutes), refresh it
        if (cachedToken != null && cachedToken.Expiration <= DateTime.Now.AddMinutes(30))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var newToken = await _authenticator.RefreshTokenAsync(cachedToken.Token.RefreshToken)
                ?? throw new InvalidOperationException("Failed to refresh token");
                StoreToken(newToken);
                return newToken;
            }
            catch
            {
                // Token refresh failed - clear token
                ClearToken();
                throw;
            }
        }

        return cachedToken?.Token;
    }

    public void StoreToken(ThreeLeggedToken token)
    {
        // Calculate expiration time - use ExpiresIn (seconds from now) instead of ExpiresAt
        var expiration = DateTime.Now.AddSeconds(token.ExpiresIn ?? 3600);
        var cachedToken = new CachedToken { Token = token, Expiration = expiration };
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration);

        _cache.Set(TokenKey, cachedToken, cacheOptions);
    }
    
    public void ClearToken()
    {
        _cache.Remove(TokenKey);
    }
}
