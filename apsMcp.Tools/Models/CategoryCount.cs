using Newtonsoft.Json;

namespace apsMcp.Tools.Models;

public class CategoryCount
{
    [JsonProperty("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonProperty("count")]
    public int Count { get; set; }
}