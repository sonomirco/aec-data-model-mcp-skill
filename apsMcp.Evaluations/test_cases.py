def get_test_cases():
    """Returns all test cases for evaluation"""
    return [
        # GetHubs test cases
        {
            "input": "Show me my hubs",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": []
            },
            "description": "Basic GetHubs request"
        },
        {
            "input": "List all hubs",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": []
            },
            "description": "GetHubs with 'list all' phrasing"
        },
        {
            "input": "What hubs do I have access to?",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": []
            },
            "description": "GetHubs with question format"
        },
        
        
        # GetProjects with hubId test cases
        {
            "input": "List projects for hub urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetProjects",
                "parameters": ["urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f"]
            },
            "description": "GetProjects with full APS hubId URN"
        },
        {
            "input": "What projects are in hub urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e?",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetProjects",
                "parameters": ["urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e"]
            },
            "description": "GetProjects with question format and full hubId URN"
        },
        
        # GetElementGroupsByProject test cases
        {
            "input": "Show me element groups for project urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupsByProject",
                "parameters": ["urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef"]
            },
            "description": "GetElementGroupsByProject with projectId"
        },
        {
            "input": "List models for project urn:adsk.workspace:prod.project:f1e2d3c4-b5a6-9807-6543-21098765fedc",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupsByProject",
                "parameters": ["urn:adsk.workspace:prod.project:f1e2d3c4-b5a6-9807-6543-21098765fedc"]
            },
            "description": "GetElementGroupsByProject using 'models' terminology"
        },
        
        # GetNumberOfElementsByCategory test cases
        {
            "input": "Count pipe elements in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetNumberOfElementsByCategory",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn", "pipe"]
            },
            "description": "GetNumberOfElementsByCategory with specific elementGroupId and category"
        },
        {
            "input": "How many wall elements are in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetNumberOfElementsByCategory",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn", "wall"]
            },
            "description": "GetNumberOfElementsByCategory with question format"
        },
        {
            "input": "Count all the mechanical equipment for element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetNumberOfElementsByCategory",
                "parameters": ["ABC123XYZ789", "mechanical equipment"]
            },
            "description": "GetNumberOfElementsByCategory with multi-word category"
        },
        
        # GetFileInformation test cases
        {
            "input": "Get file information for element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetFileInformation",
                "parameters": ["ABC123XYZ789"]
            },
            "description": "GetFileInformation with elementGroupId"
        },
        {
            "input": "Show me project information for element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetFileInformation",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn"]
            },
            "description": "GetFileInformation with 'project information' phrasing"
        },
        {
            "input": "What file details are available for element group TEST789?",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetFileInformation",
                "parameters": ["TEST789"]
            },
            "description": "GetFileInformation with question format using 'file details'"
        },
        
        # GetElementsWithFilter test cases
        {
            "input": "Filter all wall elements in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn", "property.name.category=contains=walls and 'property.name.Element Context'==Instance"]
            },
            "description": "GetElementsWithFilter basic filtering (default External ID only)"
        },
        {
            "input": "Filter all Pipe Fittings and sum their length in element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["ABC123XYZ789", "property.name.category=contains='Pipe Fittings' and 'property.name.Element Context'==Instance", "Length"]
            },
            "description": "GetElementsWithFilter with multi-word category and additional property"
        },
        {
            "input": "Show me all door elements with their area and volume for element group TEST789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["TEST789", "property.name.category=contains=doors and 'property.name.Element Context'==Instance", "Area", "Volume"]
            },
            "description": "GetElementsWithFilter with multiple additional properties"
        },
        {
            "input": "Show me pipes with length less than 0.4 meters in element group ABC123",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["ABC123", "property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.Length<0.4"]
            },
            "description": "GetElementsWithFilter with property condition filtering"
        },
        {
            "input": "Filter walls with area between 100 and 200 in element group XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["XYZ789", "property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.area>=100 and property.name.area<200", "Area"]
            },
            "description": "GetElementsWithFilter with range condition and additional property"
        },
        
        # Contextual sequence test  
        {
            "sequence": [
                {
                    "input": "Show element groups for project urn:adsk.workspace:prod.project:GUID",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetElementGroupsByProject",
                        "parameters": ["urn:adsk.workspace:prod.project:GUID"]
                    }
                },
                {
                    "input": "Filter wall elements in element group ABC123XYZ789",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetElementsWithFilter", 
                        "parameters": ["ABC123XYZ789", "property.name.category=contains=walls and 'property.name.Element Context'==Instance"]
                    }
                }
            ],
            "description": "Contextual sequence: list models then filter elements with explicit ID"
        },

        # Priority 1 Critical Templates Test Cases
        {
            "input": "Show all models in hub urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupsByHub",
                "parameters": ["urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f"]
            },
            "description": "GetElementGroupsByHub hub-wide analysis"
        },
        {
            "input": "Survey element groups across hub urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupsByHub",
                "parameters": ["urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e"]
            },
            "description": "GetElementGroupsByHub cross-project discovery"
        },
        {
            "input": "Show version 2 of element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupByVersionNumber",
                "parameters": ["ABC123XYZ789", "2"]
            },
            "description": "GetElementGroupByVersionNumber with version number"
        },
        {
            "input": "Get element group version 1 for ABC123",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupByVersionNumber",
                "parameters": ["ABC123", "1"]
            },
            "description": "GetElementGroupByVersionNumber with version 1"
        },
        {
            "input": "What properties are available for element group TEST789?",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetPropertyDefinitionsByElementGroup",
                "parameters": ["TEST789"]
            },
            "description": "GetPropertyDefinitionsByElementGroup basic schema discovery"
        },
        {
            "input": "Show property definitions for element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetPropertyDefinitionsByElementGroup",
                "parameters": ["ABC123XYZ789"]
            },
            "description": "GetPropertyDefinitionsByElementGroup with definitions terminology"
        },
        {
            "input": "Get property schema for element group XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetPropertyDefinitionsByElementGroup",
                "parameters": ["XYZ789"]
            },
            "description": "GetPropertyDefinitionsByElementGroup with schema terminology"
        },

        # Filter-to-Highlight workflow contextual sequence
        {
            "sequence": [
                {
                    "input": "Show me all walls in element group ABC123",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetElementsWithFilter",
                        "parameters": ["ABC123", "property.name.category=contains=walls and 'property.name.Element Context'==Instance"]
                    }
                },
                {
                    "input": "Highlight elements with External IDs 1001, 1002, 1003",
                    "expected": {
                        "tool": "aps-highlight-elements",
                        "template": None,
                        "parameters": ["1001", "1002", "1003"]
                    }
                }
            ],
            "description": "Filter→highlight workflow: filter walls then highlight specific elements"
        },
        
        # Complete quantity takeoff workflow contextual sequence
        {
            "sequence": [
                {
                    "input": "List all hubs",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetHubs",
                        "parameters": []
                    }
                },
                {
                    "input": "List all projects for hub urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetProjects",
                        "parameters": ["urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f"]
                    }
                },
                {
                    "input": "Show element groups for project urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetElementGroupsByProject",
                        "parameters": ["urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef"]
                    }
                },
                {
                    "input": "Filter all doors with name Store Front Double Door in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn",
                    "expected": {
                        "tool": "aecdm-execute-graphql",
                        "template": "GetElementsWithFilter",
                        "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn", "property.name.category=contains=doors and 'property.name.Element Context'==Instance and 'property.name.Element Name'=='Store Front Double Door'"]
                    }
                }
            ],
            "description": "Complete quantity takeoff workflow: Hubs → Projects → ElementGroups → Filter specific doors by name"
        },
        
        # Viewer tool test cases
        {
            "input": "Load this model urn:adsk.wipprod:fs.file:vf.12345abc-def6-7890-ghij-klmnop123456",
            "expected": {
                "tool": "aps-viewer-render",
                "template": None,
                "parameters": ["urn:adsk.wipprod:fs.file:vf.12345abc-def6-7890-ghij-klmnop123456"]
            },
            "description": "Basic aps-viewer-render with fileVersionUrn"
        },
        {
            "input": "Render model urn:adsk.wipprod:fs.file:vf.98765fed-cba9-8765-4321-098765fedcba",
            "expected": {
                "tool": "aps-viewer-render", 
                "template": None,
                "parameters": ["urn:adsk.wipprod:fs.file:vf.98765fed-cba9-8765-4321-098765fedcba"]
            },
            "description": "aps-viewer-render with 'render model' phrasing"
        },
        
        # Simple "show me" pattern that should trigger filtering (not full workflow)
        {
            "input": "Show me all doors in element group XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["XYZ789", "property.name.category=contains=doors and 'property.name.Element Context'==Instance"]
            },
            "description": "Show me [category] - should filter to get External IDs for potential highlighting"
        },
        
        # Exchange tool test cases (using simple exchange filter syntax, NOT RSQL)
        {
            "input": "Create exchange with all the windows for element group ABC123XYZ789",
            "expected": {
                "tool": "deme-create-exchange",
                "template": None,
                "parameters": ["(category=='Windows')", "ABC123XYZ789"]
            },
            "description": "Basic exchange creation with Windows filter (simple syntax) and cached element group ID"
        },
        {
            "input": "Export the doors from element group XYZ789 named My Doors Export",
            "expected": {
                "tool": "deme-create-exchange",
                "template": None,
                "parameters": ["(category=='Doors')", "XYZ789", "My Doors Export"]
            },
            "description": "Exchange creation with custom name, Doors filter (simple syntax), and cached element group ID"
        },
        {
            "input": "Create a data exchange for walls and doors in element group TEST123",
            "expected": {
                "tool": "deme-create-exchange",
                "template": None,
                "parameters": ["(category=='Walls' or category=='Doors')", "TEST123"]
            },
            "description": "Exchange creation with complex filter (OR logic) using simple exchange syntax"
        },
        {
            "input": "Create an exchange for all the pipes with the name PipesFilteredExchange for element group PIPE123",
            "expected": {
                "tool": "deme-create-exchange",
                "template": None,
                "parameters": ["(category=='Pipes')", "PIPE123", "PipesFilteredExchange"]
            },
            "description": "Exchange creation with custom name parameter for pipes category"
        },

        # Pagination test cases - GetHubs with pagination
        {
            "input": "Show me the first 20 hubs",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": ["20"]
            },
            "description": "GetHubs with pageSize pagination parameter"
        },
        {
            "input": "List the first 10 hubs",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": ["10"]
            },
            "description": "GetHubs with pageSize using 'first' phrasing"
        },

        # Pagination test cases - GetProjects with pagination
        {
            "input": "Show me 25 projects for hub urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetProjects",
                "parameters": ["urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f", "25"]
            },
            "description": "GetProjects with hubId and pageSize pagination"
        },
        {
            "input": "List the first 50 projects for hub urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetProjects",
                "parameters": ["urn:adsk.ace:prod.scope:dccde3e3-c20c-40d3-a27c-7ac53b051b6e", "50"]
            },
            "description": "GetProjects with hubId and pageSize using 'first' phrasing"
        },

        # Pagination test cases - GetElementGroupsByProject with pagination
        {
            "input": "Show me 30 element groups for project urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementGroupsByProject",
                "parameters": ["urn:adsk.workspace:prod.project:a1b2c3d4-5678-90ef-1234-567890abcdef", "30"]
            },
            "description": "GetElementGroupsByProject with projectId and pageSize pagination"
        },

        # Pagination test cases - GetElementsWithFilter with pagination
        {
            "input": "Show me the first 100 wall elements in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ385cExvUGk4MlRheTFFaTBGVlZvMUVn", "property.name.category=contains=walls and 'property.name.Element Context'==Instance", "100"]
            },
            "description": "GetElementsWithFilter with pagination and default External ID property"
        },
        {
            "input": "Filter 75 pipe elements with their lengths in element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetElementsWithFilter",
                "parameters": ["ABC123XYZ789", "property.name.category=contains=pipes and 'property.name.Element Context'==Instance", "Length", "75"]
            },
            "description": "GetElementsWithFilter with pagination, category filter, and additional property"
        },

        # Pagination test cases - GetNumberOfElementsByCategory with pagination
        {
            "input": "Count the first 50 pipe elements in element group YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetNumberOfElementsByCategory",
                "parameters": ["YWVjZH5GRk5DV3pBTmhkam9USUdWdTNFUm9ZX0wyQ285cExvUGk4MlRheTFFaTBGVlZvMUVn", "pipe", "50"]
            },
            "description": "GetNumberOfElementsByCategory with elementGroupId, category, and pageSize"
        },

        # Pagination test cases - GetFileInformation with pagination
        {
            "input": "Get the first 25 file information entries for element group ABC123XYZ789",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetFileInformation",
                "parameters": ["ABC123XYZ789", "25"]
            },
            "description": "GetFileInformation with elementGroupId and pageSize pagination"
        },

        # Pagination test cases - Cursor continuation
        {
            "input": "Continue from cursor abc123xyz with page size 25 for hubs",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetHubs",
                "parameters": ["abc123xyz", "25"]
            },
            "description": "GetHubs with cursor continuation and pageSize"
        },
        {
            "input": "Show next page of projects for hub urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f using cursor def456ghi and page size 30",
            "expected": {
                "tool": "aecdm-execute-graphql",
                "template": "GetProjects",
                "parameters": ["urn:adsk.ace:prod.scope:ea55ab2a-b852-4fc6-9fcb-f0afea34ef4f", "def456ghi", "30"]
            },
            "description": "GetProjects with hubId, cursor continuation, and pageSize"
        }
    ]
