using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using ApsMcp.Tools.Services;
using apsMcp.Tools.Services;
using apsMcp.Tools;
using DotNetEnv;
using ModelContextProtocol.Protocol;

Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "mcp_server.log"),
               rollingInterval: RollingInterval.Day,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .WriteTo.Debug()
           .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose)
           .CreateLogger();

try
{
    Log.Information("Starting APS MCP Stdio Server...");

    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    // Check if APS configuration is already available (injected by AppHost or system environment)
    var clientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET");
    var callbackUrl = Environment.GetEnvironmentVariable("APS_CALLBACK_URL");

    // Only load .env file if configuration is missing (standalone execution)
    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(callbackUrl))
    {
        Log.Information("APS configuration not found in environment variables. Searching for .env file...");

        var rootPath = builder.Environment.ContentRootPath;
        var envName = builder.Environment.EnvironmentName;
        var envFilePath = Path.Combine(rootPath, "..", "..", ".env");

        // Try multiple potential .env file locations
        var possibleEnvPaths = new[]
        {
            Path.Combine(rootPath, ".env"),
            Path.Combine(rootPath, "..", ".env"),
            Path.Combine(rootPath, "..", "..", ".env"),
            envFilePath // Two levels up (solution root)
        };

        string? loadedEnvPath = null;
        foreach (var envPath in possibleEnvPaths)
        {
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                loadedEnvPath = envPath;
                Log.Information("Loaded configuration from .env file: {EnvPath}", envPath);

                // Read values after loading .env file
                clientId = Environment.GetEnvironmentVariable("APS_CLIENT_ID") ?? Env.GetString("APS_CLIENT_ID", "");
                clientSecret = Environment.GetEnvironmentVariable("APS_CLIENT_SECRET") ?? Env.GetString("APS_CLIENT_SECRET", "");
                callbackUrl = Environment.GetEnvironmentVariable("APS_CALLBACK_URL") ?? Env.GetString("APS_CALLBACK_URL", "");
                break;
            }
        }

        if (loadedEnvPath == null)
        {
            Log.Warning("No .env file found in any of the expected locations: {Paths}", string.Join(", ", possibleEnvPaths));
        }
    }
    else
    {
        Log.Information("APS configuration found in environment variables (likely injected by AppHost)");
    }

    // Final validation of required configuration
    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(callbackUrl))
    {
        Log.Error(
            """
            Missing required APS configuration!

            Please set the following environment variables or create a .env file:
              APS_CLIENT_ID      - Your APS application client ID
              APS_CLIENT_SECRET  - Your APS application client secret
              APS_CALLBACK_URL   - OAuth callback URL (e.g., http://localhost:5096/api/auth/callback)

            Get your APS credentials from: https://aps.autodesk.com/myapps/

            Example .env file content:
              APS_CLIENT_ID=your_client_id_here
              APS_CLIENT_SECRET=your_client_secret_here
              APS_CALLBACK_URL=http://localhost:5096/api/auth/callback
            """);
        throw new ApplicationException("Missing required APS configuration. See console output above for details.");
    }

    Log.Information("APS configuration loaded successfully!");

    // Add configuration
    builder.Configuration.AddEnvironmentVariables();

    // Add services
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<TokenStorageService>();
    builder.Services.AddSingleton<RelationalCacheService>();
    builder.Services.AddSingleton<ViewerRuntimeService>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<ViewerRuntimeService>());
    builder.Services.AddScoped<AecDataModelGraphQLService>();
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

    // Configure MCP server
    builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation { Name = "apsMcp.StdioServer", Version = "1.0.0" };
        options.ServerInstructions =
            "You are a Autodesk Platform Services (APS) assistant using GraphQL. " +
            "You are able to use the tools provided to you to help the user with their questions. " +
            "You can execute GraphQL queries against AEC Data Model API, manage authentication, " +
            "and work with 3D models using the Autodesk Viewer.";
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(GraphQlTools).Assembly)
    .WithResourcesFromAssembly(typeof(GraphQlResources).Assembly);

    await builder.Build().RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
