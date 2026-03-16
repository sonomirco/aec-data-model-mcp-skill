using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using apsMcp.Tools.Services;
using ApsMcp.Tools.Services;
using Newtonsoft.Json;

namespace apsMcp.Tools;

[McpServerToolType]
public class GraphQlTools
{
    private readonly AecDataModelGraphQLService _aecDataModelService;
    private readonly TokenStorageService _tokenStorage;
    private readonly AuthService _authService;

    public GraphQlTools(IServiceProvider services)
    {
        _aecDataModelService = services.GetRequiredService<AecDataModelGraphQLService>();
        _tokenStorage = services.GetRequiredService<TokenStorageService>();
        _authService = services.GetRequiredService<AuthService>();
    }

    [McpServerTool(Name = "aecdm-execute-graphql"), Description("Execute GraphQL queries against AEC Data Model API using predefined templates. Available templates and their parameters are provided in the system prompt. Automatically handles authentication if not already authenticated.")]
    public async Task<object> ExecuteAECGraphQLAsync(
        [Description("Name of the GraphQL template to execute")]
        string templateName,
        [Description("Array of parameter values in order required by the template, or null if no parameters needed")]
        string[]? parameters = null,
        [Description("Region for the API request (default: 'US')")]
        string region = "US")
    {
        // Check if we have a valid token, authenticate if not
        await _authService.EnsureAuthenticatedAsync(_tokenStorage);
        
        return await _aecDataModelService.ExecuteTemplateWithArrayAsync(templateName, region, parameters);
    }

    [McpServerTool(Name = "deme-create-exchange"), Description("Create data exchange from source file to target folder with a specific filter. Uses cached element group information from previous queries.")]
    public async Task<object> CreateExchangeAsync(
        [Description("Exchange filter for elements using simple syntax (e.g., \"(category=='Windows')\") - NOT RSQL format")]
        string filter,
        [Description("Element group ID to create exchange from (must be from previously cached results)")]
        string elementGroupId,
        [Description("Optional exchange name (auto-generated from filter if not provided)")]
        string? targetExchangeName = null,
        [Description("Region for the API request (default: 'US')")]
        string region = "US")
    {
        // Check if we have a valid token, authenticate if not
        await _authService.EnsureAuthenticatedAsync(_tokenStorage);
        
        // Retrieve cached element group information
        var elementGroup = await _aecDataModelService.GetCachedElementGroupInfo(elementGroupId);
        if (elementGroup == null)
        {
            throw new InvalidOperationException($"Element group {elementGroupId} not found in cache. Please run GetElementGroupsByProject first to cache the required information.");
        }
        
        if (string.IsNullOrEmpty(elementGroup.FileUrn))
        {
            throw new InvalidOperationException($"Element group {elementGroupId} does not have a cached fileUrn (lineage ID) required for exchange creation.");
        }
        
        // Get hub, project, and folder info from element group context
        var contextInfo = await _aecDataModelService.GetElementGroupContext(elementGroupId);
        
        // Generate base64 encoded IDs using cached information
        var sourceFileId = Utilities.Base64IdGenerator.GenerateSourceFileId(contextInfo.HubId, contextInfo.ProjectId, elementGroup.ParentFolderId, elementGroup.FileUrn);
        var targetFolderId = Utilities.Base64IdGenerator.GenerateTargetFolderId(contextInfo.HubId, contextInfo.ProjectId, elementGroup.ParentFolderId);
        var exchangeName = targetExchangeName ?? Utilities.Base64IdGenerator.GenerateExchangeName(filter);
        
        // Execute CreateExchange template
        return await _aecDataModelService.ExecuteTemplateWithArrayAsync("CreateExchange", region, 
            new string[] { filter, sourceFileId, exchangeName, targetFolderId });
    }
}
