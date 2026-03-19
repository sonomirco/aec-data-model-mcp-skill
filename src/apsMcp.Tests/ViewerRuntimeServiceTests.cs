using apsMcp.Tools.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace apsMcp.Tests;

public class ViewerRuntimeServiceTests
{
    [Fact]
    public async Task EnsureStartedAsync_starts_listeners_and_stops_cleanly()
    {
        await using var service = new ViewerRuntimeService(NullLogger<ViewerRuntimeService>.Instance);

        await service.EnsureStartedAsync(CancellationToken.None);

        service.HttpPort.Should().BeGreaterThan(0);
        service.WebSocketPort.Should().BeGreaterThan(0);

        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task SendMessageAsync_returns_false_without_active_socket()
    {
        await using var service = new ViewerRuntimeService(NullLogger<ViewerRuntimeService>.Instance);
        await service.EnsureStartedAsync(CancellationToken.None);

        var sent = await service.SendMessageAsync("sample", CancellationToken.None);

        sent.Should().BeFalse();
    }
}

