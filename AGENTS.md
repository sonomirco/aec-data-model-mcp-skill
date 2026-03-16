# Agents guide

This file is the working guide for coding agents in this repository.

## Startup memory

1. Check for `CLAUDE.md` at the repository root and use it first.
2. If `CLAUDE.md` does not exist, use `AGENTS.md`.

Current repo state: `CLAUDE.md` is not a separate file at root, so this file is the active source of guidance.

## What this repo is

This repository hosts an Autodesk APS GraphQL MCP server built with .NET 9 and .NET Aspire.

### Solution map

- `src/apsMcp.sln`: solution entry point.
- `src/apsMcp.AppHost`: Aspire host that orchestrates services and environment wiring.
- `src/apsMcp.SseServer`: HTTP MCP server.
- `src/apsMcp.StdioServer`: stdio MCP server.
- `src/apsMcp.Tools`: MCP tools, resources, GraphQL templates, viewer tooling, auth and caching services.
- `src/apsMcp.ServiceDefaults`: shared service defaults for telemetry, resilience, and health endpoints.
- `src/apsMcp.Evaluations`: POML + Python evaluation pipeline for tool/template selection quality.
- `src/apsMcp.Tests`: unit tests for template/query behaviors.
- `docs`: repository documentation hub (plans, reviews, references, backlog, and imported skills).

### Documentation map

- `docs/README.md`: index for documentation folders.
- `docs/backlog/ToDos.md`: active backlog and follow-up items.
- `docs/plans`: implementation and design plans.
- `docs/reviews`: solution and code review snapshots.
- `docs/reference`: technical reference markdown files.
- `skills/aps-assistant`: imported APS skill package from `aps-skill` (`SKILL.md` + `references`).

### Main tools exposed by the server

- `aecdm-execute-graphql`: execute predefined AEC data model GraphQL templates.
- `aps-viewer-render`: open viewer and load a model by `fileVersionUrn`.
- `aps-highlight-elements`: highlight elements in the currently loaded viewer.
- `deme-create-exchange`: create filtered exchanges using cached element group context.

### Core template families

- discovery: `GetHubs`, `GetProjects`, `GetElementGroupsByProject`.
- analysis: `GetNumberOfElementsByCategory`, `GetElementsWithFilter`, `GetFileInformation`.
- schema/version: `GetPropertyDefinitionsByElementGroup`, `GetElementGroupByVersionNumber`.
- exchange: `CreateExchange`.

## Why this repo exists

The goal is to provide a conversational interface to Autodesk APS data and model workflows through MCP tools.

Primary outcomes:

- enable LLMs to query APS data without custom API coding.
- support model exploration, filtering, and highlighting workflows.
- support filtered data exchange creation from model context.
- continuously evaluate prompt and tool-selection quality using structured evaluations.

## How to work in this repo

### Environment prerequisites

Create a root `.env` file with:

- `APS_CLIENT_ID`
- `APS_CLIENT_SECRET`
- `APS_CALLBACK_URL`
- `OPENAI_API_KEY` (required for evaluations)

### Build and run commands

Run from repository root:

```bash
dotnet build src/apsMcp.sln
```

```bash
dotnet test src/apsMcp.sln
```

```bash
dotnet run --project src/apsMcp.AppHost
```

```bash
dotnet run --project src/apsMcp.SseServer
```

```bash
dotnet run --project src/apsMcp.StdioServer
```

```bash
dotnet run --project src/apsMcp.Evaluations
```

### verification expectations after changes

At minimum:

1. `dotnet build src/apsMcp.sln`
2. `dotnet test src/apsMcp.sln --no-build`

When prompt or template behavior changes:

1. rerun `src/apsMcp.Evaluations`
2. confirm expected template/tool selection cases still pass
3. update `src/apsMcp.Tools/prompt.md` if `prompt.poml` changed

### coding and architecture notes

- nullable is enabled across projects; treat warnings as defects to clean up.
- keep template definitions centralized in `src/apsMcp.Tools/Configuration`.
- keep parameter mapping/transforms in service layer, not in tool entrypoints.
- prefer DI-managed services over direct instantiation.
- preserve parity between tool names in code, prompt files, tests, and documentation.

## How to maintain this file

When updating `AGENTS.md`, keep it practical and current.

### include these three perspectives

- what: technology, stack, project structure, and where code lives.
- why: purpose of each major part of the repository.
- how: exact commands, validation steps, environment setup, and working conventions.

### writing style

- use sentence case in headings and prose.
- keep instructions concrete and command-oriented.
- prefer short sections over narrative paragraphs.

### maintenance checklist

- confirm commands match real paths in this repo.
- confirm tool names match actual `[McpServerTool(Name = ...)]` values.
- remove stale references to files that do not exist.
- keep architecture map aligned with `src/apsMcp.sln`.
- update this file when adding/removing projects, tools, or evaluation workflows.

## Execplan usage

For complex features, investigations, or major refactors, use an ExecPlan from `.agents/plans.md` from design through implementation.

<!-- BEGIN COMPOUND CODEX TOOL MAP -->
## compound codex tool mapping (claude compatibility)

This section maps Claude Code plugin tool references to Codex behavior.
Only this block is managed automatically.

Tool mapping:
- Read: use shell reads (cat/sed) or rg
- Write: create files via shell redirection or apply_patch
- Edit/MultiEdit: use apply_patch
- Bash: use shell_command
- Grep: use rg (fallback: grep)
- Glob: use rg --files or find
- LS: use ls via shell_command
- WebFetch/WebSearch: use curl or Context7 for library docs
- AskUserQuestion/Question: ask the user in chat
- Task/Subagent/Parallel: run sequentially in main thread; use multi_tool_use.parallel for tool calls
- TodoWrite/TodoRead: use file-based todos in todos/ with file-todos skill
- Skill: open the referenced SKILL.md and follow it
- ExitPlanMode: ignore
<!-- END COMPOUND CODEX TOOL MAP -->
