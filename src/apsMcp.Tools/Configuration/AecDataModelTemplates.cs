using apsMcp.Tools.Models;

namespace apsMcp.Tools.Configuration;

public static class AecDataModelTemplates
{
    public static List<GraphQLTemplate> GetTemplates()
    {
        return new List<GraphQLTemplate>
        {
            new GraphQLTemplate
            {
                Name = "GetHubs",
                Intent = "List all accessible Autodesk Construction Cloud hubs",
                Query = @"query GetHubs{parameterDefinitions} {
                    hubs{paginationClause} {
                        pagination {
                            cursor
                            pageSize
                        }
                        results {
                            name
                            id
                            alternativeIdentifiers {
                                dataManagementAPIHubId
                            }
                        }
                    }
                }",
                ExtractPath = "hubs.results",
                CacheKeyPrefix = "hubs",
                ResponseType = typeof(List<CachedHub>),
                RequiredParameters = new List<string>(),
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 25,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<CachedHub>)
            },

            new GraphQLTemplate
            {
                Name = "GetProjects",
                Intent = "List projects for a specific hub",
                Query = @"query GetProjects{parameterDefinitions} {
                    projects{paginationClause} {
                        pagination {
                            cursor
                            pageSize
                        }
                        results {
                            id
                            name
                            alternativeIdentifiers {
                                dataManagementAPIProjectId
                            }
                        }
                    }
                }",
                ExtractPath = "projects.results",
                CacheKeyPrefix = "projects",
                ResponseType = typeof(List<CachedProject>),
                RequiredParameters = new List<string> { "hubId" },
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 50,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<CachedProject>)
            },

            new GraphQLTemplate
            {
                Name = "GetElementGroupsByProject",
                Intent = "List element groups for a specific project",
                Query = @"query GetElementGroupsByProject{parameterDefinitions} {
                    elementGroupsByProject{paginationClause} {
                        pagination {
                            cursor
                            pageSize
                        }
                        results {
                            name
                            id
                            alternativeIdentifiers {
                                fileUrn
                                fileVersionUrn
                            }
                            parentFolder {
                                name
                                id
                                parentFolder {
                                    name
                                }
                            }
                        }
                    }
                }",
                ExtractPath = "elementGroupsByProject.results",
                CacheKeyPrefix = "elementgroups",
                ResponseType = typeof(List<CachedElementGroup>),
                RequiredParameters = new List<string> { "projectId" },
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 50,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<CachedElementGroup>)
            },

            // Semantic Wrapper Template: Users provide intuitive parameters, system handles complex GraphQL requirements
            new GraphQLTemplate
            {
                Name = "GetNumberOfElementsByCategory",
                Intent = "Count elements by category in an element group using semantic parameter wrapper",
                Query = @"query GetNumberOfElementsByCategory{parameterDefinitions} {
                    distinctPropertyValuesInElementGroupById{paginationClause} {
                        pagination {
                            cursor
                            pageSize
                        }
                        values {
                            value,
                            count
                        }
                    }
                }",
                ExtractPath = "distinctPropertyValuesInElementGroupById.values",
                CacheKeyPrefix = "elementcounts",
                ResponseType = typeof(List<CategoryCount>),
                // Note: Template uses semantic wrapper - "category" parameter gets auto-transformed to "propertyFilter"
                RequiredParameters = new List<string> { "elementGroupId", "category" },
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 100,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<CategoryCount>),
                HasSemanticWrapper = true,
                SemanticWrapperSourceParam = "category",
                SemanticWrapperTargetParam = "propertyFilter"
            },

            // Enhanced RSQL Filtering Template: LLM composes complete RSQL filters
            new GraphQLTemplate
            {
                Name = "GetElementsWithFilter",
                Intent = "Retrieve element summaries within an element group with optional filters and property picks",
                Query = @"query GetElementsWithFilter{parameterDefinitions} {
                    elementsByElementGroup{paginationClause} {
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
                }",
                ExtractPath = "elementsByElementGroup.results",
                CacheKeyPrefix = "elementsfilter",
                ResponseType = typeof(List<ElementWithProperties>),
                // Note: Enhanced RSQL filtering with LLM composition:
                // - Minimum 2 params: ["elementGroupId", "propertyFilter"] → defaults to ["External ID"]
                // - Extended: ["elementGroupId", "propertyFilter", "property1", "property2", ...] → custom propertyNames
                // - "propertyFilter" must be complete RSQL (e.g., "property.name.category=contains='pipes' and 'property.name.Element Context'==Instance and property.name.Length < 0.4")
                // - Additional params become propertyNames array (always includes External ID for viewer integration)
                // - LLM responsible for generating syntactically correct RSQL with compound operations, proper quoting, and Element Context requirement
                RequiredParameters = new List<string> { "elementGroupId", "propertyFilter" },
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 100,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<ElementWithProperties>),
                SupportsVariableParameters = true,
                VariableParameterName = "propertyNames"
            },

            new GraphQLTemplate
            {
                Name = "GetFileInformation",
                Intent = "Get file information by retrieving Project Information category properties, filtered for properties starting with 'Project' and non-null values",
                Query = @"query GetFileInformation{parameterDefinitions} {
                    elementsByElementGroup{paginationClause} {
                        pagination {
                            cursor
                            pageSize
                        }
                        results {
                            properties{
                                results {
                                    name
                                    value
                                }
                            }
                        }
                    }
                }",
                ExtractPath = "elementsByElementGroup.results",
                CacheKeyPrefix = "fileinfo",
                ResponseType = typeof(List<CachedFileInfo>),
                RequiredParameters = new List<string> { "elementGroupId" },
                OptionalParameters = new List<string> { "cursor", "pageSize" },
                DefaultPageSize = 100,
                SupportsPagination = true,
                PaginatedResponseType = typeof(PaginatedResponse<CachedFileInfo>)
            },


            new GraphQLTemplate
            {
                Name = "GetElementGroupByVersionNumber",
                Intent = "Retrieve element group data for a specific version number - critical for version comparison workflows",
                Query = @"query GetElementGroupByVersionNumber($elementGroupId: ID!, $versionNumber: Int!) {
                    elementGroupByVersionNumber(elementGroupId: $elementGroupId, versionNumber: $versionNumber) {
                        id
                        name
                        versionNumber
                        alternativeIdentifiers {
                            fileUrn
                            fileVersionUrn
                        }
                        parentFolder {
                            name
                            id
                            parentFolder {
                                name
                            }
                        }
                    }
                }",
                ExtractPath = "elementGroupByVersionNumber",
                CacheKeyPrefix = "elementgroupversion",
                ResponseType = typeof(CachedElementGroup),
                RequiredParameters = new List<string> { "elementGroupId", "versionNumber" }
            },

            new GraphQLTemplate
            {
                Name = "GetPropertyDefinitionsByElementGroup",
                Intent = "Retrieve property definitions (schema) for an element group - essential for property discovery and validation workflows",
                Query = @"query GetPropertyDefinitionsByElementGroup($elementGroupId: ID!, $cursor: String, $limit: Int) {
                    propertyDefinitionsByElementGroup(
                        elementGroupId: $elementGroupId,
                        pagination: {cursor: $cursor, limit: $limit}
                    ) {
                        pagination {
                            cursor
                            limit
                        }
                        results {
                            id
                            name
                            description
                            dataType
                            units
                        }
                    }
                }",
                ExtractPath = "propertyDefinitionsByElementGroup",
                CacheKeyPrefix = "propertydefinitions",
                ResponseType = typeof(PropertyDefinitionsResponse),
                RequiredParameters = new List<string> { "elementGroupId" }
            }
        };
    }
}