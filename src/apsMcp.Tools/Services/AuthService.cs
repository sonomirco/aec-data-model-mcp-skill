using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Autodesk.Authentication.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apsMcp.Tools.Services;

/// <summary>
/// Handles the authentication process using a three-legged PKCE flow.
/// Useful links
/// <see href="https://chuongmep.com/posts/2024-05-07-get-3leg-aps-with-csharp.html"/>
/// <see href="https://github.com/chuongmep/aps-tookit-auth-3leg-pkce/blob/master/aps-tookit-auth-3leg-pkce/MainWindow.xaml.cs"/>
/// </summary>
public class AuthService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly Uri _callbackUri;
    private readonly string _callbackUrl;
    private readonly string _listenerPrefix;
    private readonly string[] _scopes;
    private readonly ILogger<AuthService> _logger;

    public AuthService(string clientId, string clientSecret, string callbackUrl, string[] scopes, ILogger<AuthService> logger)
    {
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _callbackUri = ValidateCallbackUrl(callbackUrl);
        _callbackUrl = _callbackUri.AbsoluteUri;
        _listenerPrefix = BuildListenerPrefix(_callbackUri);
        _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the three-legged PKCE authentication flow.
    /// </summary>
    public async Task<ThreeLeggedToken> AuthenticateAsync()
    {
        _logger.LogInformation("Starting 3-legged authentication flow.");

        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        string authUrl = $"https://developer.api.autodesk.com/authentication/v2/authorize" +
                         $"?response_type=code" +
                         $"&client_id={Uri.EscapeDataString(_clientId)}" +
                         $"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}" +
                         $"&scope={Uri.EscapeDataString(string.Join(" ", _scopes))}" +
                         $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                         $"&code_challenge_method=S256";

        _logger.LogDebug("Auth URL: {AuthUrl}", authUrl);
        _logger.LogInformation("Opening browser for user authentication.");

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open browser for authentication.");
            throw;
        }

        using HttpListener listener = new();
        listener.Prefixes.Add(_listenerPrefix);
        listener.Start();
        _logger.LogInformation("Listening for callback on {ListenerPrefix}", _listenerPrefix);

        HttpListenerContext? context = null;
        while (true)
        {
            context = await listener.GetContextAsync();
            if (IsMatchingCallback(context.Request.Url, _callbackUri))
            {
                break;
            }

            _logger.LogWarning(
                "Ignored authentication callback request on path {RequestPath}. Expected path {ExpectedPath}.",
                context.Request.Url?.AbsolutePath,
                _callbackUri.AbsolutePath);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
        }

        string? authCode = context.Request.QueryString["code"];
        if (string.IsNullOrEmpty(authCode))
        {
            _logger.LogError("Authorization code was not received or is invalid.");
            throw new InvalidOperationException("Invalid authorization code received.");
        }

        string responseHtml = "<html><head><title>Authentication Complete</title></head>" +
                              "<body><h2>Authentication successful. You may close this window.</h2></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();

        _logger.LogInformation("Exchanging authorization code for tokens.");
        try
        {
            var token = await ExchangeCodeForTokenAsync(authCode, codeVerifier);
            _logger.LogInformation("Authentication flow completed successfully.");
            return token;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogError(ex, "Failed to exchange authorization code for tokens.");
            throw new InvalidOperationException("Failed to exchange authorization code for tokens.", ex);
        }
    }

    internal static Uri ValidateCallbackUrl(string callbackUrl)
    {
        if (string.IsNullOrWhiteSpace(callbackUrl))
        {
            throw new ArgumentException("Callback URL cannot be null or empty.", nameof(callbackUrl));
        }

        if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out var callbackUri))
        {
            throw new ArgumentException("Callback URL must be an absolute URI.", nameof(callbackUrl));
        }

        if (!string.Equals(callbackUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(callbackUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Callback URL must use http or https.", nameof(callbackUrl));
        }

        if (!string.IsNullOrEmpty(callbackUri.Query) || !string.IsNullOrEmpty(callbackUri.Fragment))
        {
            throw new ArgumentException("Callback URL cannot include query string or fragment.", nameof(callbackUrl));
        }

        return callbackUri;
    }

    internal static string BuildListenerPrefix(Uri callbackUri)
    {
        var callbackPath = callbackUri.AbsolutePath;
        var listenerPath = callbackPath.EndsWith("/", StringComparison.Ordinal)
            ? callbackPath
            : callbackPath[..(callbackPath.LastIndexOf('/') + 1)];

        if (string.IsNullOrWhiteSpace(listenerPath))
        {
            listenerPath = "/";
        }

        if (!listenerPath.StartsWith("/", StringComparison.Ordinal))
        {
            listenerPath = "/" + listenerPath;
        }

        return $"{callbackUri.Scheme}://{callbackUri.Authority}{listenerPath}";
    }

    private static bool IsMatchingCallback(Uri? requestUri, Uri callbackUri)
    {
        if (requestUri is null)
        {
            return false;
        }

        var expectedPath = callbackUri.AbsolutePath.TrimEnd('/');
        var requestPath = requestUri.AbsolutePath.TrimEnd('/');
        return string.Equals(requestPath, expectedPath, StringComparison.OrdinalIgnoreCase);
    }

    internal static string GenerateCodeVerifier(int byteLength = 64)
    {
        if (byteLength is < 32 or > 96)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length must produce RFC 7636 verifier length between 43 and 128 characters.");
        }

        byte[] randomBytes = RandomNumberGenerator.GetBytes(byteLength);
        var verifier = Base64UrlEncode(randomBytes);
        if (verifier.Length is < 43 or > 128)
        {
            throw new InvalidOperationException("Generated PKCE verifier is outside RFC 7636 length bounds.");
        }

        return verifier;
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private async Task<ThreeLeggedToken> ExchangeCodeForTokenAsync(string authCode, string codeVerifier)
    {
        using HttpClient httpClient = new();
        var postData = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "authorization_code" },
                { "code", authCode },
                { "scope", string.Join(" ", _scopes) },
                { "redirect_uri", _callbackUrl },
                { "code_verifier", codeVerifier }
            };

        var content = new FormUrlEncodedContent(postData);
        HttpResponseMessage response = await httpClient.PostAsync("https://developer.api.autodesk.com/authentication/v2/token", content);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new InvalidOperationException("Received null or empty json response");
        }

        JObject bodyjson = JObject.Parse(jsonResponse);
        return new ThreeLeggedToken()
        {
            AccessToken = bodyjson["access_token"]?.Value<string>() ?? throw new InvalidOperationException("Access token not found in response"),
            ExpiresIn = bodyjson["expires_in"]?.Value<int>() ?? throw new InvalidOperationException("Expires in not found in response"),
            TokenType = bodyjson["token_type"]?.Value<string>() ?? throw new InvalidOperationException("Token type not found in response"),
            RefreshToken = bodyjson["refresh_token"]?.Value<string>() ?? throw new InvalidOperationException("Refresh token not found in response")
        };
    }

    public async Task<ThreeLeggedToken> RefreshTokenAsync(string refreshToken)
    {
        using HttpClient httpClient = new();
        var postData = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "client_secret", _clientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "scope", string.Join(" ", _scopes) }
        };

        var content = new FormUrlEncodedContent(postData);
        HttpResponseMessage response = await httpClient.PostAsync("https://developer.api.autodesk.com/authentication/v2/token", content);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(jsonResponse))
        {
            throw new InvalidOperationException("Received null or empty json response");
        }

        JObject bodyjson = JObject.Parse(jsonResponse);
        return new ThreeLeggedToken()
        {
            AccessToken = bodyjson["access_token"]?.Value<string>() ?? throw new InvalidOperationException("Access token not found in response"),
            ExpiresIn = bodyjson["expires_in"]?.Value<int>() ?? throw new InvalidOperationException("Expires in not found in response"),
            TokenType = bodyjson["token_type"]?.Value<string>() ?? throw new InvalidOperationException("Token type not found in response"),
            RefreshToken = bodyjson["refresh_token"]?.Value<string>() ?? throw new InvalidOperationException("Refresh token not found in response")
        };
    }

    /// <summary>
    /// Ensures the user is authenticated by checking for a valid token and authenticating if needed.
    /// This method works with TokenStorageService to handle the complete authentication flow.
    /// </summary>
    /// <param name="tokenStorage">The token storage service to check for existing tokens</param>
    public async Task EnsureAuthenticatedAsync(TokenStorageService tokenStorage)
    {
        try
        {
            var token = await tokenStorage.GetToken();

            if (token == null)
            {
                await ReAuthenticateAsync(tokenStorage);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Token retrieval failed due to HTTP error. Falling back to full authentication.");
            await ReAuthenticateAsync(tokenStorage);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Token retrieval failed due to invalid state. Falling back to full authentication.");
            await ReAuthenticateAsync(tokenStorage);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Token payload parsing failed. Falling back to full authentication.");
            await ReAuthenticateAsync(tokenStorage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure while ensuring authentication continuity.");
            throw;
        }
    }

    private async Task ReAuthenticateAsync(TokenStorageService tokenStorage)
    {
        var newToken = await AuthenticateAsync();
        tokenStorage.StoreToken(newToken);
    }
}

