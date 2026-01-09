# MCP Servers Configuration for School Management System

## 🔒 Privacy & Security Overview

This project uses Model Context Protocol (MCP) servers to enhance development capabilities. Below is the configuration focusing on **privacy and local-only operations**.

---

## 📦 Installed MCP Servers

### **Core Local Servers (No External Data)**

#### 1. **@modelcontextprotocol/server-filesystem**
- **Purpose**: Read, write, and manage local project files
- **Privacy**: ✅ 100% Local
- **Data Location**: Your local file system only
- **Use Cases**:
  - Read/edit Backend C# files
  - Manage Frontend React/TypeScript components
  - Access configuration files

#### 2. **@modelcontextprotocol/server-git**
- **Purpose**: Git version control operations
- **Privacy**: ✅ 100% Local (unless you explicitly push/pull)
- **Data Location**: Local .git repository
- **Use Cases**:
  - View git history and diffs
  - Commit changes
  - Branch management
  - Check repository status

#### 3. **@modelcontextprotocol/server-memory**
- **Purpose**: Maintain context across conversations
- **Privacy**: ✅ 100% Local
- **Data Location**: Local memory/filesystem
- **Use Cases**:
  - Remember project conventions
  - Store session-specific context
  - Track ongoing work

#### 4. **@modelcontextprotocol/server-sequential-thinking**
- **Purpose**: Complex problem-solving and reasoning
- **Privacy**: ✅ 100% Local
- **Data Location**: No data storage
- **Use Cases**:
  - Architectural decisions
  - Debug complex issues
  - Plan multi-step implementations

---

## ⚠️ Optional External Servers (Not Installed by Default)

These servers require external API calls and are **NOT** included in the default installation. Install them only if needed and you're comfortable with data transmission:

### **@modelcontextprotocol/server-fetch**
- ⚠️ **Fetches content from external URLs**
- **Data Sent**: URLs you request
- **Alternative**: Download docs locally and use `server-filesystem`

### **@modelcontextprotocol/server-brave-search**
- ⚠️ **Sends search queries to Brave Search API**
- **Data Sent**: Your search queries
- **Requires**: Brave API key
- **Alternative**: Use local search or offline documentation

### **@modelcontextprotocol/server-github**
- ⚠️ **Communicates with GitHub API**
- **Data Sent**: Repository data, issues, PRs
- **Requires**: GitHub personal access token
- **Alternative**: Use `server-git` for local operations only

### **@modelcontextprotocol/server-puppeteer**
- ⚠️ **Can navigate to external websites**
- **Data Sent**: Depends on sites visited
- **Alternative**: Local testing frameworks, manual testing

---

## 🚀 Installation

### Install Core Local Servers Only (Recommended)
```powershell
cd mcp
npm install
```

This installs only the 4 privacy-focused local servers.

### Install Optional External Servers (If Needed)
```powershell
npm install @modelcontextprotocol/server-fetch
npm install @modelcontextprotocol/server-github
npm install @modelcontextprotocol/server-brave-search
```

---

## ⚙️ Configuration

### VS Code Settings (Local-Only MCP Configuration)

Add this to your `.vscode/settings.json` or global VS Code settings:

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
      "cwd": "${workspaceFolder}"
    },
    "memory": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-memory/dist/index.js"],
      "cwd": "${workspaceFolder}"
    },
    "sequential-thinking": {
      "command": "node",
      "args": ["./mcp/node_modules/@modelcontextprotocol/server-sequential-thinking/dist/index.js"],
      "cwd": "${workspaceFolder}"
    }
  }
}
```

---

## 🛡️ Security Recommendations

1. **✅ Default Setup**: Use only the 4 core local servers for maximum privacy
2. **⚠️ External Servers**: Only install if absolutely necessary
3. **🔐 API Keys**: Never commit API keys to git (use `.env` files)
4. **🔍 Review**: Audit which servers have access to your data
5. **📝 .gitignore**: Add MCP configuration files containing secrets:
   ```
   mcp/.env
   mcp/config.json
   .vscode/mcp-settings.json
   ```

---

## 📊 Data Flow Summary

| Server | Data Transmission | Risk Level | Recommended |
|--------|------------------|------------|-------------|
| filesystem | None (Local only) | 🟢 None | ✅ Yes |
| git | None (Local only) | 🟢 None | ✅ Yes |
| memory | None (Local only) | 🟢 None | ✅ Yes |
| sequential-thinking | None (Local only) | 🟢 None | ✅ Yes |
| fetch | External URLs | 🟡 Medium | ⚠️ Optional |
| brave-search | Search queries | 🟡 Medium | ⚠️ Optional |
| github | Repo data | 🟡 Medium | ⚠️ Optional |
| puppeteer | Web browsing | 🔴 High | ❌ Not recommended |

---

## 🏫 School Management System Specific Use Cases

### Backend (.NET 9)
- **filesystem**: Edit controllers, services, models
- **git**: Track changes to API endpoints
- **memory**: Remember entity relationships, database schema
- **sequential-thinking**: Design complex queries, optimize performance

### Frontend (React + TypeScript)
- **filesystem**: Manage components, pages, contexts
- **git**: Track UI changes
- **memory**: Remember component patterns, state management
- **sequential-thinking**: Plan complex UI interactions

### Database (SQL Server)
- **filesystem**: Edit migration files, seed scripts
- **git**: Version control schema changes
- **memory**: Track database structure
- **sequential-thinking**: Design database relationships

### Real-time (SignalR)
- **filesystem**: Edit hubs, manage SignalR config
- **git**: Track real-time feature changes
- **memory**: Remember connection patterns
- **sequential-thinking**: Debug connection issues

---

## 🔄 Alternative Solutions for External Needs

If you need functionality from external servers:

### Documentation Search
- **Download offline docs**: .NET docs, React docs, TypeScript docs
- **Use filesystem server**: Access local documentation
- **Install Zeal (Windows)**: Offline documentation browser

### Code Search
- **Use VS Code search**: Built-in workspace search
- **Git grep**: `git grep "pattern"`
- **ripgrep**: Fast local code search

### Testing
- **xUnit**: For .NET testing (local)
- **Vitest**: For React testing (local)
- **Playwright**: Local browser automation

---

## 📞 Support

For issues or questions about MCP servers:
- MCP Documentation: https://modelcontextprotocol.io
- Project Issues: Create an issue in this repository

---

**Last Updated**: January 8, 2026
**Privacy Level**: High (Local-only by default)
**External Dependencies**: None (unless optional servers are installed)
