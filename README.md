# APS GraphQL MCP

## Overview

This project provides a chat-friendly gateway to Autodesk Platform Services (APS) data using GraphQL. It runs as an MCP (Model Context Protocol) server so an AI assistant can browse your hubs and projects, discover models, answer questions about their contents, and help you visualize results — all without requiring end users to know APIs or query languages.

Think of it as a set of reusable “skills” an assistant can call to query APS AEC Data Model and related Graph GraphQL endpoints, open a 3D viewer, and highlight elements that match your request.

## What You Can Do

- Explore organizations and projects: List hubs and drill into projects.
- Discover models and context: Find element groups/files and their basic details.
- Ask questions about models: Count elements by category, retrieve file information, or filter elements by properties.
- Create filtered exports: Build data exchanges from a model using simple, human-readable filters.
- View models in 3D: Open a model in a browser-based viewer.
- Highlight what matters: Isolate specific elements in the viewer by their External IDs.
- Sign in once: The assistant handles authentication when needed.
- Get responsive answers: Results are cached to keep common queries quick.

## Typical Flows

- “Show my hubs → pick a project → list its models”: Browse your data in a few steps.
- “How many walls are in this model?”: Get fast counts by category.
- “Show windows with a certain property, then highlight them in the viewer”: Filter first, view, then isolate exactly what you asked for.
- “Create an exchange of doors from this model”: Generate a filtered export so downstream tools can use the subset you care about.

## Who It’s For

Teams who want a simple, conversational way to explore APS construction data, ask targeted questions about models, and visualize the results — without writing queries or scripts.

## APS support knowledge

This repository also includes an APS skill package at `skills/aps-assistant` that works as support knowledge for MCP usage.

It helps assistants use the MCP server more reliably by defining:

- intent-to-tool and template routing (for example `GetHubs`, `GetProjects`, `GetElementsWithFilter`, exchange creation, and viewer tools)
- parameter and identifier rules (including when to use URNs vs raw `elementGroupId`)
- RSQL construction guidance for element filtering (including Element Context and quoting rules)
- required 3D viewer execution sequence (`aps-viewer-render` before `aps-highlight-elements`)

## presentation package

Presentation and research material is available under `docs/presentation`:

- [docs/presentation/presentation-outline.md](docs/presentation/presentation-outline.md): main presentation storyline and narrative flow.
- [docs/presentation/reference-library.md](docs/presentation/reference-library.md): single consolidated references and research file.

## Running the MCP server

The server supports two connection modes:

- HTTP server (`src/apsMcp.SseServer`) with streamable HTTP (preferred) and legacy SSE compatibility
- stdio executable (`src/apsMcp.StdioServer`)

Required environment variables for both modes:

- `APS_CLIENT_ID`
- `APS_CLIENT_SECRET`
- `APS_CALLBACK_URL`

### HTTP mode (for remote clients)

Build and run the SSE server:

```bash
dotnet build src/apsMcp.sln
dotnet run --project src/apsMcp.SseServer --launch-profile http
```

Preferred streamable HTTP endpoint with `http` launch profile:

```text
http://localhost:5096/
```

If you run with the `https` profile, use:

```text
https://localhost:7270/
```

Legacy SSE endpoint (for clients that explicitly require SSE transport):

```text
http://localhost:5096/sse
https://localhost:7270/sse
```

The Aspire MCP inspector is configured for streamable HTTP and should target the root endpoint (`/`).

### stdio mode (for local executable clients)

Create publish artifacts for Windows and macOS:

```bash
dotnet publish src/apsMcp.StdioServer/apsMcp.StdioServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/stdio/win-x64
dotnet publish src/apsMcp.StdioServer/apsMcp.StdioServer.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/stdio/osx-x64
```

Published executable locations:

- Windows: `artifacts\stdio\win-x64\apsMcp.StdioServer.exe`
- macOS: `artifacts/stdio/osx-x64/apsMcp.StdioServer`

Use those executable paths in your MCP client configuration.

## Client configuration examples

### Claude Code with SSE (legacy via mcp-remote)

```json
{
  "mcpServers": {
    "aspire-server": {
      "command": "npx",
      "args": [
        "mcp-remote",
        "http://localhost:5096/sse"
      ],
      "env": {
        "APS_CLIENT_ID": "your_client_id",
        "APS_CLIENT_SECRET": "your_client_secret",
        "APS_CALLBACK_URL": "http://localhost:8080/api/auth/callback"
      }
    }
  }
}
```

### Claude Code with stdio executable

```json
{
  "mcpServers": {
    "aps-stdio": {
      "command": "C:\\path\\to\\artifacts\\stdio\\win-x64\\apsMcp.StdioServer.exe",
      "env": {
        "APS_CLIENT_ID": "your_client_id",
        "APS_CLIENT_SECRET": "your_client_secret",
        "APS_CALLBACK_URL": "http://localhost:8080/api/auth/callback"
      }
    }
  }
}
```

On macOS, set `command` to `/path/to/artifacts/stdio/osx-x64/apsMcp.StdioServer`.

### Codex with SSE (legacy via mcp-remote bridge)

```bash
codex mcp add aspire-server --env APS_CLIENT_ID=your_client_id --env APS_CLIENT_SECRET=your_client_secret --env APS_CALLBACK_URL=http://localhost:8080/api/auth/callback -- npx mcp-remote http://localhost:5096/sse
```

### Codex with stdio executable

```bash
codex mcp add aps-stdio --env APS_CLIENT_ID=your_client_id --env APS_CLIENT_SECRET=your_client_secret --env APS_CALLBACK_URL=http://localhost:8080/api/auth/callback -- C:\path\to\artifacts\stdio\win-x64\apsMcp.StdioServer.exe
```

On macOS, use the published binary path:

```text
/path/to/artifacts/stdio/osx-x64/apsMcp.StdioServer
```

Equivalent Codex config (`~/.codex/config.toml`):

```toml
[mcp_servers.aps_stdio]
command = "C:\\path\\to\\artifacts\\stdio\\win-x64\\apsMcp.StdioServer.exe"
env = { APS_CLIENT_ID = "your_client_id", APS_CLIENT_SECRET = "your_client_secret", APS_CALLBACK_URL = "http://localhost:8080/api/auth/callback" }
```

## Learn More

- APS AEC Data Model (Beta): https://aps.autodesk.com/en/docs/aecdatamodel-beta/v1/developers_guide/overview/
- APS AEC Data Model (GA): https://aps.autodesk.com/en/docs/aecdatamodel/v1/developers_guide/overview/
- APS FDX Graph: https://aps.autodesk.com/en/docs/fdxgraph/v1/developers_guide/overview/
- Reference MCP implementation (C#): https://github.com/autodesk-platform-services/aps-aecdm-mcp-dotnet
- MCP background (layers): https://engineering.block.xyz/blog/build-mcp-tools-like-ogres-with-layers
- APS GraphQL tutorial: https://autodesk-platform-services.github.io/aps-dx-graphql-tutorial/
- POML language (prompt design): https://microsoft.github.io/poml/latest/
- CSnakes (.NET ↔ Python interop): https://tonybaloney.github.io/CSnakes/v1/
- OpenAI Cookbook (prompting ideas): https://cookbook.openai.com/
- Prompt evaluations example: https://github.com/anthropics/courses/blob/master/prompt_evaluations/01_intro_to_evals/01_intro_to_evals.ipynb

---

For contributors: see `AGENTS.md` for architecture and workflow details.
