# Linxmd

> Build and run AI workflow building blocks with one CLI.

Linxmd installs reusable Agents, Skills, and Workflows into your project, then syncs wrappers for GitHub Copilot, Claude Code, and OpenCode.

## Why Linxmd

- One unified command model (`add`, `remove`, `list`, `update`, `sync`, `status`, `init`)
- Typed artifact IDs to avoid ambiguity (`agent:test-writer`, `skill:feature`, `workflow:sdd-tdd`)
- Dependency-safe uninstall (blocks removals that would break installed workflows)
- Source-aware installs and updates via `.linxmd/sources.json`
- Cross-tool wrapper generation in a single sync step

## 60-Second Quickstart

### Install

Windows (PowerShell):

```powershell
Invoke-WebRequest -Uri https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-win-x64.exe -OutFile linxmd.exe; Move-Item linxmd.exe "$env:LOCALAPPDATA\Microsoft\WindowsApps\linxmd.exe" -Force
```

Linux:

```bash
curl -Lo linxmd https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-linux-x64 && chmod +x linxmd && sudo mv linxmd /usr/local/bin/
```

macOS:

```bash
curl -Lo linxmd https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-osx-arm64 && chmod +x linxmd && sudo mv linxmd /usr/local/bin/
```

### Initialize and install a workflow

```bash
linxmd init
linxmd add workflow:sdd-tdd --yes
linxmd status
```

## Command Model

```bash
linxmd init
linxmd add [query-or-type:name] [--yes] [--source <id>]
linxmd remove [name-or-type:name] [--yes]
linxmd list [type|type:name] [--json]
linxmd update [--yes]
linxmd sync
linxmd status
```

Examples:

```bash
linxmd add agent:test-writer --yes
linxmd add workflow:sdd-tdd --yes
linxmd remove skill:feature --yes
linxmd list workflow
linxmd list skill:task-management
linxmd list --json
```

## Multi-Source Registry

`linxmd init` creates `.linxmd/sources.json` with a default source:

```json
{
  "sources": [
    {
      "id": "default",
      "kind": "github",
      "owner": "Knaackee",
      "repo": "linxmd",
      "branch": "main",
      "basePath": "lib"
    }
  ]
}
```

Install from a source:

```bash
linxmd add agent:echo-test --source default --yes
```

`linxmd update` checks each installed artifact against its original source metadata.

## Dependency Safety

By default, uninstall is safe:

- If `workflow:sdd-tdd` depends on `skill:task-management`, removing `skill:task-management` is blocked.
- Remove dependents first, then dependencies.

## Project Structure

```text
your-project/
+-- .linxmd/
¦   +-- agents/
¦   +-- skills/
¦   +-- workflows/
¦   +-- tasks/
¦   ¦   +-- backlog/
¦   ¦   +-- in-progress/
¦   +-- installed.json
¦   +-- sources.json
+-- .github/agents/
+-- .claude/agents/
+-- .claude/skills/
+-- .opencode/agents/
```

## Included Library Artifacts

### Workflows

- `workflow:sdd-tdd`
- `workflow:content-review`
- `workflow:artifact-factory`
- `workflow:echo-test`

### Skills

- `skill:feature`
- `skill:task-management`
- `skill:debugging`
- `skill:refactoring`
- `skill:preview-delivery`
- `skill:echo-test`

### Agents

- `agent:test-writer`
- `agent:implementer`
- `agent:reviewer-spec`
- `agent:reviewer-quality`
- `agent:docs-writer`
- `agent:echo-test`

## VS Code Coverage Setup

Linxmd includes workspace settings and tasks for coverage visualization.

1. Install extension: `ryanluker.vscode-coverage-gutters`
2. Run task: `test:coverage`
3. In Coverage Gutters command palette, choose "Display Coverage"

If the gutter is not visible, run tests once and verify a `coverage.cobertura.xml` exists under `tests/Linxmd.Tests/TestResults`.

## Development

```bash
dotnet build
dotnet test
```

## License

MIT
