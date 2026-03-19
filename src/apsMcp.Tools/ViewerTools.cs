using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using apsMcp.Tools.Services;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace apsMcp.Tools;

[McpServerToolType]
public class ViewerTool
{
    private readonly TokenStorageService _tokenStorage;
    private readonly AuthService _authService;
    private readonly ViewerRuntimeService _viewerRuntimeService;

    public ViewerTool(IServiceProvider services)
    {
        _tokenStorage = services.GetRequiredService<TokenStorageService>();
        _authService = services.GetRequiredService<AuthService>();
        _viewerRuntimeService = services.GetRequiredService<ViewerRuntimeService>();
    }

    [McpServerTool(Name = "aps-highlight-elements"), Description("Highlight specific elements in the currently loaded viewer by their External IDs, or pass an empty array to show all elements (clear view). The viewer must be loaded first using aps-viewer-render.")]
    public async Task<string> HighLightElements([Description("Array of external IDs of elements to highlight in the viewer (e.g., ['12345', '67890']), or empty array [] to show all elements (clear view)")] string[] externalIds)
    {
        var sent = await _viewerRuntimeService.SendMessageAsync(string.Join(",", externalIds), CancellationToken.None);
        if (!sent)
        {
            return "No active viewer connection. Please load a model first using aps-viewer-render.";
        }

        return externalIds.Length == 0
            ? "Cleared view - showing all elements in viewer."
            : $"Highlighted {externalIds.Length} elements in viewer.";
    }

    [McpServerTool(Name = "aps-viewer-render"), Description("Load and render a 3D model in a separate browser window.")]
    public async Task<string> ViewerAsync([Description("File version URN of the model to render - format: 'urn:adsk.wipprod:fs.file:vf.'")] string fileVersionUrn)
    {
        await _authService.EnsureAuthenticatedAsync(_tokenStorage);

        string urnBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileVersionUrn));
        var token = await _tokenStorage.GetToken() ?? throw new InvalidOperationException("User is not authenticated.");

        await _viewerRuntimeService.EnsureStartedAsync(CancellationToken.None);

        var htmlContent = BuildViewerHtml(token.AccessToken, urnBase64, _viewerRuntimeService.WebSocketPort);
        _viewerRuntimeService.UpdateHtmlContent(htmlContent);

        Process.Start(new ProcessStartInfo($"http://localhost:{_viewerRuntimeService.HttpPort}/") { UseShellExecute = true });
        return "Viewer has been initialized in the browser!";
    }

    private static string BuildViewerHtml(string accessToken, string urnBase64, int wsPort)
    {
        return @"
        <!DOCTYPE html>
        <html>

        <head>
        <title>Autodesk Platform Services Claude Viewer Template</title>
        <!-- Autodesk Platform Services Viewer files -->
        <link rel=""stylesheet"" href=""https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/style.min.css""
                type=""text/css"">
        <script src=""https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/viewer3D.min.js""></script>
        </head>

        <body onload=""initAPSViewer()"">
        <div id=""apsViewer""></div>
        </body>
        <script>
            let viewer = null;
            //REPLACE THE TOKEN HERE
            var _access_token = '" + accessToken + @"';
            //REPLACE THE URN HERE
            var _urn = '" + urnBase64 + @"';

            let socket = new WebSocket('ws://localhost:" + wsPort + @"');
            socket.onmessage = function(event) {
            var externalIds = event.data.split(',');

            // If empty array or single empty string, show all elements (clear view)
            if (externalIds.length === 0 || (externalIds.length === 1 && externalIds[0].trim() === '')) {
                viewer.showAll();
                viewer.fitToView();
                return;
            }

            viewer.model.getExternalIdMapping((externalIdsDictionary) => {
                let dbids = [];
                externalIds.forEach(externalId => {
                let dbid = externalIdsDictionary[externalId];
                if (!!dbid)
                    dbids.push(dbid);
                });
                viewer.isolate(dbids);
                viewer.fitToView();
            }, console.log)
            };

            async function initAPSViewer() {
                const options = {
                env: 'AutodeskProduction',
                accessToken: _access_token,
                isAEC: true
                };

            Autodesk.Viewing.Initializer(options, () => {

            const div = document.getElementById('apsViewer');

            const config = { extensions: ['Autodesk.DocumentBrowser'] }

            viewer = new Autodesk.Viewing.Private.GuiViewer3D(div, config);
            viewer.start();
            viewer.setTheme(""light-theme"");
            Autodesk.Viewing.Document.load(`urn:${_urn}`, doc =>
            {
                var viewables = doc.getRoot().getDefaultGeometry();
                viewer.loadDocumentNode(doc, viewables).then(onLoadFinished);
                });
                });
            }

            function onLoadFinished() {
                console.log('Model loaded successfully');
            }
        </script>
        </html>";
    }
}

