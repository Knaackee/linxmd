# Linxmd

> Install reusable AI building blocks — Agents, Skills, Workflows, and Templates — into any project, with one CLI.

Linxmd is a lightweight package manager for AI workflow artifacts. Run `linxmd init` once, then `linxmd add` whatever you need. Every install is dependency-aware, version-pinned, and synced across Copilot, Claude Code, and OpenCode automatically. Templates can also be copied into your project with `linxmd new`.

## ✨ Features

- 📦 **Reusable artifacts and templates** — browse the current shipped catalog in [lib/README.md](lib/README.md)
- 🔗 **Dependency-safe installs** — blocked removals protect dependent workflows from breaking
- 🔄 **One-command sync** — `linxmd sync` regenerates all tool wrappers in one step
- 🧩 **File templates** — install template bundles and copy them into your project unchanged
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
linxmd new [template|template:name] [--force]
linxmd update [--yes]
linxmd sync
linxmd status
```

**Examples**

```bash
linxmd add workflow:sdd-tdd --yes           # full TDD pipeline
linxmd add agent:router --yes               # smart request router
linxmd add skill:context-management --yes   # context handling
linxmd add template:agent-core --yes        # install a file template
linxmd new template:agent-core              # copy template files into the project
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

Artifact structure and frontmatter specs:

- Shared frontmatter schema (quick actions + lifecycle hooks): [`lib/FRONTMATTER-SPEC.md`](lib/FRONTMATTER-SPEC.md)
- Agent artifact spec: [`lib/agents/SPEC.md`](lib/agents/SPEC.md)
- Skill artifact spec: [`lib/skills/SPEC.md`](lib/skills/SPEC.md)
- Template artifact spec: [`lib/templates/SPEC.md`](lib/templates/SPEC.md)
- Workflow artifact spec: [`lib/workflows/SPEC.md`](lib/workflows/SPEC.md)

### 🔄 Workflows

| Artifact | Description |
|---|---|
| `workflow:sdd-tdd` | Full Spec-Driven + TDD pipeline from spec to reviewed, documented code |
| `workflow:bug-fix` | Reproduce, fix, verify, and document a bug |
| `workflow:quality-baseline` | Audit and raise project quality — coverage, static analysis, security scan |
| `workflow:release` | Cut a clean, documented release in one pipeline |
| `workflow:content-review` | Draft → fact-check → edit content pipeline |
| `workflow:artifact-factory` | Author new agents, skills, and workflows |

### 🤖 Agents

| Artifact | Description |
|---|---|
| `agent:router` | Routes requests to the right workflow or agent |
| `agent:planner` | Decomposes a spec into a sequenced, independently-committable task list |
| `agent:architect` | Records technical decisions as numbered ADRs |
| `agent:test-writer` | Writes failing tests from acceptance criteria |
| `agent:implementer` | Writes the minimum code to turn failing tests green |
| `agent:reviewer-spec` | Verifies all acceptance criteria before merge |
| `agent:reviewer-quality` | Audits code quality, security, and design smells |
| `agent:docs-writer` | Updates READMEs, changelogs, and API docs after code changes |
| `agent:changelog-writer` | Writes perfectly formatted changelog entries from task context or diffs |
| `agent:drafter` | Produces first-pass content drafts |
| `agent:editor` | Polishes content for clarity, structure, and style |
| `agent:fact-checker` | Verifies factual claims, numbers, and references in documents |

### 🛠️ Skills

| Artifact | Description |
|---|---|
| `skill:task-management` | Backlog, spec files, and task-tracking conventions |
| `skill:project-memory` | ADRs, changelog, and known-issues log — durable knowledge that survives context resets |
| `skill:debugging` | Systematic fault isolation with hypothesis tracking |
| `skill:refactoring` | Test-backed refactoring with rollback checkpoints |
| `skill:api-design` | OpenAPI 3.1 spec writing, versioning strategies, and security checklists |
| `skill:code-translator` | Port code between languages idiomatically, with test-suite verification |
| `skill:i18n` | Audit and extract hardcoded strings into locale files |
| `skill:text-translator` | Translate content between human languages preserving tone and brand voice |
| `skill:design-tokens` | Extract CSS magic values into a W3C token system with Tailwind + CSS output |
| `skill:context-management` | Coherent-context strategies for long sessions |
| `skill:observability` | Structured logging and traces for agentic pipelines |
| `skill:preview-delivery` | Build previews, share links, collect feedback |

### 📦 Packs

| Artifact | Description |
|---|---|
| `pack:fullstack-tdd` | Complete TDD pipeline — sdd-tdd + router + context management |
| `pack:content-pipeline` | Full content stack — content-review workflow + drafter + editor + fact-checker |
| `pack:quality-sprint` | Quality in one shot — baseline audit + project memory + automated routing |
| `pack:i18n-ready` | Multilingual-ready — i18n extraction + text-translator + task management |

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
│   ├── templates/
│   ├── tasks/
│   │   ├── backlog/
│   │   └── in-progress/
│   ├── installed.json
│   └── sources.json
├── docs/
│   └── decisions/
├── CHANGELOG.md
├── KNOWN_ISSUES.md
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