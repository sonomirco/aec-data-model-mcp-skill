using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Logging;
using apsMcp.Tools.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace apsMcp.Tools.Services;

public abstract class BaseGraphQLService : IDisposable
{
    protected const string AecDataModelUrl = "https://developer.api.autodesk.com/aec/graphql";
    protected const string DataExchangeUrl = "https://developer.api.autodesk.com/dataexchange/2023-05/graphql";
    
    protected readonly ILogger _logger;
    protected readonly RelationalCacheService _cache;
    protected readonly Dictionary<string, GraphQLTemplate> _templates = new();
    protected readonly GraphQLHttpClient _aecDataModelClient;
    protected readonly GraphQLHttpClient _dataExchangeClient;

    protected BaseGraphQLService(ILogger logger, RelationalCacheService cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        
        // Initialize GraphQL clients for both endpoints
        _aecDataModelClient = new GraphQLHttpClient(AecDataModelUrl, new NewtonsoftJsonSerializer());
        _dataExchangeClient = new GraphQLHttpClient(DataExchangeUrl, new NewtonsoftJsonSerializer());
        
        _logger.LogDebug("BaseGraphQLService initialized with AEC Data Model and Data Exchange endpoints");
    }

    public void RegisterTemplate(GraphQLTemplate template)
    {
        _templates[template.Name] = template;
        _logger.LogDebug("Registered GraphQL template: {TemplateName}", template.Name);
    }

    public async Task<object> ExecuteTemplateAsync(string templateName, string accessToken, string region = "US")
    {
        return await ExecuteTemplateAsync(templateName, accessToken, region, new Dictionary<string, object>());
    }

    public async Task<object> ExecuteTemplateAsync(string templateName, string accessToken, string region, Dictionary<string, object> parameters)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        // Determine if this should return a paginated response
        var shouldPaginate = ShouldReturnPaginatedResponse(template, parameters);

        // Build cache key using the actual parameters that will be sent to GraphQL
        var cacheKey = _cache.BuildCacheKey(templateName, parameters);

        // Try cache first
        var cached = _cache.Get(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for template {TemplateName} with key {CacheKey}", templateName, cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss for template {TemplateName}, executing query", templateName);

        // Build the dynamic query with pagination support
        var query = template.BuildQuery(parameters);

        // Execute the query
        var result = await ExecuteQueryWithTemplate(template, query, accessToken, region, parameters);

        // Extract and format the data
        var extracted = shouldPaginate ?
            ExtractPaginatedData(result, template.ExtractPath, template.PaginatedResponseType ?? template.ResponseType) :
            ExtractData(result, template.ExtractPath, template.ResponseType);

        // Cache the result
        _cache.Set(cacheKey, extracted);

        return extracted;
    }

    public async Task<PaginatedResponse<T>> ExecutePaginatedQueryAsync<T>(string templateName, string accessToken, string region, Dictionary<string, object> parameters)
    {
        if (!_templates.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found");
        }

        if (!template.SupportsPagination)
        {
            throw new InvalidOperationException($"Template '{templateName}' does not support pagination");
        }

        // Set default page size if not provided
        if (!parameters.ContainsKey("pageSize"))
        {
            parameters["pageSize"] = template.DefaultPageSize;
        }

        var query = template.BuildQuery(parameters);
        var result = await ExecuteQueryWithTemplate(template, query, accessToken, region, parameters);

        return ExtractPaginatedResponse<T>(result, template.ExtractPath);
    }

    private bool ShouldReturnPaginatedResponse(GraphQLTemplate template, Dictionary<string, object> parameters)
    {
        // Always return paginated response for pagination-enabled templates to capture cursor info
        return template.SupportsPagination;
    }

    private async Task<GraphQLResponse<object>> ExecuteQueryWithTemplate(GraphQLTemplate template, string query, string accessToken, string region, Dictionary<string, object> parameters)
    {
        // Create a temporary template for the dynamic query execution
        var tempTemplate = new GraphQLTemplate { Query = query, Name = template.Name };
        return await ExecuteQueryAsync(tempTemplate, accessToken, region, parameters);
    }

    private object ExtractPaginatedData(GraphQLResponse<object> response, string extractPath, Type responseType)
    {
        if (response.Data == null)
        {
            throw new InvalidOperationException("No data in GraphQL response");
        }

        var dataObject = JObject.FromObject(response.Data!);

        if (!string.IsNullOrWhiteSpace(extractPath))
        {
            var queryRoot = extractPath.Split('.')[0]; // e.g., "hubs" from "hubs.results"
            var rootToken = dataObject.SelectToken(queryRoot);

            if (rootToken == null)
            {
                throw new InvalidOperationException($"Query root '{queryRoot}' not found");
            }

            // Extract pagination info
            var paginationToken = rootToken.SelectToken("pagination");
            var resultsToken = rootToken.SelectToken("results");

            if (resultsToken == null)
            {
                throw new InvalidOperationException("Results not found in paginated response");
            }

            // Build paginated response
            var paginationInfo = paginationToken?.ToObject<PaginationInfo>() ?? new PaginationInfo();

            // Correctly resolve the element type for results array
            Type resultsType;
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(PaginatedResponse<>))
            {
                // Extract T from PaginatedResponse<T> → ElementWithProperties
                resultsType = responseType.GetGenericArguments()[0];
            }
            else if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // Extract T from List<T>
                resultsType = responseType.GetGenericArguments()[0];
            }
            else
            {
                resultsType = responseType;
            }

            var results = resultsToken.ToObject(typeof(List<>).MakeGenericType(resultsType));

            var paginatedResponseType = typeof(PaginatedResponse<>).MakeGenericType(resultsType);
            var paginatedResponse = Activator.CreateInstance(paginatedResponseType);

            paginatedResponseType.GetProperty("Pagination")?.SetValue(paginatedResponse, paginationInfo);
            paginatedResponseType.GetProperty("Results")?.SetValue(paginatedResponse, results);

            return paginatedResponse ?? throw new InvalidOperationException("Failed to create paginated response");
        }

        return dataObject;
    }

    private PaginatedResponse<T> ExtractPaginatedResponse<T>(GraphQLResponse<object> response, string extractPath)
    {
        if (response.Data == null)
        {
            throw new InvalidOperationException("No data in GraphQL response");
        }

        var dataObject = JObject.FromObject(response.Data!);

        if (!string.IsNullOrWhiteSpace(extractPath))
        {
            var queryRoot = extractPath.Split('.')[0];
            var rootToken = dataObject.SelectToken(queryRoot);

            if (rootToken == null)
            {
                throw new InvalidOperationException($"Query root '{queryRoot}' not found");
            }

            var paginationToken = rootToken.SelectToken("pagination");
            var resultsToken = rootToken.SelectToken("results");

            if (resultsToken == null)
            {
                throw new InvalidOperationException("Results not found in paginated response");
            }

            return new PaginatedResponse<T>
            {
                Pagination = paginationToken?.ToObject<PaginationInfo>() ?? new PaginationInfo(),
                Results = resultsToken.ToObject<List<T>>() ?? new List<T>()
            };
        }

        throw new InvalidOperationException("Cannot extract paginated response without extract path");
    }
    protected abstract Task<GraphQLResponse<object>> ExecuteQueryAsync(GraphQLTemplate template, string accessToken, string region, Dictionary<string, object>? parameters = null);  

    protected async Task<GraphQLResponse<object>> ExecuteAecDataModelQuery(string query, string accessToken, string region, Dictionary<string, object>? variables = null)
    {
        try
        {
            // Set authorization header
            _aecDataModelClient.HttpClient.DefaultRequestHeaders.Clear();
            _aecDataModelClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _aecDataModelClient.HttpClient.DefaultRequestHeaders.Add("Region", region);

            _logger.LogDebug("Executing AEC Data Model GraphQL query with variables: {Variables}", 
                variables != null ? JsonConvert.SerializeObject(variables) : "none");

            // Create GraphQL request
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = variables
            };

            // Execute query
            var response = await _aecDataModelClient.SendQueryAsync<object>(request);

            if (response.Errors != null && response.Errors.Length > 0)
            {
                var errorMessages = string.Join(", ", response.Errors.Select(e => e.Message));
                _logger.LogWarning("AEC Data Model GraphQL query failed: {ErrorMessages}", errorMessages);
                throw new InvalidOperationException($"GraphQL query failed: {errorMessages}");
            }

            _logger.LogDebug("AEC Data Model GraphQL query executed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing AEC Data Model GraphQL query");
            throw;
        }
    }

    protected async Task<GraphQLResponse<object>> ExecuteDataExchangeQuery(string query, string accessToken, string region, Dictionary<string, object>? variables = null)
    {
        try
        {
            // Set authorization header  
            _dataExchangeClient.HttpClient.DefaultRequestHeaders.Clear();
            _dataExchangeClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            _dataExchangeClient.HttpClient.DefaultRequestHeaders.Add("Region", region);

            _logger.LogDebug("Executing Data Exchange GraphQL query with variables: {Variables}", 
                variables != null ? JsonConvert.SerializeObject(variables) : "none");

            // Create GraphQL request
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = variables
            };

            // Execute query (this will handle both queries and mutations)
            var response = await _dataExchangeClient.SendQueryAsync<object>(request);

            if (response.Errors != null && response.Errors.Length > 0)
            {
                var errorMessages = string.Join(", ", response.Errors.Select(e => e.Message));
                _logger.LogWarning("Data Exchange GraphQL query failed: {ErrorMessages}", errorMessages);
                throw new InvalidOperationException($"GraphQL query failed: {errorMessages}");
            }

            _logger.LogDebug("Data Exchange GraphQL query executed successfully");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while executing Data Exchange GraphQL query");
            throw;
        }
    }

    private object ExtractData(GraphQLResponse<object> response, string extractPath, Type responseType)
    {
        if (response.Data == null)
        {
            throw new InvalidOperationException("No data in GraphQL response");
        }

        // Navigate through the extract path
        var dataObject = JObject.FromObject(response.Data!);
        
        if (!string.IsNullOrWhiteSpace(extractPath))
        {
            var token = dataObject.SelectToken(extractPath);
            if (token == null)
            {
                return JsonConvert.SerializeObject(new { error = $"extractPath '{extractPath}' not found", data = dataObject });
            }

            var result = token.ToObject(responseType);
            return result ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        return dataObject;
    }


    public void Dispose()
    {
        _aecDataModelClient?.Dispose();
        _dataExchangeClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}