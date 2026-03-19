using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using apsMcp.Tools;
using Serilog;
using apsMcp.Tools.Services;
using DotNetEnv;

Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "mcp_server.log"),
               rollingInterval: RollingInterval.Day,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .WriteTo.Debug()
           .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose)
           .CreateLogger();

Log.Information("Starting server...");
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
//builder.Logging.ClearProviders();
builder.Services.AddMemoryCache();
builder.Logging.AddSerilog();

// Check if APS configuration is already available
var clientId = builder.Configuration["APS_CLIENT_ID"];
var clientSecret = builder.Configuration["APS_CLIENT_SECRET"];
var callbackUrl = builder.Configuration["APS_CALLBACK_URL"];

// Only load .env file if configuration is missing
if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(callbackUrl))
{
    Console.WriteLine("ℹ️  APS configuration not found in system environment variables or appsettings");
    Console.WriteLine("   Searching for .env file...");
    
    var rootPath = builder.Environment.ContentRootPath;
    var possibleEnvPaths = new[]
    {
        Path.Combine(rootPath, ".env"),
        Path.Combine(rootPath, "..", ".env"),
        Path.Combine(rootPath, "..", "..", ".env")
    };

    string? loadedEnvPath = null;
    foreach (var envPath in possibleEnvPaths)
    {
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            loadedEnvPath = envPath;
            Console.WriteLine($"✅ Loaded configuration from .env file: {envPath}");
            
            // Reload configuration after loading .env file
            builder.Configuration.AddEnvironmentVariables();
            clientId = builder.Configuration["APS_CLIENT_ID"];
            clientSecret = builder.Configuration["APS_CLIENT_SECRET"];
            callbackUrl = builder.Configuration["APS_CALLBACK_URL"];
            break;
        }
    }

    if (loadedEnvPath == null)
    {
        Console.WriteLine("❌ No .env file found in any of the expected locations:");
        foreach (var path in possibleEnvPaths)
        {
            Console.WriteLine($"   - {path}");
        }
    }
}
else
{
    Console.WriteLine("✅ APS configuration found in system environment variables or appsettings");
}

// Final validation of required configuration
if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(callbackUrl))
{
    Console.WriteLine();
    Console.WriteLine("❌ Missing required APS configuration!");
    Console.WriteLine();
    Console.WriteLine("Please set the following environment variables or create a .env file:");
    Console.WriteLine("  APS_CLIENT_ID      - Your APS application client ID");
    Console.WriteLine("  APS_CLIENT_SECRET  - Your APS application client secret");
    Console.WriteLine("  APS_CALLBACK_URL   - OAuth callback URL (e.g., http://localhost:5096/api/auth/callback)");
    Console.WriteLine();
    Console.WriteLine("Get your APS credentials from: https://aps.autodesk.com/myapps/");
    Console.WriteLine();
    Console.WriteLine("Example .env file content:");
    Console.WriteLine("  APS_CLIENT_ID=your_client_id_here");
    Console.WriteLine("  APS_CLIENT_SECRET=your_client_secret_here");
    Console.WriteLine("  APS_CALLBACK_URL=http://localhost:5096/api/auth/callback");
    Console.WriteLine();
    
    throw new ApplicationException("Missing required APS configuration. See console output above for details.");
}

Console.WriteLine("✅ APS configuration loaded successfully!");

// Add GraphQL services
builder.Services.AddSingleton<apsMcp.Tools.Services.RelationalCacheService>();
builder.Services.AddScoped<apsMcp.Tools.Services.AecDataModelGraphQLService>();
builder.Services.AddSingleton<TokenStorageService>();
builder.Services.AddSingleton<ViewerRuntimeService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ViewerRuntimeService>());
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AuthService>>();
    return new AuthService(
        clientId: clientId,
        clientSecret: clientSecret,
        callbackUrl: callbackUrl,
        scopes: ["data:read"],
        logger: logger
    );
});

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation { Name = "apsMcp", Version = "0.0.1" };
        options.ServerInstructions =
            "You are a Autodesk Platform Services (APS) assistant using GraphQL. " +
            "You are able to use the tools provided to you to help the user with their questions.";
    })
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(GraphQlTools).Assembly)
    .WithResourcesFromAssembly(typeof(GraphQlResources).Assembly);


var app = builder.Build();

// Configure the app to listen on port 5096
// app.Urls.Add("http://localhost:5096");

app.Use(async (context, next) =>
{
    var isRootGet = HttpMethods.IsGet(context.Request.Method) && context.Request.Path == "/";
    var hasSessionHeader = !string.IsNullOrWhiteSpace(context.Request.Headers["Mcp-Session-Id"]);

    if (isRootGet && !hasSessionHeader)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            name = "apsMcp sse server",
            message = "This endpoint uses MCP streamable HTTP. Start a session with POST / and include Accept: application/json, text/event-stream.",
            inspector = "Use the Aspire MCP inspector to connect and browse tools/resources."
        });

        return;
    }

    await next();
});

app.MapMcp();
app.MapDefaultEndpoints();
app.Run();

