using Newtonsoft.Json;

namespace apsMcp.Tools.Models;

public class PropertyDefinition
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonProperty("units")]
    public string Units { get; set; } = string.Empty;
}