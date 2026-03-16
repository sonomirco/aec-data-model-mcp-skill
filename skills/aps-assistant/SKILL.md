---
name: aps-assistant
description: This skill MUST be always used for any interaction with Autodesk Platform Services (APS) ACC data management via GraphQL MCP tools. It provides specialized knowledge for executing GraphQL queries, composing RSQL filters, handling URN formats, and working with 3D viewer tools. INVOKE THIS SKILL when user mentions: APS, Autodesk Platform Services, ACC, Autodesk Construction Cloud, BIM360, Revit models/files in cloud context, element groups, AEC Data Model, AECDM, or any combination of generic terms (hubs/projects/files/models/elements/viewer) WITH Autodesk/APS/ACC context.
---

# Purpose

Provide specialized assistance for Autodesk Platform Services (APS) operations, focusing on ACC (Autodesk Construction Cloud) data management through GraphQL MCP tools. Enable efficient querying of element groups, elements, properties, and 3D model viewing through proper tool selection, parameter extraction, and filter composition.

# When to use this skill

Use this skill when:
- Working with APS/ACC projects, hubs, files, or models
- Querying element data, categories, or properties
- Creating data exchanges with filtered elements
- Loading or highlighting elements in 3D viewer
- Composing RSQL filters for element queries
- Handling URN formats and ID mappings

# Available MCP tools

## Data query tools

**aecdm-execute-graphql**: Execute GraphQL queries against AEC Data Model API using predefined templates. Available templates:

- **GetHubs**: Lists all accessible hubs (no parameters required)
- **GetProjects**: Lists all projects for a specific hub (requires hubId parameter)
- **GetElementGroupsByProject**: Lists element groups/files/models for a project (requires projectId, caches fileUrn and fileVersionUrn)
- **GetNumberOfElementsByCategory**: Count elements by category in an elementGroup (requires elementGroupId, simple category name - NOT RSQL)
- **GetFileInformation**: Get file information by retrieving Project Information properties (requires elementGroupId)
- **GetElementsWithFilter**: Retrieve element summaries within an element group with optional filters and property picks (requires elementGroupId, propertyFilter, optional additional property names like Area, Length, Volume, Element Name, Family Name). The propertyFilter must be a complete RSQL expression including Element Context requirement. When passing property names as parameters, use simple array strings without quotes (e.g., "Element Name" not "'Element Name'").
- **GetElementGroupsByHub**: Survey element groups across a hub to support cross-project discovery workflows (requires hubId)
- **GetElementGroupByVersionNumber**: Retrieve element group data for a specific version number - critical for version comparison workflows (requires elementGroupId, versionNumber)
- **GetPropertyDefinitionsByElementGroup**: Retrieve property definitions (schema) for an element group - essential for property discovery and validation workflows (requires elementGroupId, optional cursor, limit)

**deme-create-exchange**: Create data exchange from source file to target folder with a specific filter (requires filter, elementGroupId from cached results, optional targetExchangeName)

## 3D viewer tools

**aps-viewer-render**: Load and render 3D models in a separate browser window (REQUIRED before highlighting, requires fileVersionUrn)

**aps-highlight-elements**: Highlight specific elements in the loaded viewer by External IDs (requires External IDs array, model must be loaded first)

# Core definitions

- **ElementGroup**: A part of an AEC project that contains elements. "Model" or "Design" is sometimes used interchangeably with "ElementGroup". ElementGroups describe Models/Files like .rvt or other file types in APS.
- **Elements**: Represents an individual piece of an elementGroup such as a wall, window, or door. The data contained in an Element gives it context by using Classification, Property, and Property Definition. An element can be filtered by its Category.
- **Element Name and Family Name**: These properties provide the Name information of an element. They are used differently depending on context:
  - When used **directly in RSQL filters**, wrap with quotes: `'property.name.Element Name'` or `'property.name.Family Name'`
  - When listed as **parameter names in arrays** (for filtering down results), pass as simple array strings without quotes: `["Element Name", "Family Name"]`

# How to use this skill

## Step 1: Analyze the user request

Identify the operation type:
- Data query (listing hubs/projects/files, counting elements, filtering elements)
- Data exchange creation (exporting filtered elements)
- 3D viewing (loading models, highlighting elements)
- Schema discovery (property definitions)

## Step 2: Select appropriate MCP tool and template

### For GraphQL data queries (aecdm-execute-graphql)

Match user intent to templates:

- "list hubs" or "show hubs" GetHubs template (no parameters)
- "list projects", "show projects", or "list all projects" GetProjects template (hubId required). If hubId not provided, use empty parameters array
- "list files/models/designs", "get element groups", "show models"  GetElementGroupsByProject template (projectId required). If projectId not provided, use empty parameters array
- "count elements", "how many elements", "element count", "number of elements"  GetNumberOfElementsByCategory template (requires elementGroupId, category)
- "file information", "project information", "get file info", "show file details" GetFileInformation template (requires elementGroupId)
- "filter elements", "show me elements", "elements with", "filter by category/type", "I want to see", "show me all [category/type]", "show me [type] with [property]" GetElementsWithFilter template (requires elementGroupId, complete RSQL propertyFilter, optional property names)
- "show all models in hub", "hub-wide analysis", "survey element groups", "cross-project discovery" GetElementGroupsByHub template (requires hubId)
- "show version", "get version", "element group version", "compare versions", "version number" GetElementGroupByVersionNumber template (requires elementGroupId, versionNumber)
- "property definitions", "schema discovery", "available properties", "property schema", "what properties" GetPropertyDefinitionsByElementGroup template (requires elementGroupId, optional cursor, limit)

### For data exchange operations (deme-create-exchange)

- "create exchange", "export elements", "create filtered export", "export with filter" deme-create-exchange tool
- Requires: filter (simple syntax, NOT RSQL), elementGroupId (from cached results), optional targetExchangeName
- After creating exchange: Inform user about location using parentFolder information from cached element group data
- Location template: "Your exchange has been created in folder '{parentFolderName}' within '{grandparentFolderName}'"

### For 3D viewer operations

- "load model", "render model", "view model" aps-viewer-render tool (requires fileVersionUrn)
- "highlight elements", "show elements" aps-highlight-elements tool (requires External IDs array, model must be loaded first)

**CRITICAL 3D viewer workflow**: When users request filtering/viewing with phrases like "Show me in viewer", "Display only", "Filter viewer", "I want to see only", "Highlight/Isolate", ALWAYS follow this EXACT sequence:

1. FIRST: Execute GetElementsWithFilter to get External IDs (compose complete RSQL filter)
2. SECOND: Load model using aps-viewer-render (with fileVersionUrn) - REQUIRED for highlighting
3. THIRD: Use aps-highlight-elements with the External IDs from step 1

NEVER skip model loading - highlighting CANNOT work without a loaded model!

## Step 3: Extract and format parameters

### URN format handling (CRITICAL)

**When to use URNs vs Raw IDs:**
- hubId parameter: Use full URN format 'urn:adsk.ace:prod.scope:GUID'
- projectId parameter: Use full URN format 'urn:adsk.workspace:prod.project:GUID'
- fileVersionUrn parameter: Use full URN format 'urn:adsk.wipprod:fs.file:vf.xxx'
- elementGroupId parameter: Use RAW ID as provided (never add URN prefix, NEVER convert to URN format, NEVER generate new IDs)

**For 3D viewing:**
- Use fileVersionUrn (format: urn:adsk.wipprod:fs.file:vf.xxx) from alternativeIdentifiers.fileVersionUrn
- Never use URN referring to hub or project
- Never use long GUID strings

### Category matching logic

Category parameters use 'contains' matching - be flexible with singular/plural and case variations.

**Common Autodesk categories:**
- Pipe Curves, Piping System, Walls, Pipe Fittings, Levels, Floors, Pipe Accessories, Pipes, Stairs Railing, Mechanical Equipment, Generic Model, Structural Foundation, MEP Spaces, Views, Specialty Equipment, Windows, Doors, Stairs, Columns, Rooms

**Matching logic:** The system uses 'contains' for category filtering, so "pipe" will match "Pipes", "Pipe Fittings", "Pipe Curves", etc.

### RSQL filter composition for GetElementsWithFilter (CRITICAL)

**Base requirements:**
- ALWAYS include Element Context: 'property.name.Element Context'==Instance
- Combine with other conditions using 'and' operator
- PREFERRED ORDER: category conditions first, then Element Context, then other properties
- NO SPACES around operators: use == not == (with spaces)

**RSQL quoting rules (MANDATORY COMPLIANCE):**
- Property names with spaces MUST be quoted: 'property.name.Element Context', 'property.name.Element Name', 'property.name.Family Name'
- Property names without spaces do NOT need quotes: property.name.category, property.name.Length
- Values with spaces MUST be quoted: 'Pipe Accessories', 'Foundation Wall', 'Store Front Double Door'
- Single word values do NOT need quotes: Instance, pipes, walls, doors
- CRITICAL: No spaces around operators like == or =contains= for clean formatting
- **NOTE ON Element Name/Family Name**: These are used WITH quotes in RSQL filters (as shown above), but WITHOUT quotes when passed as parameter names in arrays for filtering down results

**Examples:**
- WRONG: property.name.Element Context==Instance (missing quotes on property)
- WRONG: property.name.category=contains='doors' (unnecessary quotes on single word)
- WRONG: 'property.name.Element Name' == 'Store Front Double Door' (spaces around ==)
- CORRECT: 'property.name.Element Context'==Instance
- CORRECT: property.name.category=contains=doors
- CORRECT: property.name.category=contains='Pipe Fittings'
- CORRECT: 'property.name.Element Name'=='Store Front Double Door'

**RSQL operators by data type:**
- String: == (case-insensitive), =caseSensitive=, !=, =contains=, =startsWith=, =endsWith=
- Numeric: ==, !=, <, >, <=, >= (floats need decimal digits)
- Boolean: ==, !=
- DateTime: ==, !=, <, >, <=, >= (ISO 8601 format)

**Compound operations:**
- AND: condition1 and condition2
- OR: condition1 or condition2
- NOT: not(condition)
- Grouping: (condition1 or condition2) and condition3

**Common RSQL patterns:**
- Simple category: `property.name.category=contains=pipes and 'property.name.Element Context'==Instance`
- Multi-word category: `property.name.category=contains='Pipe Fittings' and 'property.name.Element Context'==Instance`
- Element name matching: `property.name.category=contains=doors and 'property.name.Element Context'==Instance and 'property.name.Element Name'=='Store Front Double Door'`
- Property range: `property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.area>=100 and property.name.area<200`
- Multiple conditions: `property.name.category=contains=pipes and 'property.name.Element Context'==Instance and property.name.Length>0.4 and 'property.name.Element Name'=contains=HVAC`
- Multiple categories: `(property.name.category=contains=walls or property.name.category=contains=doors) and 'property.name.Element Context'==Instance`

**For comprehensive filtering documentation:** See `references/filtering-guide.md` for extensive real-world examples, complete operator reference, edge cases, and best practices.

### Exchange filter syntax (NOT RSQL)

Exchange filters use a simple syntax different from RSQL:
- Format: "(category=='Windows')" - NOT RSQL format
- Supported levels: Category, Family, and Type only
- Simple filter: "(category=='Windows')", "(category=='Doors')", "(type=='Wall')"
- Complex filters: "(category=='Windows' and family=='Double Hung')" or "(category=='Doors' or category=='Windows')"
- Do NOT use RSQL operators like 'property.name.category=contains=' in exchange filters

## Step 4: Handle template hierarchy and backwards retrieval

**Core workflow:**
GetHubs -> GetProjects [hubId] -> GetElementGroupsByProject [projectId] -> GetElementsWithFilter [elementGroupId, propertyFilter]

**Alternative workflows:**
- Hub-wide: GetHubs -> GetElementGroupsByHub [hubId]
- Version: GetElementGroupsByProject [projectId] -> GetElementGroupByVersionNumber [elementGroupId, versionNumber]
- Schema: GetElementGroupsByProject [projectId] -> GetPropertyDefinitionsByElementGroup [elementGroupId]
- Exchange: GetElementGroupsByProject [projectId] -> deme-create-exchange [elementGroupId, filter]

**GetElementGroupsByProject also provides:**
- fileVersionUrn for aps-viewer-render
- fileUrn for deme-create-exchange

**If a required parameter is missing:** Work backwards through the hierarchy to retrieve it.

Example: Need elementGroupId but only have file name?
-> GetHubs -> GetProjects -> GetElementGroupsByProject -> find the file

## Bundled resources

### references/filtering-guide.md

Comprehensive RSQL filtering guide with real-world scenarios organized by domain (Architecture, MEP, Structural, QA/QC), complete operator reference, performance tips, and troubleshooting. Load this when constructing complex filters, exploring real-world examples, or handling edge cases.

# Key reminders

1. ALWAYS preserve complete URN formats - never extract just the GUID portion
2. elementGroupId must be RAW ID, never convert to URN format
3. RSQL filters MUST include Element Context requirement
4. No spaces around RSQL operators
5. Quote multi-word property names and values in RSQL
6. Exchange filters use simple syntax, NOT RSQL
7. 3D viewer requires model loading BEFORE highlighting
8. Use 'contains' matching for categories (flexible with singular/plural)
