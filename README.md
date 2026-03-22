# Linxmd

> Install reusable AI building blocks — Agents, Skills, and Workflows — into any project, with one CLI.

Linxmd is a lightweight package manager for AI workflow artifacts. Run `linxmd init` once, then `linxmd add` whatever you need. Every install is dependency-aware, version-pinned, and synced across Copilot, Claude Code, and OpenCode automatically.

## ✨ Features

- 📦 **20 ready-to-use artifacts** — [workflows](lib/README.md#-workflows), [agents](lib/README.md#-agents), and [skills](lib/README.md#-skills) in the box
- 🔗 **Dependency-safe installs** — blocked removals protect dependent workflows from breaking
- 🔄 **One-command sync** — `linxmd sync` regenerates all tool wrappers in one step
- 🌐 **Multi-source registry** — pull from GitHub, local paths, or your own fork
- 🏷️ **Semantic versioning** — `@>=0.2.0` constraints keep everything compatible
- 🤖 **Cross-tool** — syncs to GitHub Copilot (`.github/agents/`), Claude Code (`.claude/`), and OpenCode (`.opencode/`)
- 🧭 **Smart onboarding** — `linxmd init` bootstraps your project and prints a ready-to-run LLM prompt

## 60-Second Quickstart

### Install

**Windows (PowerShell)**

```powershell
Invoke-WebRequest -Uri https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-win-x64.exe -OutFile linxmd.exe; Move-Item linxmd.exe "$env:LOCALAPPDATA\Microsoft\WindowsApps\linxmd.exe" -Force
```

**Linux**

```bash
curl -Lo linxmd https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-linux-x64 && chmod +x linxmd && sudo mv linxmd /usr/local/bin/
```

**macOS**

```bash
curl -Lo linxmd https://github.com/Knaackee/linxmd/releases/latest/download/linxmd-osx-arm64 && chmod +x linxmd && sudo mv linxmd /usr/local/bin/
```

### Initialize

```bash
linxmd init
linxmd init-prompt
```

`linxmd init` creates `.linxmd/`, writes a bootstrap skill, runs sync, and prints a base onboarding prompt.

Use `linxmd init-prompt --copy` to copy the onboarding prompt directly to your clipboard.

## Command Reference

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

**Examples**

```bash
linxmd add workflow:sdd-tdd --yes           # full TDD pipeline
linxmd add agent:router --yes               # smart request router
linxmd add skill:context-management --yes   # context handling
linxmd remove agent:docs-writer --yes       # blocked if a workflow needs it
linxmd list workflow
linxmd list --json
linxmd update --yes
linxmd init-prompt --copy
```

## Onboarding Model

Linxmd uses a 2-step onboarding:

1. `linxmd init` — sets up local structure and bootstrap context
2. Run the printed prompt in your LLM tool to assess the project and propose/install workflows

The onboarding prompt does not call `linxmd init` again.

## Library

The full artifact catalog with dependency graphs and usage notes lives in [`lib/`](lib/README.md). Quick overview:

### 🔄 Workflows

| Artifact | Description |
|---|---|
| `workflow:sdd-tdd` | Full Spec-Driven + TDD pipeline from spec to reviewed, documented code |
| `workflow:content-review` | Draft → fact-check → edit content pipeline |
| `workflow:bug-fix` | Reproduce, fix, verify, and document a bug |
| `workflow:artifact-factory` | Author new agents, skills, and workflows |

### 🤖 Agents

| Artifact | Description |
|---|---|
| `agent:router` | Routes requests to the right workflow or agent |
| `agent:planner` | Decomposes a spec into a structured task list |
| `agent:test-writer` | Writes failing tests from acceptance criteria |
| `agent:implementer` | Writes code until tests pass |
| `agent:reviewer-spec` | Verifies all acceptance criteria are met |
| `agent:reviewer-quality` | Audits code quality and security |
| `agent:docs-writer` | Updates docs after reviews pass |
| `agent:drafter` | Produces first-pass content drafts |
| `agent:editor` | Polishes content for clarity and style |
| `agent:fact-checker` | Verifies claims and references in documents |

### 🛠️ Skills

| Artifact | Description |
|---|---|
| `skill:task-management` | Backlog, spec files, and task-tracking conventions |
| `skill:context-management` | Coherent-context strategies for long sessions |
| `skill:debugging` | Systematic fault isolation with hypothesis tracking |
| `skill:refactoring` | Test-backed refactoring with rollback checkpoints |
| `skill:observability` | Structured logging and traces for agentic pipelines |
| `skill:preview-delivery` | Build previews, share links, collect feedback |

→ **[Browse the full catalog with deps and dependency graph](lib/README.md)**

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

Supported source kinds: `github`, `local`.

To use a local lib directory (useful during development or for private artifact repos):

```json
{
  "sources": [
    {
      "id": "local",
      "kind": "local",
      "localPath": "/path/to/your/lib"
    }
  ]
}
```

Install from a specific source:

```bash
linxmd add agent:echo-test --source default --yes
```

`linxmd update` checks each installed artifact against its source metadata.

## Dependency Safety

Uninstall is safe by default:

- `workflow:sdd-tdd` depends on `skill:task-management`
- Removing `skill:task-management` is blocked while the workflow is installed
- Remove dependents first, then dependencies

## Project Structure

```text
your-project/
├── .linxmd/
│   ├── agents/
│   ├── skills/
│   │   └── linxmd-self-bootstrap/
│   │       └── SKILL.md
│   ├── workflows/
│   ├── tasks/
│   │   ├── backlog/
│   │   └── in-progress/
│   ├── installed.json
│   └── sources.json
├── .github/agents/
├── .claude/agents/
├── .claude/skills/
└── .opencode/agents/
```

## Development

```bash
dotnet build
dotnet test
```

## License

MIT