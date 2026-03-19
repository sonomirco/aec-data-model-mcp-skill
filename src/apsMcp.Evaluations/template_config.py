"""
Template configuration for GraphQL template validation.
This centralized configuration drives the evaluation system,
making it easy to add new templates without code changes.
"""

# GraphQL Template Configuration
TEMPLATE_CONFIGS = {
    "GetHubs": {
        "parameter_count": 0,
        "parameters": [],
        "description": "List all accessible Autodesk Construction Cloud hubs"
    },
    "GetProjects": {
        "parameter_count": 1,
        "parameters": ["hubId"],
        "description": "List projects for a specific hub"
    },
    "GetElementGroupsByProject": {
        "parameter_count": 1,
        "parameters": ["projectId"],
        "description": "List element groups for a specific project"
    },
    "GetNumberOfElementsByCategory": {
        "parameter_count": 2,
        "parameters": ["elementGroupId", "category"],
        "description": "Count elements by category in an element group",
        "parameter_transformations": {
            "category": {
                "graphql_variable": "propertyFilter",
                "transformation": "category_to_rsql_filter",
                "description": "Transforms simple category name into RSQL filter format"
            }
        },
        "semantic_wrapper": True
    },
    "GetElementsWithFilter": {
        "parameter_count": "variable",  # Minimum 2, supports additional property names
        "min_parameters": 2,
        "parameters": ["elementGroupId", "propertyFilter"],
        "optional_parameters": ["propertyName1", "propertyName2", "..."],
        "description": "Retrieve element summaries within an element group with optional filters and property picks (enables highlighting workflow)",
        "parameter_transformations": {
            "additional_params": {
                "graphql_variable": "propertyNames",
                "transformation": "build_property_names_array",
                "description": "Combines External ID (always included) with additional property names"
            }
        },
        "variable_parameters": True,
        "rsql_composition": True
    },
    "GetFileInformation": {
        "parameter_count": 1,
        "parameters": ["elementGroupId"],
        "description": "Get file information by retrieving Project Information category properties, filtered for properties starting with 'Project' and non-null values"
    },
    # Priority Templates - Enhanced Capabilities
    "GetElementGroupsByHub": {
        "parameter_count": 1,
        "parameters": ["hubId"],
        "description": "Survey element groups across a hub to support cross-project discovery workflows"
    },
    "GetElementGroupByVersionNumber": {
        "parameter_count": 2,
        "parameters": ["elementGroupId", "versionNumber"],
        "description": "Retrieve element group data for a specific version number - critical for version comparison workflows"
    },
    "GetPropertyDefinitionsByElementGroup": {
        "parameter_count": "variable",  # Minimum 1, supports optional pagination parameters
        "min_parameters": 1,
        "parameters": ["elementGroupId"],
        "optional_parameters": ["cursor", "limit"],
        "description": "Retrieve property definitions (schema) for an element group - essential for property discovery and validation workflows",
        "variable_parameters": True,
        "pagination_support": True
    }
}

# Viewer Tool Configuration
VIEWER_TOOL_CONFIGS = {
    "aps-viewer-render": {
        "parameter_count": 1,
        "parameters": ["fileVersionUrn"],
        "template": None,
        "validation": "fileVersionUrn",  # Special validation type
        "description": "Load and render 3D models in a separate browser window"
    },
    "aps-highlight-elements": {
        "parameter_count": "variable",  # Can have multiple External IDs
        "parameters": ["externalIds"],
        "template": None,
        "validation": "externalIds",
        "description": "Highlight specific elements in the loaded viewer by External IDs (requires model loaded first, often follows GetElementsByProperties)"
    }
}

# Exchange Tool Configuration  
EXCHANGE_TOOL_CONFIGS = {
    "deme-create-exchange": {
        "parameter_count": "variable",  # 2 minimum, 3 with optional name
        "min_parameters": 2,
        "parameters": ["filter", "elementGroupId"],
        "optional_parameters": ["targetExchangeName"],
        "template": None,
        "validation": "exchange_filter",
        "description": "Create data exchange from source file to target folder with specific filter using cached element group information"
    }
}

def get_all_template_names() -> list:
    """Get list of all GraphQL template names for schema generation"""
    return list(TEMPLATE_CONFIGS.keys())

def get_all_tool_names() -> list:
    """Get list of all tool names for schema generation"""
    return ["aecdm-execute-graphql"] + list(VIEWER_TOOL_CONFIGS.keys()) + list(EXCHANGE_TOOL_CONFIGS.keys())

def get_template_config(template_name: str) -> dict:
    """Get configuration for a specific template"""
    return TEMPLATE_CONFIGS.get(template_name, {})

def get_viewer_tool_config(tool_name: str) -> dict:
    """Get configuration for a specific viewer tool"""
    return VIEWER_TOOL_CONFIGS.get(tool_name, {})

def get_exchange_tool_config(tool_name: str) -> dict:
    """Get configuration for a specific exchange tool"""
    return EXCHANGE_TOOL_CONFIGS.get(tool_name, {})
