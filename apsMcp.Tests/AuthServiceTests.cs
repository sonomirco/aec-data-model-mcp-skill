using ApsMcp.Tools.Services;
using FluentAssertions;
using Xunit;

namespace apsMcp.Tests;

public class AuthServiceTests
{
    [Fact]
    public void ValidateCallbackUrl_rejects_querystring()
    {
        var action = () => AuthService.ValidateCallbackUrl("http://localhost:5096/callback?code=1");

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot include query string*");
    }

    [Fact]
    public void ValidateCallbackUrl_rejects_non_http_schemes()
    {
        var action = () => AuthService.ValidateCallbackUrl("ftp://localhost/callback");

        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*must use http or https*");
    }

    [Fact]
    public void BuildListenerPrefix_uses_callback_parent_path()
    {
        var callbackUri = new Uri("http://localhost:5096/api/auth/callback");

        var prefix = AuthService.BuildListenerPrefix(callbackUri);

        prefix.Should().Be("http://localhost:5096/api/auth/");
    }

    [Fact]
    public void BuildListenerPrefix_defaults_to_root_for_single_segment_callback()
    {
        var callbackUri = new Uri("http://localhost:5096/callback");

        var prefix = AuthService.BuildListenerPrefix(callbackUri);

        prefix.Should().Be("http://localhost:5096/");
    }

    [Fact]
    public void GenerateCodeVerifier_creates_rfc7636_compatible_value()
    {
        var verifier = AuthService.GenerateCodeVerifier();

        verifier.Length.Should().BeGreaterOrEqualTo(43);
        verifier.Length.Should().BeLessOrEqualTo(128);
        verifier.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }
}
