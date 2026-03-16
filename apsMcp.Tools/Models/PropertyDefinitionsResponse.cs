using Newtonsoft.Json;

namespace apsMcp.Tools.Models;

public class PropertyDefinitionsResponse
{
    [JsonProperty("pagination")]
    public PaginationInfo Pagination { get; set; } = new();

    [JsonProperty("results")]
    public List<PropertyDefinition> Results { get; set; } = new();
}

public class PaginationInfo
{
    [JsonProperty("cursor")]
    public string? Cursor { get; set; }

    [JsonProperty("limit")]
    public int? Limit { get; set; }

    [JsonProperty("pageSize")]
    public int PageSize { get; set; }

    public bool HasNextPage => !string.IsNullOrWhiteSpace(Cursor);
}