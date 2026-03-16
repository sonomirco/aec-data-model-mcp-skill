namespace apsMcp.Evaluations.Models;

public class EvaluationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int TotalTests { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public string PassRate { get; set; } = string.Empty;
    public List<TestResult> Results { get; set; } = [];
}

public class TestResult
{
    public string UserInput { get; set; } = string.Empty;
    public string ExpectedTool { get; set; } = string.Empty;
    public Dictionary<string, object> ExpectedParams { get; set; } = [];
    public string AssistantResponse { get; set; } = string.Empty;
    public bool Pass { get; set; }
    public List<string> Issues { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public string? FoundTool { get; set; }
    public bool FoundAuthGuidance { get; set; }
}