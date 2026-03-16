using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace apsMcp.Tools.Services;

public class RelationalCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RelationalCacheService> _logger;

    public RelationalCacheService(IMemoryCache cache, ILogger<RelationalCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public object? Get(string key) => _cache.Get(key);

    public void Set(string key, object value)
    {
        _cache.Set(key, value);
        _logger.LogDebug("Cached data with key: {Key}", key);
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Removed cache key: {Key}", key);
    }

    public string BuildCacheKey(string templateName, Dictionary<string, object>? parameters)
    {
        var baseCacheKey = templateName switch
        {
            "GetHubs" => "hubs",
            "GetProjects" when parameters?.ContainsKey("hubId") == true =>
                $"projects:{parameters["hubId"]}",
            "GetElementGroupsByProject" when parameters?.ContainsKey("projectId") == true =>
                $"elementgroups:{parameters["projectId"]}",
            "GetPropertiesOfTheElement" when parameters?.ContainsKey("elementGroupId") == true && parameters?.ContainsKey("filter") == true =>
                $"properties:{parameters["elementGroupId"]}:{parameters["filter"]?.GetHashCode()}",
            "GetNumberOfElementsByCategory" when parameters?.ContainsKey("elementGroupId") == true && parameters?.ContainsKey("propertyFilter") == true =>
                $"elementcounts:{parameters["elementGroupId"]}:{parameters["propertyFilter"]?.GetHashCode()}",
            "GetElementsWithFilter" when parameters?.ContainsKey("elementGroupId") == true && parameters?.ContainsKey("propertyFilter") == true =>
                $"elementsfilter:{parameters["elementGroupId"]}:{parameters["propertyFilter"]?.GetHashCode()}:{GetPropertyNamesHash(parameters)}",
            "GetFileInformation" when parameters?.ContainsKey("elementGroupId") == true =>
                $"fileinfo:{parameters["elementGroupId"]}",
            "CreateExchange" when parameters?.ContainsKey("filter") == true && parameters?.ContainsKey("sourceFileId") == true =>
                $"exchange:{parameters["sourceFileId"]}:{parameters["filter"]?.GetHashCode()}:{parameters["targetExchangeName"]?.GetHashCode()}",
            _ => $"{templateName}:{string.Join(":", parameters?.Values.Select(v => v?.ToString() ?? "") ?? Array.Empty<string>())}"
        };

        // Add pagination parameters to cache key to ensure different paginated requests are cached separately
        var paginationSuffix = BuildPaginationSuffix(parameters);
        return paginationSuffix.Length > 0 ? $"{baseCacheKey}:{paginationSuffix}" : baseCacheKey;
    }

    private string BuildPaginationSuffix(Dictionary<string, object>? parameters)
    {
        if (parameters == null) return "";

        var paginationParts = new List<string>();

        if (parameters.ContainsKey("cursor") && parameters["cursor"] != null)
        {
            paginationParts.Add($"cursor:{parameters["cursor"]?.GetHashCode()}");
        }

        if (parameters.ContainsKey("pageSize") && parameters["pageSize"] != null)
        {
            paginationParts.Add($"pageSize:{parameters["pageSize"]}");
        }

        return string.Join(":", paginationParts);
    }

    private int GetPropertyNamesHash(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("propertyNames", out var propertyNamesObj) && propertyNamesObj is string[] propertyNames)
        {
            return string.Join(",", propertyNames).GetHashCode();
        }
        return 0;
    }
}