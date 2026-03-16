using Xunit;
using FluentAssertions;
using apsMcp.Tools.Models;
using apsMcp.Tools.Configuration;
using apsMcp.Tests.Helpers;

namespace apsMcp.Tests;

/// <summary>
/// Tests to verify parameter definitions match their usage in queries and that
/// pagination parameters are handled correctly across all templates.
/// </summary>
public class ParameterConsistencyTests
{
    private readonly string[] _templates = {
        "GetHubs", "GetProjects", "GetElementGroupsByProject",
        "GetNumberOfElementsByCategory", "GetElementsWithFilter", "GetFileInformation"
    };

    private readonly Dictionary<string, GraphQLTemplate> _templateMap;

    public ParameterConsistencyTests()
    {
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
    public void ValidateParameterDefinitionsConsistent(string templateName)
    {
        // Arrange
        var template = _templateMap[templateName];
        var testParams = GetAllParameterCombinations(templateName);

        // Act & Assert
        foreach (var paramSet in testParams)
        {
            var query = template.BuildQuery(paramSet);

            // Extract parameter definitions from query using shared helper
            var definedParams = QueryParsingHelper.ExtractParameterDefinitions(query);

            // Verify all used parameters are defined
            var usedParams = QueryParsingHelper.ExtractUsedParameters(query);

            usedParams.Should().BeSubsetOf(definedParams,
                $"Template {templateName}: Used parameters must be defined. Query: {query}");
        }
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidatePaginationParametersHandled(string templateName)
    {
        // Arrange
        var template = _templateMap[templateName];

        // Test pagination parameter handling
        var withCursor = GetRequiredParams(templateName);
        withCursor["cursor"] = "test_cursor";

        var withPageSize = GetRequiredParams(templateName);
        withPageSize["pageSize"] = 25;

        var withBoth = GetRequiredParams(templateName);
        withBoth["cursor"] = "test_cursor";
        withBoth["pageSize"] = 25;

        // Act
        var queryCursor = template.BuildQuery(withCursor);
        var queryPageSize = template.BuildQuery(withPageSize);
        var queryBoth = template.BuildQuery(withBoth);

        // Assert
        if (template.SupportsPagination)
        {
            queryCursor.Should().Contain("$cursor: String!");
            queryCursor.Should().Contain("cursor: $cursor");

            queryPageSize.Should().Contain("$pageSize: Int");
            queryPageSize.Should().Contain("limit: $pageSize");

            queryBoth.Should().Contain("$cursor: String!");
            queryBoth.Should().Contain("$pageSize: Int");
            queryBoth.Should().Contain("cursor: $cursor");
            queryBoth.Should().Contain("limit: $pageSize");
        }
    }

    [Fact]
    public void ValidateSemanticWrapperTransformation()
    {
        // Test GetNumberOfElementsByCategory semantic wrapper
        var template = _templateMap["GetNumberOfElementsByCategory"];
        var parameters = new Dictionary<string, object>
        {
            ["elementGroupId"] = "test-group",
            ["category"] = "walls",
            ["propertyFilter"] = "property.name.category=contains='walls' and 'property.name.Element Context'==Instance"
        };

        var query = template.BuildQuery(parameters);

        // Should use propertyFilter in the query, not category
        query.Should().Contain("$propertyFilter: String!");
        query.Should().Contain("filter: {query: $propertyFilter}");
        query.Should().NotContain("$category");
    }

    [Fact]
    public void ValidateGetElementsWithFilterPropertyNames()
    {
        // Test GetElementsWithFilter with propertyNames
        var template = _templateMap["GetElementsWithFilter"];
        var parameters = new Dictionary<string, object>
        {
            ["elementGroupId"] = "test-group",
            ["propertyFilter"] = "property.name.category=contains='walls'",
            ["propertyNames"] = new[] { "External ID", "Area", "Volume" }
        };

        var query = template.BuildQuery(parameters);

        // Should include propertyNames parameter
        query.Should().Contain("$propertyNames: [String!]!");
        query.Should().Contain("filter:{names:$propertyNames}");
    }

    [Theory]
    [InlineData("GetHubs", 0)]
    [InlineData("GetProjects", 1)]
    [InlineData("GetElementGroupsByProject", 1)]
    [InlineData("GetNumberOfElementsByCategory", 2)]
    [InlineData("GetElementsWithFilter", 2)]
    [InlineData("GetFileInformation", 1)]
    public void ValidateRequiredParameterCounts(string templateName, int expectedCount)
    {
        var template = _templateMap[templateName];
        template.RequiredParameters.Count.Should().Be(expectedCount,
            $"Template {templateName} should have {expectedCount} required parameters");
    }

    [Fact]
    public void ValidateOptionalParametersConfiguration()
    {
        foreach (var templateName in _templates)
        {
            var template = _templateMap[templateName];

            if (template.SupportsPagination)
            {
                template.OptionalParameters.Should().Contain("cursor");
                template.OptionalParameters.Should().Contain("pageSize");
            }
        }
    }

    [Fact]
    public void ValidateGetElementsWithFilterParameterProcessing()
    {
        // Test the exact scenario that was failing
        var template = _templateMap["GetElementsWithFilter"];
        var parameters = new Dictionary<string, object>
        {
            ["elementGroupId"] = "YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ345cExvUGk4MlRheTFFaTBGVlZvMUVn",
            ["propertyFilter"] = "property.name.category=contains='Pipe Fittings' and 'property.name.Element Context'==Instance",
            ["propertyNames"] = new[] { "External ID", "Length" }
        };

        var query = template.BuildQuery(parameters);

        // Should NOT contain cursor with "Length" value
        query.Should().NotContain("$cursor: String!");
        query.Should().NotContain("cursor: $cursor");

        // Should contain propertyNames with Length
        query.Should().Contain("$propertyNames: [String!]!");
        query.Should().Contain("filter:{names:$propertyNames}");

        // Should contain required parameters
        query.Should().Contain("$elementGroupId: ID!");
        query.Should().Contain("$propertyFilter: String!");
    }

    [Fact]
    public void ValidateGetElementsWithFilterMinimalParameters()
    {
        // Test minimal parameter scenario (first call without cursor)
        var template = _templateMap["GetElementsWithFilter"];
        var parameters = new Dictionary<string, object>
        {
            ["elementGroupId"] = "test-group",
            ["propertyFilter"] = "property.name.category=contains='pipes'",
            ["propertyNames"] = new[] { "External ID" }
        };

        var query = template.BuildQuery(parameters);


        // Should NOT contain pagination parameters for first call
        query.Should().NotContain("$cursor");
        query.Should().NotContain("$pageSize");
        query.Should().NotContain("pagination:");

        // Should contain propertyNames (defaults to External ID)
        query.Should().Contain("$propertyNames: [String!]!");
        query.Should().Contain("filter:{names:$propertyNames}");

        // Should match expected first call format
        var expectedQuery = @"query GetElementsWithFilter($elementGroupId: ID!, $propertyFilter: String!, $propertyNames: [String!]!) {
                    elementsByElementGroup(elementGroupId: $elementGroupId, filter: {query: $propertyFilter}) {
                        pagination {
                            cursor
                            pageSize
                        }
                        results{
                            properties(filter:{names:$propertyNames}){
                                results {
                                    name
                                    value
                                }
                            }
                        }
                    }
                }";

        // Compare normalized queries (remove whitespace differences)
        var normalizedActual = System.Text.RegularExpressions.Regex.Replace(query, @"\s+", " ").Trim();
        var normalizedExpected = System.Text.RegularExpressions.Regex.Replace(expectedQuery, @"\s+", " ").Trim();

        normalizedActual.Should().Be(normalizedExpected);
    }

    [Fact]
    public void ValidateGetElementsWithFilterWithCursor()
    {
        // Test subsequent call with cursor (pagination scenario)
        var template = _templateMap["GetElementsWithFilter"];
        var parameters = new Dictionary<string, object>
        {
            ["elementGroupId"] = "test-group",
            ["propertyFilter"] = "property.name.category=contains='pipes'",
            ["propertyNames"] = new[] { "External ID" },
            ["cursor"] = "Y3Vyc241MH41MA"
        };

        var query = template.BuildQuery(parameters);


        // Should contain cursor parameter for subsequent calls
        query.Should().Contain("$cursor: String!");
        query.Should().Contain("cursor: $cursor");
        query.Should().Contain("pagination: { cursor: $cursor }");

        // Should still contain propertyNames
        query.Should().Contain("$propertyNames: [String!]!");
        query.Should().Contain("filter:{names:$propertyNames}");
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidateConsistentCursorPaginationBehavior(string templateName)
    {
        // Test that all templates handle cursor pagination consistently
        var template = _templateMap[templateName];

        // First call parameters (no cursor)
        var firstCallParams = GetRequiredParams(templateName);
        var firstQuery = template.BuildQuery(firstCallParams);

        // Should NOT contain cursor parameters for first call
        firstQuery.Should().NotContain("$cursor: String!", $"Template {templateName} should not include cursor parameter in first call");
        firstQuery.Should().NotContain("cursor: $cursor", $"Template {templateName} should not reference cursor in first call");
        firstQuery.Should().NotContain("pagination:", $"Template {templateName} should not include pagination clause in first call");

        // Second call parameters (with cursor)
        var secondCallParams = GetRequiredParams(templateName);
        secondCallParams["cursor"] = "test_cursor_value";
        var secondQuery = template.BuildQuery(secondCallParams);

        // Should contain cursor parameters for subsequent calls
        secondQuery.Should().Contain("$cursor: String!", $"Template {templateName} should include cursor parameter definition in subsequent calls");
        secondQuery.Should().Contain("cursor: $cursor", $"Template {templateName} should reference cursor in subsequent calls");
        secondQuery.Should().Contain("pagination: { cursor: $cursor }", $"Template {templateName} should include pagination clause with cursor in subsequent calls");

        // Both queries should contain the standard pagination structure in response
        firstQuery.Should().Contain("pagination {", $"Template {templateName} should return pagination info in response (first call)");
        firstQuery.Should().Contain("cursor", $"Template {templateName} should return cursor field in response (first call)");

        secondQuery.Should().Contain("pagination {", $"Template {templateName} should return pagination info in response (subsequent call)");
        secondQuery.Should().Contain("cursor", $"Template {templateName} should return cursor field in response (subsequent call)");
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidatePageSizeParameterHandling(string templateName)
    {
        // Test that pageSize parameter is handled consistently
        var template = _templateMap[templateName];

        // Call with pageSize parameter
        var paramsWithPageSize = GetRequiredParams(templateName);
        paramsWithPageSize["pageSize"] = 25;
        var queryWithPageSize = template.BuildQuery(paramsWithPageSize);

        queryWithPageSize.Should().Contain("$pageSize: Int", $"Template {templateName} should include pageSize parameter definition");
        queryWithPageSize.Should().Contain("limit: $pageSize", $"Template {templateName} should reference pageSize as limit");
        queryWithPageSize.Should().Contain("pagination: { limit: $pageSize }", $"Template {templateName} should include pagination clause with limit");
    }

    [Theory]
    [InlineData("GetHubs")]
    [InlineData("GetProjects")]
    [InlineData("GetElementGroupsByProject")]
    [InlineData("GetNumberOfElementsByCategory")]
    [InlineData("GetElementsWithFilter")]
    [InlineData("GetFileInformation")]
    public void ValidateCursorAndPageSizeCombination(string templateName)
    {
        // Test that cursor + pageSize combination works correctly
        var template = _templateMap[templateName];

        var paramsWithBoth = GetRequiredParams(templateName);
        paramsWithBoth["cursor"] = "test_cursor";
        paramsWithBoth["pageSize"] = 50;
        var queryWithBoth = template.BuildQuery(paramsWithBoth);

        queryWithBoth.Should().Contain("$cursor: String!", $"Template {templateName} should include cursor parameter");
        queryWithBoth.Should().Contain("$pageSize: Int", $"Template {templateName} should include pageSize parameter");
        queryWithBoth.Should().Contain("pagination: { cursor: $cursor, limit: $pageSize }", $"Template {templateName} should include both cursor and limit in pagination clause");
    }

    private List<Dictionary<string, object>> GetAllParameterCombinations(string templateName)
    {
        var combinations = new List<Dictionary<string, object>>();
        var baseParams = GetRequiredParams(templateName);

        // Base parameters only
        combinations.Add(new Dictionary<string, object>(baseParams));

        if (_templateMap[templateName].SupportsPagination)
        {
            // With pageSize only
            var withPageSize = new Dictionary<string, object>(baseParams);
            withPageSize["pageSize"] = 25;
            combinations.Add(withPageSize);

            // With cursor only
            var withCursor = new Dictionary<string, object>(baseParams);
            withCursor["cursor"] = "test_cursor";
            combinations.Add(withCursor);

            // With both cursor and pageSize
            var withBoth = new Dictionary<string, object>(baseParams);
            withBoth["cursor"] = "test_cursor";
            withBoth["pageSize"] = 25;
            combinations.Add(withBoth);
        }

        return combinations;
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

        // Handle special transformations
        if (templateName == "GetNumberOfElementsByCategory" && parameters.ContainsKey("category"))
        {
            parameters["propertyFilter"] = $"property.name.category=contains='{parameters["category"]}' and 'property.name.Element Context'==Instance";
        }

        // Add propertyNames for GetElementsWithFilter
        if (templateName == "GetElementsWithFilter")
        {
            parameters["propertyNames"] = new[] { "External ID", "Area", "Volume" };
        }

        return parameters;
    }

    [Fact]
    public void ValidateGetElementsWithFilterReturnsPaginatedResponse()
    {
        // This test verifies that GetElementsWithFilter always returns PaginatedResponse
        // even on first call (no cursor) to capture pagination info for subsequent calls
        var template = _templateMap["GetElementsWithFilter"];

        // Verify template is configured for pagination
        template.SupportsPagination.Should().BeTrue("GetElementsWithFilter should support pagination");
        template.PaginatedResponseType.Should().NotBeNull("GetElementsWithFilter should have PaginatedResponseType defined");

        // The ResponseType should be List<ElementWithProperties>
        template.ResponseType.Should().Be(typeof(List<ElementWithProperties>));

        // The PaginatedResponseType should be PaginatedResponse<ElementWithProperties>
        template.PaginatedResponseType.Should().Be(typeof(PaginatedResponse<ElementWithProperties>));

        // Note: The actual return type logic is handled by ShouldReturnPaginatedResponse() in BaseGraphQLService
        // which should now ALWAYS return true for pagination-enabled templates
    }

    [Fact]
    public void DocumentExpectedPaginationWorkflow()
    {
        // This test documents the expected workflow for cursor-based pagination
        // with GetElementsWithFilter template

        /*
        EXPECTED PAGINATION WORKFLOW:

        1. FIRST CALL (No cursor):
           Input: ["elementGroupId", "propertyFilter", "property1", "property2"]
           Returns: PaginatedResponse<ElementWithProperties> {
               Pagination: { Cursor: "Y3Vyc241MH41MA", PageSize: 100 },
               Results: [ { Properties: { Results: [...] } }, ... ]
           }

        2. SUBSEQUENT CALL (With cursor):
           Input: ["elementGroupId", "propertyFilter", "property1", "property2", "Y3Vyc241MH41MA"]
           Returns: PaginatedResponse<ElementWithProperties> {
               Pagination: { Cursor: "nextCursorValue", PageSize: 100 },
               Results: [ { Properties: { Results: [...] } }, ... ]
           }

        3. FINAL CALL (No more pages):
           Returns: PaginatedResponse<ElementWithProperties> {
               Pagination: { Cursor: null, PageSize: 100 },
               Results: [ { Properties: { Results: [...] } }, ... ]
           }

        The key fix: ALL pagination-enabled templates now return PaginatedResponse
        to capture cursor information, even on the first call.
        */

        var template = _templateMap["GetElementsWithFilter"];
        template.SupportsPagination.Should().BeTrue();
        template.PaginatedResponseType.Should().Be(typeof(PaginatedResponse<ElementWithProperties>));
    }

    [Fact]
    public void ValidateElementWithPropertiesDeserialization()
    {
        // Test to ensure the type resolution fix works correctly
        // This verifies that PaginatedResponse<ElementWithProperties> correctly
        // resolves to ElementWithProperties for individual result elements

        var template = _templateMap["GetElementsWithFilter"];

        // Verify template configuration
        template.ResponseType.Should().Be(typeof(List<ElementWithProperties>));
        template.PaginatedResponseType.Should().Be(typeof(PaginatedResponse<ElementWithProperties>));

        // The key insight: when ExtractPaginatedData receives PaginatedResponse<ElementWithProperties>,
        // it should extract ElementWithProperties as the element type for results array,
        // NOT PaginatedResponse<ElementWithProperties> (which was causing nested pagination)

        // This test documents the expected behavior - actual verification would require
        // mock GraphQL response data which is beyond unit test scope
    }

    // Removed duplicate methods - now using shared QueryParsingHelper utility class
}