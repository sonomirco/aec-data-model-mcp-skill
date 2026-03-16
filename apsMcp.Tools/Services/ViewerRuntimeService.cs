using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApsMcp.Tools.Services;

/// <summary>
/// Manages viewer-side HTTP and WebSocket listeners with explicit lifecycle control.
/// </summary>
public sealed class ViewerRuntimeService(ILogger<ViewerRuntimeService> logger) : IHostedService, IAsyncDisposable
{
    private readonly ILogger<ViewerRuntimeService> _logger = logger;
    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private CancellationTokenSource? _serverCts;
    private HttpListener? _httpListener;
    private HttpListener? _webSocketListener;
    private Task? _httpLoopTask;
    private Task? _webSocketLoopTask;
    private WebSocket? _webSocket;
    private string _currentHtmlContent = "<html><body><h3>Viewer content is not initialized yet.</h3></body></html>";

    public int HttpPort { get; private set; }
    public int WebSocketPort { get; private set; }
    public bool HasOpenWebSocket => _webSocket?.State == WebSocketState.Open;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopServerAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await StopServerAsync(CancellationToken.None);
        _stateLock.Dispose();
    }

    public void UpdateHtmlContent(string htmlContent)
    {
        _currentHtmlContent = htmlContent ?? throw new ArgumentNullException(nameof(htmlContent));
    }

    public async Task EnsureStartedAsync(CancellationToken cancellationToken)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning())
            {
                return;
            }

            HttpPort = HttpPort == 0 ? GetAvailablePort() : HttpPort;
            WebSocketPort = WebSocketPort == 0 ? GetAvailablePort() : WebSocketPort;

            _serverCts?.Dispose();
            _serverCts = new CancellationTokenSource();
            _httpLoopTask = RunHttpLoopAsync(HttpPort, _serverCts.Token);
            _webSocketLoopTask = RunWebSocketLoopAsync(WebSocketPort, _serverCts.Token);

            _logger.LogInformation(
                "Viewer runtime listeners started on HTTP port {HttpPort} and WebSocket port {WebSocketPort}.",
                HttpPort,
                WebSocketPort);
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task<bool> SendMessageAsync(string payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(payload))
        {
            payload = string.Empty;
        }

        var socket = _webSocket;
        if (socket is null || socket.State != WebSocketState.Open)
        {
            return false;
        }

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(payload);
            await socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "Failed to send viewer WebSocket payload.");
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private bool IsRunning()
    {
        return _httpLoopTask is { IsCompleted: false } && _webSocketLoopTask is { IsCompleted: false };
    }

    private async Task StopServerAsync(CancellationToken cancellationToken)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            _serverCts?.Cancel();

            StopAndClose(_httpListener);
            StopAndClose(_webSocketListener);
            _httpListener = null;
            _webSocketListener = null;

            await CloseAndDisposeSocketAsync(_webSocket, cancellationToken);
            _webSocket = null;

            await ObserveLoopAsync(_httpLoopTask);
            await ObserveLoopAsync(_webSocketLoopTask);
            _httpLoopTask = null;
            _webSocketLoopTask = null;

            _serverCts?.Dispose();
            _serverCts = null;
        }
        finally
        {
            _stateLock.Release();
        }
    }

    private async Task RunHttpLoopAsync(int port, CancellationToken cancellationToken)
    {
        var listener = new HttpListener();
        _httpListener = listener;
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync().WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string currentContent = _currentHtmlContent;
                byte[] buffer = Encoding.UTF8.GetBytes(currentContent);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, cancellationToken);
                context.Response.Close();
            }
        }
        finally
        {
            StopAndClose(listener);
            if (ReferenceEquals(_httpListener, listener))
            {
                _httpListener = null;
            }
        }
    }

    private async Task RunWebSocketLoopAsync(int port, CancellationToken cancellationToken)
    {
        var listener = new HttpListener();
        _webSocketListener = listener;
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync().WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                    continue;
                }

                HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                var previousSocket = Interlocked.Exchange(ref _webSocket, webSocketContext.WebSocket);
                await CloseAndDisposeSocketAsync(previousSocket, cancellationToken);
            }
        }
        finally
        {
            StopAndClose(listener);
            if (ReferenceEquals(_webSocketListener, listener))
            {
                _webSocketListener = null;
            }
        }
    }

    private async Task ObserveLoopAsync(Task? task)
    {
        if (task is null)
        {
            return;
        }

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // expected during shutdown
        }
        catch (HttpListenerException)
        {
            // expected during shutdown
        }
        catch (ObjectDisposedException)
        {
            // expected during shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Viewer runtime loop terminated unexpectedly.");
        }
    }

    private static void StopAndClose(HttpListener? listener)
    {
        if (listener is null)
        {
            return;
        }

        try
        {
            listener.Stop();
        }
        catch (ObjectDisposedException)
        {
            // already disposed
        }

        listener.Close();
    }

    private static async Task CloseAndDisposeSocketAsync(WebSocket? socket, CancellationToken cancellationToken)
    {
        if (socket is null)
        {
            return;
        }

        try
        {
            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "viewer runtime shutdown", cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // do not block shutdown when cancellation is requested
        }
        catch (WebSocketException)
        {
            // ignore best-effort close errors
        }
        finally
        {
            socket.Dispose();
        }
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
