# APS MCP stdio server - Quick setup

A Model Context Protocol (MCP) server for Autodesk Platform Services providing GraphQL tools for ACC data, 3D viewing, and element filtering.

## Quick setup

### 1. Get APS credentials

Get your credentials from the [Autodesk Developer Portal](https://aps.autodesk.com/myapps/):

- Client ID
- Client secret
- Callback URL (use `http://localhost:5096/api/auth/callback`)

### 2. Unzip the package

Save the mcp executable and copy the location `C:\\path\\to\\apsMcp.StdioServer.exe`

### 3. Configure Claude Code

Add to `~/.config/claude-code/config.json` (Windows: `%APPDATA%\.claude-code\config.json`):

```json
{
  "mcpServers": {
    "autodesk-mcp": {
      "command": "C:\\path\\to\\apsMcp.StdioServer.exe",
      "env": {
        "APS_CLIENT_ID": "your_client_id",
        "APS_CLIENT_SECRET": "your_client_secret",
        "APS_CALLBACK_URL": "http://localhost:8080/"
      }
    }
  }
}
```

### 4. Install the APS skill

Copy the skill to Claude Code:

```bash
# Windows
cp -r .claude/skills/aps-skill ~/.claude/skills/

# macOS/Linux
cp -r .claude/skills/aps-skill ~/.claude/skills/
```

### 5. Restart Claude Code

The MCP server and skill are now ready to use.

## Usage

Invoke the use of the MCP using the skill for any APS-related task:

Clear trigger keywords:
- APS, Autodesk Platform Services
- ACC, Autodesk Construction Cloud
- BIM360
- Revit models/files in cloud context
- Element groups, AEC Data Model, AECDM
- Generic terms (hubs/projects/files/models/elements/viewer) WITH Autodesk/APS/ACC context

Now the behavior should be:
- ❌ "tell me my hub" - NOT invoked (no context)
- ✅ "tell me my hub in APS" - INVOKED (has APS)
- ✅ "tell me my hub in ACC" - INVOKED (has ACC)
- ✅ "tell me my Autodesk hub" - INVOKED (has Autodesk)
- ✅ "show my BIM360 projects" - INVOKED (has BIM360)

The skill automatically handles:

- GraphQL query composition with proper RSQL filters
- URN format validation
- 3-step filter-to-highlight workflows
- Data exchange creation with base64 ID generation

## What you can do

- **Browse ACC data**: List hubs, projects, models, and element groups
- **Query elements**: Count by category, filter by properties, retrieve file information
- **View in 3D**: Load models and highlight filtered elements in browser
- **Create exchanges**: Export filtered element sets to target folders
- **Authentication**: OAuth handled automatically when needed