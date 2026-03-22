# agentsmd

> AI Agent Workflow Manager — a CLI for Agents, Skills, and Workflows.

Self-contained executable for Windows, Linux, and macOS. No runtime required.

## Quick Start

**Windows (PowerShell):**
```powershell
Invoke-WebRequest -Uri https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-win-x64.exe -OutFile agentsmd.exe; Move-Item agentsmd.exe "$env:LOCALAPPDATA\Microsoft\WindowsApps\agentsmd.exe" -Force
```

**Linux:**
```bash
curl -Lo agentsmd https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-linux-x64 && chmod +x agentsmd && sudo mv agentsmd /usr/local/bin/
```

**macOS:**
```bash
curl -Lo agentsmd https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-osx-arm64 && chmod +x agentsmd && sudo mv agentsmd /usr/local/bin/
```

**Then in your project:**
```bash
agentsmd init
agentsmd workflow install sdd-tdd
agentsmd sync
```

## What is agentsmd?

A package manager for AI agent workflows. Install pre-built Agents, Skills, and Workflows from a central lib — and sync them automatically for GitHub Copilot, Claude Code, and OpenCode.

```
Lib (this repo)                Your Project
┌────────────────┐             ┌──────────────────┐
│ lib/           │   install   │ .agentsmd/       │
│   agents/      │ ──────────► │   agents/        │
│   skills/      │             │   skills/        │
│   workflows/   │             │   workflows/     │
└────────────────┘             │   tasks/         │
                               │   installed.json │
                               └────────┬─────────┘
                                  sync  │
                                        ▼
                               ┌──────────────────┐
                               │ .github/agents/  │ Copilot
                               │ .claude/agents/  │ Claude Code
                               │ .claude/skills/  │ Claude Code
                               │ .opencode/agents/│ OpenCode
                               └──────────────────┘
```

## CLI Commands

```bash
# Global
agentsmd init                        # Initialize project
agentsmd search [query]              # Search the lib
agentsmd list                        # List installed artifacts
agentsmd sync                        # Generate tool wrappers
agentsmd status                      # Project overview

# Agents
agentsmd agent install <name>        # Install an agent
agentsmd agent uninstall <name>      # Remove an agent
agentsmd agent list                  # List installed agents
agentsmd agent search [query]        # Search agents in lib
agentsmd agent info <name>           # Show agent details

# Skills (same verbs)
agentsmd skill install <name>
agentsmd skill uninstall <name>
agentsmd skill list
agentsmd skill search [query]
agentsmd skill info <name>

# Workflows (same verbs)
agentsmd workflow install <name>     # + automatic dependency resolution
agentsmd workflow uninstall <name>
agentsmd workflow list
agentsmd workflow search [query]
agentsmd workflow info <name>
```

## Lib Contents

### Agents

| Agent | Description |
|-------|-------------|
| `test-writer` | Writes tests from specifications (RED phase) |
| `implementer` | Minimal code until tests pass (GREEN phase) |
| `reviewer-spec` | Verifies all acceptance criteria are met |
| `reviewer-quality` | Code quality and security review |
| `docs-writer` | Updates documentation after reviews pass |

### Skills

| Skill | Description |
|-------|-------------|
| `task-management` | Backlog, specs, and task tracking |
| `feature` | Feature development with SDD+TDD workflow |
| `debugging` | Systematic debugging with hypothesis tracking |
| `refactoring` | Safe refactoring with test coverage |

### Workflows

| Workflow | Description |
|----------|-------------|
| `sdd-tdd` | Spec-Driven Development with TDD pipeline |
| `content-review` | Content creation with review pipeline |

## How `agentsmd sync` works

```
.agentsmd/agents/test-writer.md
  ├──► .github/agents/test-writer.agent.md   (+ Copilot front matter)
  ├──► .opencode/agents/test-writer.md        (+ OpenCode front matter)
  └──► .claude/agents/test-writer.md          (+ Claude Code front matter)

.agentsmd/skills/feature/
  └──► .claude/skills/feature/                (copy — entire folder)
```

Agents are generated as tool wrappers for all three AI tools.
Skills are copied to `.claude/skills/` (all three tools read this path).

## Project structure after `agentsmd init`

```
your-project/
├── .agentsmd/
│   ├── agents/              # Installed agents
│   ├── skills/              # Installed skills (folders)
│   ├── workflows/           # Installed workflows
│   ├── tasks/
│   │   ├── backlog/         # Feature ideas
│   │   └── in-progress/     # Active features with SPEC.md + TASKS.md
│   └── installed.json       # Installation state
├── .github/agents/          # Copilot wrappers (generated)
├── .claude/agents/          # Claude Code wrappers (generated)
├── .claude/skills/          # Skills for all tools (generated)
└── .opencode/agents/        # OpenCode wrappers (generated)
```

## Download

Current version: **v0.1.0**

| Platform | Download |
|----------|----------|
| Windows | [agentsmd-win-x64.exe](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-win-x64.exe) |
| Linux | [agentsmd-linux-x64](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-linux-x64) |
| macOS | [agentsmd-osx-arm64](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-osx-arm64) |

## Development

```bash
dotnet build
dotnet test
```

## License

MIT

