using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;

// Load environment variables from root .env file
var rootPath = Path.GetFullPath(Path.Join(Directory.GetCurrentDirectory(), "..", "..", ".."));
var envFilePath = Path.Join(rootPath, ".env");
if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

// Create host builder with CSnakes configuration
var builder = Host.CreateApplicationBuilder(args);

// Configure CSnakes with virtual environment
var currentDir = Directory.GetCurrentDirectory();
var venvPath = Path.Join(currentDir, ".venv");

builder.Services
    .WithPython()
    .WithVirtualEnvironment(venvPath)
    .WithPipInstaller()
    .FromRedistributable("3.12");

// Register evaluation service
var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

// Run evaluation
try
{
    Console.WriteLine("🐍 Initializing Python environment...");
    
    Console.WriteLine("📋 Running POML evaluation tests...");
    Console.WriteLine(new string('=', 50));
    
    // Get the Python module and call the function
    var evalModule = env.EvalPoml();
    var pythonResult = evalModule.EvalPrompt();
    
    // Parse JSON in C#
    using var document = System.Text.Json.JsonDocument.Parse(pythonResult);
    var root = document.RootElement;
    
    var success = root.GetProperty("success").GetBoolean();
    
    if (success)
    {
        var totalTests = root.GetProperty("total_tests").GetInt32();
        var passed = root.GetProperty("passed").GetInt32();
        var failed = root.GetProperty("failed").GetInt32();
        var passRate = root.GetProperty("pass_rate").GetString();
        
        Console.WriteLine($"📊 Evaluation Results:");
        Console.WriteLine($"   Total Tests: {totalTests}");
        Console.WriteLine($"   Passed: {passed}");
        Console.WriteLine($"   Failed: {failed}");
        Console.WriteLine($"   Pass Rate: {passRate}");
        Console.WriteLine();
        
        var results = root.GetProperty("results");
        
        foreach (var testResult in results.EnumerateArray())
        {
            var testPass = testResult.GetProperty("pass").GetBoolean();
            var description = testResult.GetProperty("description").GetString();
            var rawResponse = testResult.GetProperty("raw_response").GetString();
            var expected = testResult.GetProperty("expected");
            var expectedTool = expected.GetProperty("tool").GetString();
            var tokensUsed = testResult.GetProperty("tokens_used").GetInt32();
            
            var status = testPass ? "✅ PASS" : "❌ FAIL";
            Console.WriteLine($"{status} {description}");
            
            if (!testPass)
            {
                var issues = testResult.GetProperty("issues");
                var issuesList = new List<string>();
                
                foreach (var issue in issues.EnumerateArray())
                {
                    issuesList.Add(issue.GetString() ?? "Unknown issue");
                }
                
                Console.WriteLine($"   Issues: {string.Join(", ", issuesList)}");
            }
            
            Console.WriteLine($"   Raw Response: \"{rawResponse}\"");
            Console.WriteLine($"   Expected Tool: {expectedTool}");
            Console.WriteLine($"   Tokens Used: {tokensUsed}");
            Console.WriteLine();
        }
    }
    else
    {
        var error = root.GetProperty("error").GetString();
        Console.WriteLine($"❌ Evaluation failed: {error}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"💥 Error running evaluation: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}

Console.WriteLine("✨ Evaluation complete!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
