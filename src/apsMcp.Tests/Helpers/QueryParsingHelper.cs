using System;
using System.Collections.Generic;
using System.Linq;

namespace apsMcp.Tests.Helpers;

/// <summary>
/// Shared utility for parsing GraphQL queries in tests - eliminates code duplication
/// </summary>
public static class QueryParsingHelper
{
    /// <summary>
    /// Extracts parameter definitions from GraphQL query
    /// Handles edge cases like missing parentheses safely
    /// </summary>
    public static List<string> ExtractParameterDefinitions(string query)
    {
        var definitions = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
            return definitions;

        var startIndex = query.IndexOf('(');
        if (startIndex < 0)
            return definitions; // No parameters

        var endIndex = query.IndexOf(')', startIndex);
        if (endIndex <= startIndex)
            return definitions; // Malformed query

        var paramSection = query.Substring(startIndex + 1, endIndex - startIndex - 1);
        var paramParts = paramSection.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in paramParts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith('$'))
            {
                var colonIndex = trimmed.IndexOf(':');
                if (colonIndex > 0)
                {
                    definitions.Add(trimmed.Substring(1, colonIndex - 1));
                }
            }
        }

        return definitions;
    }

    /// <summary>
    /// Extracts used parameters from GraphQL query body
    /// </summary>
    public static List<string> ExtractUsedParameters(string query)
    {
        var usedParams = new List<string>();

        if (string.IsNullOrWhiteSpace(query))
            return usedParams;

        // Find all $parameter references in the query body
        var words = query.Split(new[] { ' ', '\t', '\n', '\r', '(', ')', '{', '}', ',', ':' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (word.StartsWith('$'))
            {
                var param = word.TrimStart('$');
                if (!string.IsNullOrWhiteSpace(param) && !usedParams.Contains(param))
                {
                    usedParams.Add(param);
                }
            }
        }

        return usedParams;
    }

    /// <summary>
    /// Determines if a query has pagination based on template properties
    /// </summary>
    public static bool HasPagination(string query)
    {
        return query.Contains("pagination") && query.Contains("cursor");
    }

    /// <summary>
    /// Gets the expected results field name based on template type
    /// </summary>
    public static string GetExpectedResultsField(string templateName, string cacheKeyPrefix)
    {
        return cacheKeyPrefix switch
        {
            "elementcounts" => "values",  // GetNumberOfElementsByCategory uses "values"
            _ => "results"  // Most templates use "results"
        };
    }
}