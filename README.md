# agentsmd

> AI Agent Workflow Manager — eine CLI für Agents, Skills und Workflows.

Self-contained Exe für Windows, Linux und macOS. Kein Runtime nötig.

## Quick Start

**Windows (PowerShell):**
```powershell
# Download
Invoke-WebRequest -Uri https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-win-x64.exe -OutFile agentsmd.exe

# In PATH ablegen (einmalig)
Move-Item agentsmd.exe "$env:LOCALAPPDATA\Microsoft\WindowsApps\agentsmd.exe"
```

**Linux:**
```bash
curl -Lo agentsmd https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-linux-x64
chmod +x agentsmd
sudo mv agentsmd /usr/local/bin/
```

**macOS:**
```bash
curl -Lo agentsmd https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-osx-arm64
chmod +x agentsmd
sudo mv agentsmd /usr/local/bin/
```

**Dann in deinem Projekt:**
```bash
agentsmd init
agentsmd workflow install sdd-tdd
agentsmd sync
```

## Was ist agentsmd?

Ein Paketmanager für AI-Agent-Workflows. Installiere vorgefertigte Agents, Skills und Workflows aus einer zentralen Lib — und synchronisiere sie automatisch für GitHub Copilot, Claude Code und OpenCode.

```
Lib (dieses Repo)              Dein Projekt
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
agentsmd init                        # Projekt initialisieren
agentsmd search [query]              # Lib durchsuchen
agentsmd list                        # Installierte Artefakte
agentsmd sync                        # Tool-Wrappers generieren
agentsmd status                      # Projekt-Überblick

# Agents
agentsmd agent install <name>        # Agent installieren
agentsmd agent uninstall <name>      # Agent entfernen
agentsmd agent list                  # Installierte Agents
agentsmd agent search [query]        # Agents in Lib suchen
agentsmd agent info <name>           # Details anzeigen

# Skills (gleiche Verben)
agentsmd skill install <name>
agentsmd skill uninstall <name>
agentsmd skill list
agentsmd skill search [query]
agentsmd skill info <name>

# Workflows (gleiche Verben)
agentsmd workflow install <name>     # + automatische Dependency Resolution
agentsmd workflow uninstall <name>
agentsmd workflow list
agentsmd workflow search [query]
agentsmd workflow info <name>
```

## Lib-Inhalte

### Agents

| Agent | Beschreibung |
|-------|-------------|
| `test-writer` | Schreibt Tests aus Spezifikationen (RED Phase) |
| `implementer` | Minimaler Code bis Tests grün (GREEN Phase) |
| `reviewer-spec` | Prüft ob alle Akzeptanzkriterien erfüllt sind |
| `reviewer-quality` | Code-Qualität und Security Review |
| `docs-writer` | Dokumentation aktualisieren |

### Skills

| Skill | Beschreibung |
|-------|-------------|
| `task-management` | Backlog, Specs, Task-Tracking |
| `feature` | Feature-Entwicklung mit SDD+TDD Workflow |
| `debugging` | Systematisches Debugging mit Hypothesen-Tracking |
| `refactoring` | Sicheres Refactoring mit Test-Absicherung |

### Workflows

| Workflow | Beschreibung |
|----------|-------------|
| `sdd-tdd` | Spec-Driven Development mit TDD Pipeline |
| `content-review` | Content-Erstellung mit Review-Pipeline |

## Wie funktioniert `agentsmd sync`?

```
.agentsmd/agents/test-writer.md
  ├──► .github/agents/test-writer.agent.md   (+ Copilot Frontmatter)
  ├──► .opencode/agents/test-writer.md        (+ OpenCode Frontmatter)
  └──► .claude/agents/test-writer.md          (+ Claude Code Frontmatter)

.agentsmd/skills/feature/
  └──► .claude/skills/feature/                (Kopie — ganzer Ordner)
```

Agents werden als Tool-Wrappers für alle drei AI-Tools generiert.
Skills werden nach `.claude/skills/` kopiert (alle drei Tools lesen diesen Pfad).

## Projektstruktur nach `agentsmd init`

```
dein-projekt/
├── .agentsmd/
│   ├── agents/              # Installierte Agents
│   ├── skills/              # Installierte Skills (Ordner)
│   ├── workflows/           # Installierte Workflows
│   ├── tasks/
│   │   ├── backlog/         # Feature-Ideen
│   │   └── in-progress/     # Aktive Features mit SPEC.md + TASKS.md
│   └── installed.json       # Installationsstate
├── .github/agents/          # Copilot Wrappers (generiert)
├── .claude/agents/          # Claude Code Wrappers (generiert)
├── .claude/skills/          # Skills für alle Tools (generiert)
└── .opencode/agents/        # OpenCode Wrappers (generiert)
```

## Download

Aktuelle Version: **v0.1.0**

| Plattform | Download |
|-----------|----------|
| Windows | [agentsmd-win-x64.exe](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-win-x64.exe) |
| Linux | [agentsmd-linux-x64](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-linux-x64) |
| macOS | [agentsmd-osx-arm64](https://github.com/Knaackee/agentsmd/releases/latest/download/agentsmd-osx-arm64) |

## Entwicklung

```bash
dotnet build
dotnet test          # 51 Tests (Unit + E2E)
```

## Lizenz

MIT
