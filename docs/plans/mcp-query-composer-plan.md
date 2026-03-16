# MCP Query Composer PoC Plan

## Objectives
- Add a server-side tool that can compose bespoke AEC Data Model GraphQL queries using cached schema knowledge without overloading MCP clients.
- Keep deterministic template execution as the default pathway and fall back to synthesis when templates do not satisfy the request.
- Extend evaluation coverage and orchestration logic to route intelligently between templates and synthetic queries.

## Phase 1 - Baseline & Gap Analysis
1. Catalogue existing GraphQL templates (code + docs + `docs/reference/AecGraphQlQueries.md`).
2. Map recognized user intents to current templates and flag uncovered patterns.
3. Review recent interaction logs (if available) to identify high-value gaps.
4. Produce a short report of priority scenarios for the PoC to target.

## Phase 2 - Query Composer Tool PoC
1. **Interface Design**
   - Define tool signature (inputs: intent, optional parameters or context; outputs: query string, rationale, assumptions).
   - Confirm coexistence with `aecdm-execute-graphql` and shared auth or token services (`src/apsMcp.Tools/GraphQlTools.cs`).
2. **Schema Retrieval Layer**
   - Pre-process `C:\Users\BianchiniM\Desktop\AecGraphQlObjectSchema.json` into searchable chunks (for example, JSONPath map or vector index stored locally).
   - Implement helper services to fetch only relevant schema fragments based on intent keywords or entities.
3. **Template Fallback Logic**
   - Use the gap report to attempt template matches first; invoke the existing template executor when possible.
4. **Query Synthesis**
   - Construct a prompt or deterministic builder that combines user intent and retrieved schema snippets.
   - Enforce guardrails (field allowlists, depth limits, parameter validation, rate limiting).
   - Provide dry-run validation using the GraphQL client's introspection or lint capabilities before returning results.
5. **Caching and Reuse**
   - Cache synthesized queries with context metadata for auditing and future template promotion.
6. Document implementation decisions, prompt iterations, and integration points.

## Phase 3 - Evaluation and Testing
1. Extend POML evaluation scenarios to cover:
   - Successful template routing for known intents.
   - Synthetic query generation for uncovered intents.
   - Failure handling when schema retrieval is ambiguous or insufficient.
2. Add unit or integration tests for schema chunking helpers, tool responses, and validation failures.
3. Track metrics (precision of field selection, validation error rate, latency) for PoC assessment.

## Phase 4 - Planner and Client Integration
1. Prototype a planner prompt (server-side or client workflow) that decides between template execution and the composer tool based on intent classification confidence or telemetry.
2. Update MCP resources or developer docs to explain how clients (Claude Desktop or custom) should request synthesized queries and interpret responses.
3. For custom clients, design optional schema-elicitation messages so they can request deep schema context on demand without bloating default prompts.

## Phase 5 - Deployment and Observability
1. Wrap the new tool behind a feature flag or configuration toggle for gradual rollout.
2. Instrument structured logging for tool selection, schema fragments referenced, validation outcomes, and cache hits.
3. Define rollback procedures (disable composer tool, revert to template-only mode, flush caches).
4. Publish release notes and adoption guidelines for downstream teams or users.

## Deliverables
- Updated tool implementation in `apsMcp.Tools` with supporting services and utilities.
- Schema retrieval or chunking assets stored alongside tooling or in a dedicated data directory.
- Expanded POML evaluation suite and automated tests covering composer behavior.
- Planner or client integration documentation and example prompts or workflows.
- Deployment checklist, logging dashboard pointers, and monitoring plan.
