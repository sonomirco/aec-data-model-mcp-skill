# POML Prompt Structure Review Plan

## Executive Summary

This document provides a detailed plan for reviewing and improving the current `prompt.poml` structure based on Anthropic's recommended best practices for complex prompts. The analysis reveals areas for structural enhancement while maintaining the domain-specific functionality required for the APS GraphQL MCP server.

## Current Structure Analysis

### Current POML Structure
```
1. <role> - User Role Definition
2. <task> - Basic Task Description
3. <stepwise-instructions> - Processing Steps
4. <cp> sections - Multiple Context/Reference Blocks:
   - Available MCP tools
   - GraphQL templates
   - Exchange templates
   - 3D viewer workflow
   - APS definitions
   - Category matching logic
   - URN format mappings
   - RSQL Filter Composition
   - Comprehensive decision logic
```

### Anthropic Recommended Structure
```
1. User Role
2. Task Context
3. Tone Context
4. Detailed Task Description and Rules
5. Examples
6. Input Data
7. Immediate Task Description
8. Precognition (Step-by-Step Thinking)
9. Output Formatting
10. Prefill (Optional)
```

## Gap Analysis

### Current Strengths
- **User Role**: Well-defined specialist role for APS assistant
- **Task Description**: Clear primary task definition
- **Processing Workflow**: Basic stepwise instructions present but could be enhanced
- **Detailed Context**: Extensive domain-specific knowledge blocks
- **Output Formatting**: Implicit through evaluation system requirements

### Missing Elements
- **Task Context**: No high-level context about the MCP server environment
- **Tone Context**: No guidance on response style or communication approach
- **Examples**: No concrete examples of successful tool calls and responses
- **Input Data Structure**: No clear definition of expected input formats
- **Immediate Task Description**: Task description is buried in complex structure
- **Enhanced Precognition**: Step-by-step instructions need more structured cognitive organization
- **Clear Section Ordering**: Information architecture doesn't follow logical flow

### Areas for Improvement
- **Information Architecture**: Better organization of reference materials
- **XML Tag Structure**: More semantic and consistent tagging
- **Logical Flow**: Reorganize for better cognitive processing
- **Examples Integration**: Add concrete examples throughout sections

## Recommended Restructuring Plan

### Phase 1: Core Structure Reorganization (Week 1)

#### 1.1 Implement POML Best Practices with Enhanced Structure

Based on Microsoft's POML documentation and best practices, the restructured prompt should follow this enhanced architecture:

```xml
<poml>
  <!-- POML Header with metadata -->
  <head>
    <title>APS GraphQL MCP Assistant</title>
    <stylesheet>
      {
        "example": { "captionStyle": "header", "captionEnding": "colon-newline" },
        "stepwise-instructions": { "listStyle": "decimal" },
        "reference-materials": { "syntax": "markdown" }
      }
    </stylesheet>
  </head>

  <body>
    <!-- 1. User Role (enhanced with POML role component) -->
    <role>
      You are a specialized APS (Autodesk Platform Services) GraphQL assistant operating within an MCP (Model Context Protocol) server environment.
    </role>

    <!-- 2. Task Context (using POML semantic components) -->
    <task>
      Analyze user requests and provide precise MCP tool calls for Autodesk APS GraphQL operations, including parameter extraction and multi-step workflow orchestration.
    </task>

    <!-- 3. Tone Context (NEW - using dedicated tone component) -->
    <tone>
      Professional technical accuracy with clear parameter extraction and systematic step-by-step reasoning for complex APS workflows.
    </tone>

    <!-- 4. Input Data Structure (NEW - defines expected input formats) -->
    <input-data>
      <format>User requests in natural language containing operation types and entity references</format>
      <operation-indicators>list, show, filter, count, create, view, highlight, render, exchange</operation-indicators>
      <entity-patterns>hub URNs (urn:adsk.ace:prod.scope:*), project URNs, element group IDs, category names</entity-patterns>
      <context-clues>Previous conversation context may contain cached entity references</context-clues>
    </input-data>

    <!-- 5. Immediate Task Description (NEW - clear, focused task) -->
    <immediate-task>
      Parse the user's natural language request and respond with a single JSON object containing the appropriate MCP tool, template, and extracted parameters for executing the requested APS GraphQL operation.
    </immediate-task>

    <!-- 6. Stepwise Instructions (using POML stepwise-instructions) -->
    <stepwise-instructions>
      <list listStyle="decimal">
        <item>Analyze request for operation type indicators (list, show, filter, count, create, view, highlight)</item>
        <item>Identify required entities (hubs, projects, element groups, elements, exchanges)</item>
        <item>Select appropriate tool based on operation + entity combination</item>
        <item>Extract parameters using URN format rules and semantic parsing</item>
        <item>Validate parameter completeness and format compliance</item>
        <item>For complex operations, plan multi-step workflow sequence</item>
        <item>Format response according to MCP evaluation schema requirements</item>
      </list>
    </stepwise-instructions>

    <!-- 7. Examples (using POML examples with chat context) -->
    <examples chat="false">
      <example caption="Basic Hub Listing">
        <input>Show me all hubs</input>
        <output>{"tool": "aecdm-execute-graphql", "template": "GetHubs", "parameters": []}</output>
      </example>

      <example caption="Parameter Extraction">
        <input>List projects for hub urn:adsk.ace:prod.scope:12345</input>
        <output>{"tool": "aecdm-execute-graphql", "template": "GetProjects", "parameters": ["urn:adsk.ace:prod.scope:12345"]}</output>
      </example>

      <example caption="Complex RSQL Filtering">
        <input>Filter walls with area greater than 100</input>
        <output>{"tool": "aecdm-execute-graphql", "template": "GetElementsWithFilter", "parameters": ["elementGroupId", "property.name.category=contains=walls and 'property.name.Element Context'==Instance and property.name.area#gt;100"]}</output>
      </example>
    </examples>

    <!-- 8. Reference Materials (using POML let variables and document inclusion) -->
    <let name="toolCatalog" value="Available MCP tools and GraphQL templates" />
    <let name="dataFormats" value="URN mappings, category logic, APS definitions" />
    <let name="queryComposition" value="RSQL filters and parameter handling rules" />

    <!-- 9. Output Schema (using POML output-schema) -->
    <output-schema parser="eval">
      z.object({
        tool: z.enum(["aecdm-execute-graphql", "aps-viewer-render", "aps-highlight-elements", "aecdm-create-exchange"]),
        template: z.enum(["GetHubs", "GetProjects", "GetElementGroupsByProject", "GetNumberOfElementsByCategory", "GetElementsWithFilter", "CreateExchange"]),
        parameters: z.array(z.string())
      })
    </output-schema>

    <!-- 10. Hints for complex scenarios -->
    <hint>For viewer workflows: Filter -> Load Model -> Highlight sequence required</hint>
    <hint>Category and type are interchangeable terms (e.g., "walls" = "wall type")</hint>
    <hint>Use complete URN formats for hub/project IDs, raw IDs for elementGroupId</hint>
  </body>
</poml>
```

#### 1.2 POML-Specific Enhancements

The new structure leverages several advanced POML features:

**Semantic Components**: Using `<role>`, `<task>`, `<examples>`, `<stepwise-instructions>` instead of generic `<cp>` sections
**Stylesheet Integration**: JSON-based styling for consistent formatting across components
**Template Engine**: Variables with `<let>` for reusable content blocks
**Output Schema**: Zod-based schema validation for guaranteed JSON compliance
**Contextual Examples**: Examples with captions and structured input/output pairs
**Hint System**: Contextual hints for complex scenarios

#### 1.3 Reorganize Reference Materials
Transform current `<cp>` sections into logical groupings:

**Before** (9 separate `<cp>` sections):
- Available MCP tools
- GraphQL templates
- Exchange templates
- 3D viewer workflow
- APS definitions
- Category matching logic
- URN format mappings
- RSQL Filter Composition
- Comprehensive decision logic

**After** (4 semantic sections):
```xml
<reference-materials>
  <tool-catalog>
    <!-- Consolidate MCP tools, templates, exchange templates -->
  </tool-catalog>

  <data-formats>
    <!-- URN mappings, category logic, APS definitions -->
  </data-formats>

  <query-composition>
    <!-- RSQL filters, parameter handling -->
  </query-composition>

  <workflow-patterns>
    <!-- Decision logic, viewer workflows, template hierarchy -->
  </workflow-patterns>
</reference-materials>
```

### Phase 2: POML Content Enhancement (Week 2)

#### 2.1 Enhanced Examples with POML Features

**Dynamic Example Selection using POML Conditionals:**
```xml
<examples>
  <!-- Context-aware example selection -->
  <if condition="context.operationType === 'listing'">
    <example caption="Basic Hub Listing" type="listing">
      <input>Show me all hubs</input>
      <output>{"tool": "aecdm-execute-graphql", "template": "GetHubs", "parameters": []}</output>
    </example>
  </if>

  <!-- Template-driven parameter examples -->
  <example for="template in supportedTemplates" caption="Template: {{ template.name }}">
    <input>{{ template.sampleQuery }}</input>
    <output>{"tool": "{{ template.tool }}", "template": "{{ template.name }}", "parameters": {{ template.sampleParams }}}</output>
  </example>

  <!-- Multi-step workflow examples with POML list structure -->
  <example caption="Multi-Step Viewer Workflow" type="workflow">
    <input>Show me all windows in the model</input>
    <stepwise-instructions>
      <list listStyle="decimal">
        <item>Filter elements: GetElementsWithFilter</item>
        <item>Load model: aps-viewer-render</item>
        <item>Highlight elements: aps-highlight-elements</item>
      </list>
    </stepwise-instructions>
    <output>{"workflow": "multi-step", "steps": [...]}</output>
  </example>
</examples>
```

**POML Document Inclusion for Reference Materials:**
```xml
<!-- Include external reference files -->
<document src="aps-tools-catalog.md" parser="markdown" />
<document src="graphql-templates.json" parser="json" />
<document src="urn-format-rules.txt" parser="text" />
```

#### 2.2 Add Explicit Thinking Process
```xml
<thinking-process>
  <step>Analyze user request for operation type indicators (list, show, filter, count, create, view, highlight)</step>
  <step>Identify required entities (hubs, projects, element groups, elements, exchanges)</step>
  <step>Determine tool selection based on operation + entity combination</step>
  <step>Extract parameters using URN format rules and semantic parsing</step>
  <step>Validate parameter completeness and format compliance</step>
  <step>For complex operations, plan multi-step workflow sequence</step>
  <step>Format response according to evaluation schema requirements</step>
</thinking-process>
```

#### 2.3 Define Clear Output Formatting
```xml
<output-format>
  <schema>
    Required JSON structure: {"tool": "enum", "template": "enum", "parameters": ["string"]}
  </schema>

  <parameter-rules>
    - Use complete URN formats for hub/project IDs
    - Use raw IDs for elementGroupId (never URN format)
    - Preserve exact parameter order as defined in template requirements
    - Include all required parameters, exclude optional ones unless specified
  </parameter-rules>

  <error-handling>
    If parameters cannot be extracted: Use empty parameters array []
    If template unclear: Default to most general applicable template
    If tool uncertain: Prefer aecdm-execute-graphql for data operations
  </error-handling>
</output-format>
```

### Phase 3: Validation and Testing (Week 3)

#### 3.1 Evaluation System Integration
- Update evaluation templates to test new structure
- Validate examples against test cases
- Ensure backward compatibility with existing functionality

#### 3.2 Performance Testing
- Compare prompt token usage before/after restructuring
- Measure evaluation pass rates with new structure
- Test edge cases and complex scenarios

#### 3.3 Documentation Updates
- Update CLAUDE.md with new prompt structure information
- Document migration process and rationale
- Create comparison metrics (before/after)

### Phase 4: Advanced Enhancements (Week 4)

#### 4.1 Dynamic Examples
Consider implementing example selection based on request type:
```xml
<examples context="category-filtering">
  <!-- Show category-specific examples -->
</examples>

<examples context="viewer-workflow">
  <!-- Show viewer-specific examples -->
</examples>
```

#### 4.2 Contextual Precognition
Add request-type-specific thinking processes:
```xml
<thinking-process context="complex-filtering">
  <step>Parse filter requirements (category, properties, ranges)</step>
  <step>Compose RSQL with proper quoting and operators</step>
  <step>Validate RSQL syntax and Element Context inclusion</step>
</thinking-process>
```

#### 4.3 Enhanced Error Guidance
```xml
<error-recovery>
  <scenario type="missing-parameter">
    <guidance>Work backwards through template hierarchy to retrieve missing context</guidance>
    <example>Need elementGroupId -> GetElementGroupsByProject -> find by name/pattern</example>
  </scenario>

  <scenario type="invalid-urn">
    <guidance>Verify URN format against mapping rules</guidance>
    <example>Hub URNs must use 'urn:adsk.ace:prod.scope:' prefix</example>
  </scenario>
</error-recovery>
```

## POML-Enhanced Implementation Strategy

### Technical Approach with POML Tooling

#### Phase A: POML Infrastructure Setup
1. **POML Environment**: Set up POML development environment with VS Code extension
2. **Template Migration**: Convert existing `prompt.poml` to enhanced POML structure
3. **Validation Pipeline**: Implement POML syntax validation in evaluation system
4. **Documentation Generation**: Auto-generate markdown from POML using `poml.write()`

#### Phase B: Enhanced Development Workflow
1. **POML Tracing**: Integrate POML tracing for evaluation debugging
2. **Context Variables**: Use POML context system for dynamic prompt generation
3. **Schema Validation**: Leverage POML output-schema for guaranteed compliance
4. **Component Testing**: Test individual POML components in isolation

#### Phase C: Advanced POML Features
1. **Dynamic Content**: Implement conditional examples based on request context
2. **Template Engine**: Use POML variables for reusable content blocks
3. **Stylesheet System**: Apply consistent formatting across prompt components
4. **Document Inclusion**: Modularize reference materials into separate files

### POML-Specific Benefits
- **Structured Editing**: VS Code extension provides syntax highlighting and validation
- **Component Reusability**: Modular components can be shared across different prompts
- **Type Safety**: Output schema validation prevents malformed responses
- **Debugging**: Built-in tracing system for prompt execution analysis
- **Version Control**: POML files are git-friendly and support meaningful diffs

### Legacy Migration Strategy
1. **Incremental Migration**: Gradually migrate from text-based prompt to POML structure
2. **A/B Testing**: Compare old vs new structure performance using evaluation metrics
3. **Backward Compatibility**: Maintain existing evaluation tests during transition
4. **Rollback Plan**: Keep current prompt.poml as backup with version tagging

### Success Metrics
- **Evaluation Pass Rate**: Target >85% (current baseline measurement needed)
- **Response Quality**: Improved parameter extraction accuracy
- **Maintainability**: Easier addition of new templates and examples
- **Token Efficiency**: Maintain or improve prompt token usage

### Risk Mitigation
- **Rollback Plan**: Keep current prompt.poml as backup
- **Gradual Rollout**: Test with subset of evaluation cases first
- **Documentation**: Comprehensive change log and migration notes

## Timeline and Resources

### Week 1: Core Restructuring
- **Effort**: 16-20 hours
- **Tasks**: Reorganize sections, implement new ordering
- **Deliverable**: Restructured prompt.poml with new architecture

### Week 2: Content Enhancement
- **Effort**: 12-16 hours
- **Tasks**: Add examples, thinking process, output formatting
- **Deliverable**: Enhanced prompt with comprehensive examples

### Week 3: Validation and Testing
- **Effort**: 8-12 hours
- **Tasks**: Evaluation testing, performance comparison
- **Deliverable**: Validated prompt with test results

### Week 4: Advanced Features
- **Effort**: 8-10 hours
- **Tasks**: Dynamic examples, contextual enhancements
- **Deliverable**: Production-ready optimized prompt

**Total Estimated Effort**: 44-58 hours over 4 weeks (including POML setup and learning)

### POML Development Workflow

#### Setup Requirements
1. **VS Code Extension**: Install POML extension for syntax highlighting and validation
2. **Python Environment**: Ensure Python environment with POML library installed
3. **Node.js Setup**: For JavaScript/TypeScript POML integration if needed
4. **OpenAI API**: Configure API key for POML evaluation testing

#### Development Process
```python
# Example POML development workflow
import poml
from openai import OpenAI

# Enable tracing for debugging
poml.set_trace(trace_dir="pomlruns")

# Load and test prompt
client = OpenAI()
params = poml.poml("src/apsMcp.Evaluations/prompt.poml",
                   context={"operationType": "listing"},
                   format="openai_chat")

# Test with evaluation system
response = client.chat.completions.create(model="gpt-4o-mini", **params)
```

#### VS Code Integration Benefits
- **Real-time Validation**: Syntax errors highlighted immediately
- **Context-aware Editing**: Intellisense for POML components
- **Preview Mode**: See rendered prompt alongside source
- **Testing Integration**: Test prompts directly from editor
- **Git Integration**: Meaningful diffs and change tracking

## Conclusion

The current prompt.poml structure contains comprehensive domain knowledge but lacks both the cognitive organization principles recommended by Anthropic and the advanced capabilities offered by Microsoft's POML framework. By implementing this enhanced restructuring plan, we can achieve:

### Core Improvements
1. **Logical Flow**: Better information architecture following Anthropic's recommended ordering
2. **Learning Examples**: Context-aware examples using POML's conditional system
3. **Explicit Reasoning**: Structured stepwise instructions with POML components
4. **Type Safety**: Output schema validation preventing malformed responses
5. **Performance**: Optimized token usage and guaranteed evaluation compliance

### POML-Specific Advantages
1. **Development Experience**: VS Code integration with syntax highlighting and validation
2. **Modular Architecture**: Reusable components and document inclusion capabilities
3. **Dynamic Content**: Template engine with variables and conditional rendering
4. **Debugging Support**: Built-in tracing system for prompt execution analysis
5. **Maintainability**: Git-friendly structure with meaningful version control diffs

### Strategic Benefits for APS GraphQL MCP
1. **Scalability**: Easy addition of new GraphQL templates through configuration
2. **Consistency**: Stylesheet system ensures uniform formatting across components
3. **Validation**: Schema enforcement prevents evaluation system failures
4. **Modularity**: Reference materials can be maintained as separate, versioned files
5. **Testing**: Individual components can be tested and validated independently

### Implementation Confidence
The phased approach leverages POML's robust tooling ecosystem while maintaining backward compatibility. The enhanced structure not only follows Anthropic's cognitive optimization principles but also provides a future-proof foundation for prompt evolution using industry-standard markup language practices.

This represents a significant upgrade from generic XML tags to semantic, validated, and tooling-supported prompt engineering that will improve both development velocity and evaluation reliability.





