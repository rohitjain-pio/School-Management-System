# MCP Servers Installation Guide

## Prerequisites

- Node.js 18+ installed
- npm or yarn package manager
- VS Code or compatible editor with MCP support

---

## Step 1: Install Core Local Servers

Navigate to the MCP directory:

```powershell
cd D:\Projects\SMS\School-Management-System\mcp
```

Install the privacy-focused local servers:

```powershell
npm install
```

This will install:
- ✅ `@modelcontextprotocol/server-filesystem`
- ✅ `@modelcontextprotocol/server-git`
- ✅ `@modelcontextprotocol/server-memory`
- ✅ `@modelcontextprotocol/server-sequential-thinking`

---

## Step 2: Configure VS Code

### Option A: Project-Level Configuration

Create `.vscode/settings.json` in your project root:

```json
{
  "mcp.servers": {
    "filesystem": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-filesystem/dist/index.js"],
      "cwd": "${workspaceFolder}",
      "allowedDirectories": [
        "${workspaceFolder}/Backend",
        "${workspaceFolder}/Frontend",
        "${workspaceFolder}/mcp"
      ]
    },
    "git": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-git/dist/index.js"],
      "cwd": "${workspaceFolder}",
      "repository": "${workspaceFolder}"
    },
    "memory": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-memory/dist/index.js"],
      "cwd": "${workspaceFolder}",
      "storePath": "${workspaceFolder}/mcp/data/memory"
    },
    "sequential-thinking": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-sequential-thinking/dist/index.js"],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

### Option B: User-Level Configuration (Global)

Open VS Code Settings (Ctrl+Shift+P → "Preferences: Open User Settings (JSON)") and add the MCP configuration above, adjusting paths as needed.

---

## Step 3: Verify Installation

1. Restart VS Code
2. Open the project folder
3. Check that MCP servers are loaded (look for MCP indicators in VS Code)
4. Test functionality:
   - Try file operations (filesystem server)
   - Check git status (git server)
   - Test context persistence (memory server)

---

## Step 4: Optional External Servers (Install Only If Needed)

### GitHub Server (for GitHub API access)

```powershell
npm install @modelcontextprotocol/server-github
```

Add to VS Code settings:
```json
{
  "github": {
    "command": "node",
    "args": ["./mcp/node_modules/@modelcontextprotocol/server-github/dist/index.js"],
    "env": {
      "GITHUB_TOKEN": "your_github_personal_access_token"
    }
  }
}
```

⚠️ **Security**: Store token in environment variable, not in committed files.

### Fetch Server (for external URL fetching)

```powershell
npm install @modelcontextprotocol/server-fetch
```

Add to VS Code settings:
```json
{
  "fetch": {
    "command": "node",
    "args": ["./mcp/node_modules/@modelcontextprotocol/server-fetch/dist/index.js"],
    "allowedDomains": [
      "docs.microsoft.com",
      "react.dev",
      "typescriptlang.org"
    ]
  }
}
```

### Brave Search Server (for web searches)

```powershell
npm install @modelcontextprotocol/server-brave-search
```

Get API key from: https://brave.com/search/api/

Add to VS Code settings:
```json
{
  "brave-search": {
    "command": "node",
    "args": ["./mcp/node_modules/@modelcontextprotocol/server-brave-search/dist/index.js"],
    "env": {
      "BRAVE_API_KEY": "your_brave_api_key"
    }
  }
}
```

---

## Troubleshooting

### MCP Servers Not Loading

1. Check Node.js is installed: `node --version`
2. Verify npm packages installed: `ls node_modules/@modelcontextprotocol`
3. Check VS Code MCP extension is enabled
4. Review VS Code output logs for errors

### Permission Errors

- Ensure VS Code has read/write access to project directories
- Check `allowedDirectories` configuration is correct
- Verify no antivirus blocking Node.js execution

### Path Issues on Windows

- Use forward slashes `/` or double backslashes `\\\\` in JSON
- Use `${workspaceFolder}` variable for relative paths
- Check `cwd` parameter points to valid directory

---

## Uninstallation

To remove MCP servers:

```powershell
# Remove all packages
cd mcp
rm -rf node_modules

# Remove VS Code configuration
# Delete .vscode/settings.json MCP section

# Optional: Remove entire MCP directory
cd ..
rm -rf mcp
```

---

## Support

- MCP Documentation: https://modelcontextprotocol.io
- Report issues: Create issue in project repository
- Check logs: VS Code → Output → Select "MCP" channel
