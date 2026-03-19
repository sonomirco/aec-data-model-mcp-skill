using Autodesk.Authentication.Model;

namespace apsMcp.Tools.Models;

/// <summary>
/// Internal wrapper for caching token with its absolute expiration
/// </summary>
sealed class CachedToken
{
    public ThreeLeggedToken Token { get; set; } = default!;
    public DateTime Expiration { get; set; }
}
