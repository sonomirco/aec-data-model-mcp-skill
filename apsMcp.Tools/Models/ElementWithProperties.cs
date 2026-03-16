using Newtonsoft.Json;

namespace apsMcp.Tools.Models;

public class ElementWithProperties
{
    [JsonProperty("properties")]
    public PropertyResults Properties { get; set; } = new();
}

public class PropertyResults
{
    [JsonProperty("results")]
    public List<PropertyValue> Results { get; set; } = new();
}

public class PropertyValue
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("value")]
    public object Value { get; set; } = string.Empty;
}