using Xunit;
using FluentAssertions;
using apsMcp.Tools.Models;
using apsMcp.Tools.Configuration;
using apsMcp.Tests.Helpers;

namespace apsMcp.Tests;

/// <summary>
/// Critical test suite validating dynamic query construction for all templates across all parameter combinations.
/// Ensures pagination support works correctly with backward compatibility.
/// </summary>
public class TemplateQueryGenerationTests
{
    private readonly string[] _templates = {
        "GetHubs", "GetProjects", "GetElementGroupsByProject",
        "GetNumberOfElementsByCategory", "GetElementsWithFilter", "GetFileInformation"
    };

    private readonly Dictionary<string, GraphQLTemplate> _templateMap;

    public TemplateQueryGenerationTests()
    {
        // Build template map from configuration
        _templateMap = AecDataModelTemplates.GetTemplates()
            .ToDictionary(t => t.Name, t => t);
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidateQueryGeneration_AllTemplates_NoParameters(string templateName)
    {
        // Arrange
        var template = _templateMap[templateName];
        var parameters = GetRequiredParamsOnly(templateName);

        // Act & Assert
        if (template.RequiredParameters.Count == 0)
        {
            // Should succeed for templates with no required parameters
            var query = template.BuildQuery(parameters);
            query.Should().NotBeNullOrEmpty();
            ValidateQuerySyntax(query, parameters, templateName);
        }
        else
        {
            // Should handle missing required parameters gracefully
            var query = template.BuildQuery(parameters);
            query.Should().NotBeNullOrEmpty();
        }
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidateQueryGeneration_AllTemplates_WithPageSize(string templateName)
    {
        // Arrange
        var template = _templateMap[templateName];
        var parameters = GetRequiredParams(templateName);
        parameters["pageSize"] = 25;

        // Act
        var query = template.BuildQuery(parameters);

        // Assert
        query.Should().NotBeNullOrEmpty();
        ValidateQuerySyntax(query, parameters, templateName);

        if (template.SupportsPagination)
        {
            query.Should().Contain("$pageSize: Int");
            query.Should().Contain("limit: $pageSize");
        }
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidateQueryGeneration_AllTemplates_WithCursorAndPageSize(string templateName)
    {
        // Arrange
        var template = _templateMap[templateName];
        var parameters = GetRequiredParams(templateName);
        parameters["cursor"] = "test_cursor_123";
        parameters["pageSize"] = 25;

        // Act
        var query = template.BuildQuery(parameters);

        // Assert
        query.Should().NotBeNullOrEmpty();
        ValidateQuerySyntax(query, parameters, templateName);

        if (template.SupportsPagination)
        {
            query.Should().Contain("$cursor: String!");
            query.Should().Contain("$pageSize: Int");
            query.Should().Contain("cursor: $cursor");
            query.Should().Contain("limit: $pageSize");
        }
    }

    [Fact]
    public void ValidateBackwardCompatibility_ExistingCallsWork()
    {
        // Test all existing template calls work without pagination params
        foreach (var templateName in _templates)
        {
            var template = _templateMap[templateName];
            var existingParams = GetTypicalExistingParameters(templateName);

            // Should not throw and should return valid query
            var query = template.BuildQuery(existingParams);
            query.Should().NotBeNullOrEmpty();
            ValidateQuerySyntax(query, existingParams, templateName);
        }
    }

    [Theory]
    [InlineData("GetHubs", 25)]
    [InlineData("GetProjects", 50)]
    [InlineData("GetElementGroupsByProject", 50)]
    [InlineData("GetNumberOfElementsByCategory", 100)]
    [InlineData("GetElementsWithFilter", 100)]
    [InlineData("GetFileInformation", 100)]
    public void ValidateDefaultPageSizes_MatchExpectedValues(string templateName, int expectedPageSize)
    {
        // Arrange
        var template = _templateMap[templateName];

        // Assert
        template.DefaultPageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public void ValidatePaginationSupport_AllTemplatesEnabled()
    {
        foreach (var templateName in _templates)
        {
            var template = _templateMap[templateName];
            template.SupportsPagination.Should().BeTrue($"Template {templateName} should support pagination");
            template.PaginatedResponseType.Should().NotBeNull($"Template {templateName} should have PaginatedResponseType defined");
        }
    }

    private Dictionary<string, object> GetRequiredParamsOnly(string templateName)
    {
        return new Dictionary<string, object>();
    }

    private Dictionary<string, object> GetRequiredParams(string templateName)
    {
        var template = _templateMap[templateName];
        var parameters = new Dictionary<string, object>();

        foreach (var param in template.RequiredParameters)
        {
            parameters[param] = param switch
            {
                "hubId" => "b.test-hub-id-123",
                "projectId" => "b.test-project-id-456",
                "elementGroupId" => "test-element-group-789",
                "category" => "walls",
                "propertyFilter" => "property.name.category=contains='walls' and 'property.name.Element Context'==Instance",
                "versionNumber" => 1,
                _ => "test-value"
            };
        }

        // Add propertyNames for GetElementsWithFilter
        if (templateName == "GetElementsWithFilter")
        {
            parameters["propertyNames"] = new[] { "External ID", "Area", "Volume" };
        }

        return parameters;
    }

    private Dictionary<string, object> GetTypicalExistingParameters(string templateName)
    {
        return templateName switch
        {
            "GetHubs" => new Dictionary<string, object>(),
            "GetProjects" => new Dictionary<string, object> { ["hubId"] = "b.test-hub-id" },
            "GetElementGroupsByProject" => new Dictionary<string, object> { ["projectId"] = "b.test-project-id" },
            "GetNumberOfElementsByCategory" => new Dictionary<string, object>
            {
                ["elementGroupId"] = "test-element-group",
                ["category"] = "walls"
            },
            "GetElementsWithFilter" => new Dictionary<string, object>
            {
                ["elementGroupId"] = "test-element-group",
                ["propertyFilter"] = "property.name.category=contains='walls'",
                ["propertyNames"] = new[] { "External ID" }
            },
            "GetFileInformation" => new Dictionary<string, object> { ["elementGroupId"] = "test-element-group" },
            _ => new Dictionary<string, object>()
        };
    }

    private void ValidateQuerySyntax(string query, Dictionary<string, object> parameters, string templateName = "unknown")
    {
        // Basic GraphQL syntax validation
        query.Should().Contain("query ");
        query.Should().Contain("{");
        query.Should().Contain("}");

        // Validate parameter definitions match usage
        AssertParameterDefinitionsMatchUsage(query, parameters, templateName);

        // Validate required fields presence
        AssertRequiredFieldsPresent(query);
    }

    private void AssertParameterDefinitionsMatchUsage(string query, Dictionary<string, object> parameters, string templateName)
    {
        // Extract parameter definitions and usage using shared helper
        var definedParams = QueryParsingHelper.ExtractParameterDefinitions(query);
        var usedParams = QueryParsingHelper.ExtractUsedParameters(query);

        // All used parameters should be defined
        foreach (var usedParam in usedParams)
        {
            definedParams.Should().Contain(usedParam,
                $"Used parameter {usedParam} must be defined in query: {query}");
        }
    }

    private void AssertRequiredFieldsPresent(string query)
    {
        // All paginated queries should have pagination and cursor fields
        if (QueryParsingHelper.HasPagination(query))
        {
            query.Should().Contain("cursor");
            // Note: Different templates use different result field names (results vs values)
            // This is validated separately using template properties if needed
        }
    }

    // Removed duplicate methods - now using shared QueryParsingHelper utility class
}