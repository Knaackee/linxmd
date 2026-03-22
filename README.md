# Linxmd

> Build and run AI workflow building blocks with one CLI.

Linxmd installs reusable Agents, Skills, and Workflows into your project, then syncs wrappers for GitHub Copilot, Claude Code, and OpenCode.

## Why Linxmd

- One unified command model (`add`, `remove`, `list`, `update`, `sync`, `status`, `init`, `init-prompt`)
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

### Initialize and complete onboarding

```bash
linxmd init
linxmd init-prompt
```

`linxmd init` creates `.linxmd/`, writes a bootstrap skill, runs sync, and prints a base onboarding prompt.

Use `linxmd init-prompt --copy` (or `linxmd init --copy-prompt`) to copy the onboarding prompt to your clipboard.

## Command Model

```bash
linxmd init
linxmd init-prompt [--copy]
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
linxmd add tdd --install --yes
linxmd add workflow:sdd-tdd --yes
linxmd remove skill:feature --yes
linxmd list workflow
linxmd list skill:task-management
linxmd list --json
linxmd init-prompt --copy
```

## Onboarding Model

Linxmd uses a 2-step onboarding model:

1. `linxmd init` sets up local structure and bootstrap context.
2. You run the printed prompt in your LLM tool to assess the project and propose/install workflows.

The onboarding prompt should not call `linxmd init` again.

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
�   +-- agents/
�   +-- skills/
�   �   +-- linxmd-self-bootstrap/
�   �       +-- SKILL.md
�   +-- workflows/
�   +-- tasks/
�   �   +-- backlog/
�   �   +-- in-progress/
�   +-- installed.json
�   +-- sources.json
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
