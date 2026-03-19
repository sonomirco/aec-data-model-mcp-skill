namespace apsMcp.Tools.Models;

public class GraphQLTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string ExtractPath { get; set; } = string.Empty;
    public string CacheKeyPrefix { get; set; } = string.Empty;
    public Type ResponseType { get; set; } = typeof(object);
    public List<string> RequiredParameters { get; set; } = new();
    public List<string> OptionalParameters { get; set; } = new();
    public int DefaultPageSize { get; set; } = 50;
    public bool SupportsPagination { get; set; } = false;
    public Type? PaginatedResponseType { get; set; }

    // Variable parameter support
    public bool SupportsVariableParameters { get; set; } = false;
    public string? VariableParameterName { get; set; } = null;

    // Semantic wrapper support (e.g., category → propertyFilter transformation)
    public bool HasSemanticWrapper { get; set; } = false;
    public string? SemanticWrapperSourceParam { get; set; } = null;
    public string? SemanticWrapperTargetParam { get; set; } = null;

    public string BuildQuery(Dictionary<string, object> parameters)
    {
        if (!SupportsPagination)
        {
            return Query;
        }

        return BuildPaginatedQuery(parameters);
    }

    private string BuildPaginatedQuery(Dictionary<string, object> parameters)
    {
        var parameterDefinitions = BuildParameterDefinitions(parameters);
        var paginationClause = BuildPaginationClause(parameters);

        // Replace placeholder patterns in the query
        var query = Query
            .Replace("{parameterDefinitions}", parameterDefinitions)
            .Replace("{paginationClause}", paginationClause.Length > 0 ? $"({paginationClause})" : "");

        return query;
    }

    private string BuildParameterDefinitions(Dictionary<string, object> parameters)
    {
        var definitions = new List<string>();

        // Add required parameters (excluding semantic wrapper source parameters)
        foreach (var param in RequiredParameters)
        {
            if (parameters.ContainsKey(param))
            {
                // Skip semantic wrapper source parameter - it gets transformed
                if (HasSemanticWrapper
                    && !string.IsNullOrWhiteSpace(SemanticWrapperSourceParam)
                    && !string.IsNullOrWhiteSpace(SemanticWrapperTargetParam)
                    && param == SemanticWrapperSourceParam
                    && parameters.ContainsKey(SemanticWrapperTargetParam))
                {
                    continue; // Don't add source parameter, only transformed parameter
                }

                definitions.Add($"${param}: {GetGraphQLType(param)}");
            }
        }

        // Add transformed parameters for semantic wrapper
        if (HasSemanticWrapper
            && !string.IsNullOrWhiteSpace(SemanticWrapperSourceParam)
            && !string.IsNullOrWhiteSpace(SemanticWrapperTargetParam)
            && RequiredParameters.Contains(SemanticWrapperSourceParam)
            && parameters.ContainsKey(SemanticWrapperTargetParam))
        {
            if (!definitions.Any(d => d.Contains($"${SemanticWrapperTargetParam}")))
            {
                definitions.Add($"${SemanticWrapperTargetParam}: String!");
            }
        }

        // Add variable parameter for templates that support them
        // Note: Always include variable parameter definition since it's referenced in query body
        if (SupportsVariableParameters && !string.IsNullOrEmpty(VariableParameterName))
        {
            definitions.Add($"${VariableParameterName}: [String!]!");
        }

        // Add optional pagination parameters if present
        if (parameters.ContainsKey("cursor"))
            definitions.Add("$cursor: String!");
        if (parameters.ContainsKey("pageSize"))
            definitions.Add("$pageSize: Int");

        return definitions.Any() ? $"({string.Join(", ", definitions)})" : "";
    }

    private string BuildPaginationClause(Dictionary<string, object> parameters)
    {
        var clauses = new List<string>();

        // Handle different template types using properties instead of hard-coded names
        if (HasSemanticWrapper
            && !string.IsNullOrWhiteSpace(SemanticWrapperTargetParam)
            && parameters.ContainsKey(SemanticWrapperTargetParam))
        {
            clauses.Add("elementGroupId: $elementGroupId");
            if (SemanticWrapperSourceParam == "category")
            {
                // GetNumberOfElementsByCategory specific handling
                clauses.Add("propertyDefinitionId: \"autodesk.revit.parameter:parameter.category-2.0.0\"");
            }
            clauses.Add($"filter: {{query: ${SemanticWrapperTargetParam}}}");
        }
        else if (SupportsVariableParameters && parameters.ContainsKey("propertyFilter"))
        {
            clauses.Add("elementGroupId: $elementGroupId");
            clauses.Add("filter: {query: $propertyFilter}");
        }
        else if (RequiredParameters.Contains("elementGroupId") && CacheKeyPrefix == "fileinfo")
        {
            clauses.Add("elementGroupId: $elementGroupId");
            clauses.Add("filter: {query: \"property.name.category=='Project Information'\"}");
        }
        else
        {
            // Standard parameter handling for other templates
            foreach (var param in RequiredParameters)
            {
                if (parameters.ContainsKey(param) && !IsSpecialParameter(param))
                {
                    clauses.Add($"{param}: ${param}");
                }
            }
        }

        // Add pagination parameters if present
        var paginationParts = new List<string>();
        if (parameters.ContainsKey("cursor"))
            paginationParts.Add("cursor: $cursor");
        if (parameters.ContainsKey("pageSize"))
            paginationParts.Add("limit: $pageSize");

        if (paginationParts.Any())
            clauses.Add($"pagination: {{ {string.Join(", ", paginationParts)} }}");

        return string.Join(", ", clauses);
    }

    private bool IsSpecialParameter(string parameterName)
    {
        return parameterName == "category" || parameterName == "propertyFilter" || parameterName == "propertyNames";
    }

    private string GetGraphQLType(string parameterName)
    {
        return parameterName switch
        {
            "hubId" or "projectId" or "elementGroupId" => "ID!",
            "versionNumber" => "Int!",
            "category" or "propertyFilter" => "String!",
            "propertyNames" => "[String!]!",
            _ => "String!"
        };
    }
}
