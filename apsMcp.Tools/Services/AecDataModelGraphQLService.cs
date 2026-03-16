using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using GraphQL;
using apsMcp.Tools.Models;
using ApsMcp.Tools.Services;
using apsMcp.Tools.Configuration;
using System.Text;

namespace apsMcp.Tools.Services;

public class AecDataModelGraphQLService : BaseGraphQLService
{
    private readonly TokenStorageService _tokenStorage;

    public AecDataModelGraphQLService(
        ILogger<AecDataModelGraphQLService> logger, 
        RelationalCacheService cache,
        TokenStorageService tokenStorage) 
        : base(logger, cache)
    {
        _tokenStorage = tokenStorage ?? throw new ArgumentNullException(nameof(tokenStorage));
        
        // Register AEC Data Model and Data Exchange templates
        RegisterTemplates();
        
        _logger.LogDebug("AecDataModelGraphQLService initialized");
    }

    protected override async Task<GraphQLResponse<object>> ExecuteQueryAsync(GraphQLTemplate template, string accessToken, string region, Dictionary<string, object>? parameters = null)
    {
        // Route to appropriate endpoint based on template name
        if (IsDataExchangeTemplate(template.Name))
        {
            return await ExecuteDataExchangeQuery(template.Query, accessToken, region, parameters);
        }
        
        // Default to AEC Data Model endpoint
        return await ExecuteAecDataModelQuery(template.Query, accessToken, region, parameters);
    }

    public async Task<object> ExecuteTemplateWithArrayAsync(string templateName, string region, string[]? parameters)
    {
        var token = await _tokenStorage.GetToken();
        if (token == null)
        {
            throw new InvalidOperationException("User not authenticated");
        }

        // Convert array parameters to dictionary based on template requirements
        var paramDict = ConvertArrayToParameterDictionary(templateName, parameters);
        
        var result = await ExecuteTemplateAsync(templateName, token.AccessToken, region, paramDict);

        // Apply post-processing for GetFileInformation template
        if (templateName == "GetFileInformation" && result is List<CachedFileInfo> fileInfoList)
        {
            foreach (var fileInfo in fileInfoList)
            {
                fileInfo.FilterProjectProperties();
            }
        }

        return result;
    }

    private Dictionary<string, object> ConvertArrayToParameterDictionary(string templateName, string[]? parameters)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        var paramDict = new Dictionary<string, object>();

        if (parameters == null || parameters.Length == 0)
        {
            if (template.RequiredParameters.Count > 0)
            {
                throw new ArgumentException($"Template '{templateName}' requires {template.RequiredParameters.Count} parameters: [{string.Join(", ", template.RequiredParameters)}]");
            }
            return paramDict;
        }

        // Calculate expected parameter range considering required params, pagination, and variable parameters
        var minParams = template.RequiredParameters.Count;
        var maxParams = template.SupportsVariableParameters
            ? int.MaxValue  // Unlimited for variable parameter templates
            : minParams + (template.SupportsPagination ? 2 : 0);  // Fixed count for others

        // Parameter validation based on template capabilities
        if (template.SupportsVariableParameters)
        {
            // Only check minimum parameters for variable parameter templates
            if (parameters.Length < minParams)
            {
                throw new ArgumentException($"Template '{templateName}' requires at least {minParams} parameters [{string.Join(", ", template.RequiredParameters)}], but {parameters.Length} were provided");
            }
        }
        else
        {
            // Strict range checking for fixed parameter templates
            if (parameters.Length < minParams || parameters.Length > maxParams)
            {
                var expectedRange = minParams == maxParams ? $"{minParams}" : $"{minParams}-{maxParams}";
                throw new ArgumentException($"Template '{templateName}' requires {expectedRange} parameters, but {parameters.Length} were provided");
            }
        }

        // Map required parameters first
        for (int i = 0; i < template.RequiredParameters.Count; i++)
        {
            string paramName = template.RequiredParameters[i];
            string paramValue = parameters[i];

            // Transform semantic wrapper parameters
            if (template.HasSemanticWrapper && paramName == template.SemanticWrapperSourceParam)
            {
                TransformCategoryToPropertyFilter(paramDict, paramValue, templateName);
            }
            else
            {
                paramDict[paramName] = paramValue;
            }
        }

        // Handle pagination parameters for supported templates
        // Note: For variable parameter templates, pagination params are optional and come at the end
        if (template.SupportsPagination && !template.SupportsVariableParameters)
        {
            var paginationParamIndex = template.RequiredParameters.Count;

            // Check for cursor parameter
            if (parameters.Length > paginationParamIndex && !string.IsNullOrWhiteSpace(parameters[paginationParamIndex]))
            {
                paramDict["cursor"] = parameters[paginationParamIndex];
                paginationParamIndex++;
            }

            // Check for pageSize parameter
            if (parameters.Length > paginationParamIndex && !string.IsNullOrWhiteSpace(parameters[paginationParamIndex]))
            {
                if (int.TryParse(parameters[paginationParamIndex], out var pageSize) && pageSize > 0)
                {
                    paramDict["pageSize"] = pageSize;
                }
                else
                {
                    throw new ArgumentException($"Invalid pageSize parameter: '{parameters[paginationParamIndex]}'. Must be a positive integer.");
                }
            }
        }

        // Handle additional variable parameters (start after required parameters)
        if (template.SupportsVariableParameters)
        {
            var variableParamStartIndex = template.RequiredParameters.Count;
            BuildPropertyNamesArray(paramDict, parameters, variableParamStartIndex);
        }

        return paramDict;
    }

    private void TransformCategoryToPropertyFilter(Dictionary<string, object> paramDict, string categoryValue, string templateName)
    {
        if (string.IsNullOrWhiteSpace(categoryValue))
        {
            throw new ArgumentException($"Category parameter cannot be null or empty for {templateName} template");
        }
        
        var sanitizedCategory = categoryValue.Trim().ToLowerInvariant();
        paramDict["propertyFilter"] = $"property.name.category=contains='{sanitizedCategory}' and 'property.name.Element Context'==Instance";
        
        _logger.LogDebug("Transformed category '{Category}' to propertyFilter for {TemplateName}", categoryValue, templateName);
    }

    private void BuildPropertyNamesArray(Dictionary<string, object> paramDict, string[] parameters, int requiredParamCount)
    {
        var propertyNames = new List<string> { "External ID" };
        
        for (int i = requiredParamCount; i < parameters.Length; i++)
        {
            var property = parameters[i]?.Trim();
            if (!string.IsNullOrWhiteSpace(property))
            {
                propertyNames.Add(property);
            }
        }

        var logMessage = parameters.Length > requiredParamCount
            ? $"Variable parameter template using custom propertyNames: [{string.Join(", ", propertyNames)}]"
            : "Variable parameter template using default propertyNames: [External ID]";

        _logger.LogDebug(logMessage);
        
        paramDict["propertyNames"] = propertyNames.ToArray();
    }

    private void RegisterTemplates()
    {
        // Register AEC Data Model templates
        foreach (var template in AecDataModelTemplates.GetTemplates())
        {
            RegisterTemplate(template);
        }
        
        // Register Data Exchange templates
        foreach (var template in DataExchangeTemplates.GetTemplates())
        {
            RegisterTemplate(template);
        }
        
        _logger.LogDebug("Registered {AecCount} AEC Data Model templates and {DataExchangeCount} Data Exchange templates", 
            AecDataModelTemplates.GetTemplates().Count, DataExchangeTemplates.GetTemplates().Count);
    }
    
    private bool IsDataExchangeTemplate(string templateName)
    {
        return DataExchangeTemplates.GetTemplates().Any(t => t.Name == templateName);
    }
    
    
    public Task<CachedElementGroup?> GetCachedElementGroupInfo(string elementGroupId)
    {
        // Search through cached element groups to find the one with matching ID
        // We need to iterate through cached project results to find the element group
        var cacheKeys = GetAllElementGroupCacheKeys();
        
        foreach (var cacheKey in cacheKeys)
        {
            var cachedElementGroups = _cache.Get(cacheKey) as List<CachedElementGroup>;
            if (cachedElementGroups != null)
            {
                var elementGroup = cachedElementGroups.FirstOrDefault(eg => eg.Id == elementGroupId);
                if (elementGroup != null)
                {
                    return Task.FromResult<CachedElementGroup?>(elementGroup);
                }
            }
        }
        
        return Task.FromResult<CachedElementGroup?>(null);
    }
    
    public async Task<(string HubId, string ProjectId)> GetElementGroupContext(string elementGroupId)
    {
        // Extract hub and project information from cache keys and return DataManagement API IDs
        var cacheKeys = GetAllElementGroupCacheKeys();
        
        foreach (var cacheKey in cacheKeys)
        {
            var cachedElementGroups = _cache.Get(cacheKey) as List<CachedElementGroup>;
            if (cachedElementGroups != null && cachedElementGroups.Any(eg => eg.Id == elementGroupId))
            {
                // Cache key format is "elementgroups:{projectId}"
                // We need to extract projectId and then find the corresponding hubId
                var projectGraphQLId = cacheKey.Replace("elementgroups:", "");
                var (hubDataMgmtId, projectDataMgmtId) = await GetDataManagementIdsFromGraphQLIds(projectGraphQLId);
                return (hubDataMgmtId, projectDataMgmtId);
            }
        }
        
        throw new InvalidOperationException($"Could not find context information for element group {elementGroupId}");
    }
    
    private List<string> GetAllElementGroupCacheKeys()
    {
        // This is a simplified approach - in a real implementation you might want to
        // maintain an index of cache keys or use a more sophisticated cache structure
        var keys = new List<string>();
        
        // Get all cached projects first
        var hubsCached = _cache.Get("hubs") as List<CachedHub>;
        if (hubsCached != null)
        {
            foreach (var hub in hubsCached)
            {
                var projectsCacheKey = $"projects:{hub.Id}";
                var projects = _cache.Get(projectsCacheKey) as List<CachedProject>;
                if (projects != null)
                {
                    foreach (var project in projects)
                    {
                        keys.Add($"elementgroups:{project.Id}");
                    }
                }
            }
        }
        
        return keys;
    }
    
    private Task<string> GetHubIdFromProjectId(string projectId)
    {
        // Search through cached projects to find the hub that contains this project
        var hubsCached = _cache.Get("hubs") as List<CachedHub>;
        if (hubsCached != null)
        {
            foreach (var hub in hubsCached)
            {
                var projectsCacheKey = $"projects:{hub.Id}";
                var projects = _cache.Get(projectsCacheKey) as List<CachedProject>;
                if (projects != null && projects.Any(p => p.Id == projectId))
                {
                    return Task.FromResult(hub.Id);
                }
            }
        }
        
        return Task.FromException<string>(new InvalidOperationException($"Could not find hub for project {projectId}"));
    }
    
    private Task<(string HubDataMgmtId, string ProjectDataMgmtId)> GetDataManagementIdsFromGraphQLIds(string projectGraphQLId)
    {
        // Search through cached projects to find the one with matching GraphQL ID and return DataManagement API IDs
        var hubsCached = _cache.Get("hubs") as List<CachedHub>;
        if (hubsCached != null)
        {
            foreach (var hub in hubsCached)
            {
                var projectsCacheKey = $"projects:{hub.Id}";
                var projects = _cache.Get(projectsCacheKey) as List<CachedProject>;
                if (projects != null)
                {
                    var project = projects.FirstOrDefault(p => p.Id == projectGraphQLId);
                    if (project != null)
                    {
                        return Task.FromResult((hub.DataManagementAPIHubId, project.DataManagementAPIProjectId));
                    }
                }
            }
        }
        
        throw new InvalidOperationException($"Could not find DataManagement API IDs for project {projectGraphQLId}");
    }
}