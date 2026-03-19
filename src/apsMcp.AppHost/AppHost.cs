using DotNetEnv;

var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

var rootPath = builder.Environment.ContentRootPath;
var envName = builder.Environment.EnvironmentName;
var envFilePath = Path.Combine(rootPath, "..", "..", ".env");

Env.Load(envFilePath);

var clientId = Env.GetString("APS_CLIENT_ID", "");
var clientSecret = Env.GetString("APS_CLIENT_SECRET", "");
var callbackUrl = Env.GetString("APS_CALLBACK_URL", "");

var sseServer = builder.AddProject<Projects.apsMcp_SseServer>("sse-server")
    .WithExternalHttpEndpoints()
    .WithEnvironment("APS_CLIENT_ID", clientId)
    .WithEnvironment("APS_CLIENT_SECRET", clientSecret)
    .WithEnvironment("APS_CALLBACK_URL", callbackUrl);

var stdioServer = builder.AddProject<Projects.apsMcp_StdioServer>("stdio-server")
    .WithEnvironment("APS_CLIENT_ID", clientId)
    .WithEnvironment("APS_CLIENT_SECRET", clientSecret)
    .WithEnvironment("APS_CALLBACK_URL", callbackUrl);

var inspector = builder.AddMcpInspector("inspector")
    .WithMcpServer(sseServer);

builder.Build().Run();