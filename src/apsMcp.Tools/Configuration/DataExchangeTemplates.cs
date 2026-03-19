using apsMcp.Tools.Models;

namespace apsMcp.Tools.Configuration;

public static class DataExchangeTemplates
{
    public static List<GraphQLTemplate> GetTemplates()
    {
        return new List<GraphQLTemplate>
        {
            new GraphQLTemplate
            {
                Name = "CreateExchange",
                Intent = "Create data exchange with filter from source file to target folder",
                Query = @"mutation CreateExchange($filter: String, $sourceFileId: String!, $targetExchangeName: String!, $targetFolderId: String!) {
                    createExchange(input: {
                        filter: $filter
                        source: { fileId: $sourceFileId }
                        target: { name: $targetExchangeName, folderId: $targetFolderId }
                    }) {
                        exchange {
                            id
                            name
                        }
                    }
                }",
                ExtractPath = "createExchange.exchange",
                CacheKeyPrefix = "exchange",
                ResponseType = typeof(CachedExchange),
                RequiredParameters = new List<string> { "filter", "sourceFileId", "targetExchangeName", "targetFolderId" }
            }
        };
    }
}