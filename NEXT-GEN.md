# agentsmd CLI — Next-Gen UX Plan

## Goal

Transform the current plain `Console.WriteLine` CLI into a **modern, interactive, visually rich** terminal experience — inspired by tools like **Charm (BubbleTea/Glow)**, **Spectre.Console**, and principles from **clig.dev**.

## Inspiration & References

| Tool | What to steal | Language |
|---|---|---|
| **Spectre.Console** (.NET, 11k★) | Tables, Trees, Progress bars, Spinners, Markup, Live rendering, Prompts | C# ✅ direct fit |
| **Charm BubbleTea** (Go, 41k★) | Elm-architecture TUI, spinners, smooth animations | Go (design patterns only) |
| **Charm Glow** (Go, 24k★) | Beautiful markdown rendering in terminal | Go (concept) |
| **Charm Huh?** (Go) | Interactive prompts & forms | Go (concept) |
| **clig.dev** | CLI design philosophy: human-first, conversational, discoverable | Guidelines |
| **Colorful.Console** (.NET) | Gradients, FIGlet ASCII art | C# (lighter option) |
| **Terminal.Gui** (.NET, 11k★) | Full TUI framework (overkill for us, but good for TUI modes) | C# |

**Winner: Spectre.Console** — native .NET, actively maintained, .NET Foundation, 11k★, covers 95% of our needs.

---

## Phase 1: Foundation — Spectre.Console Integration

### 1.1 Add Dependency
```xml
<PackageReference Include="Spectre.Console" Version="0.50.*" />
```

### 1.2 Branded Header
ASCII art logo on `--help` and `init`, using Spectre's `FigletText`:
```
   ___                    __                       __
  / _ |___ _ ___ _ ___  / /_ ___  __ _  ___  ____/ /
 / __ / _ `/ -_) ' \  / __/(_-< /  ' \/ _ \/___/ _ \
/_/ |_\_, /\__/_||_/  \__//___/ /_/_/_/\___/   /_//_/
     /___/
        AI Agent Workflow Manager
```

### 1.3 Color System
Define a consistent palette:
```
Primary:    DodgerBlue1  — commands, headers
Success:    Green3       — ✓ installed, completed
Warning:    Orange1      — ⚠ deprecation, missing
Error:      Red          — ✗ failures
Muted:      Grey         — versions, paths
Accent:     MediumPurple — highlights, links
```

---

## Phase 2: Rich Output — Replace Console.WriteLine

### 2.1 Tables everywhere
Replace plain text lists with Spectre Tables:

**Before:**
```
  agent      implementer               0.1.0      Implements features based on spec
  agent      test-writer               0.1.0      Writes tests following TDD
```

**After:**
```
┌──────────┬───────────────┬─────────┬──────────────────────────────────────┐
│ Type     │ Name          │ Version │ Description                          │
├──────────┼───────────────┼─────────┼──────────────────────────────────────┤
│ 🤖 agent │ implementer   │  0.1.0  │ Implements features based on spec    │
│ 🤖 agent │ test-writer   │  0.1.0  │ Writes tests following TDD           │
│ 📦 skill │ feature       │  0.1.0  │ Building features end-to-end         │
│ 🔄 wflow │ sdd-tdd       │  0.1.0  │ Spec-driven TDD workflow             │
└──────────┴───────────────┴─────────┴──────────────────────────────────────┘
```

### 2.2 `info` command — Rich Panel
```
╭─ 🔄 Workflow: sdd-tdd ──────────────────────────────╮
│                                                       │
│  Version:  0.1.0                                      │
│  Tags:     tdd, spec-driven, quality                  │
│                                                       │
│  Description:                                         │
│  Spec-driven TDD workflow with review gates.          │
│  Ensures every feature goes through spec → test →     │
│  implement → review cycle.                            │
│                                                       │
│  Dependencies:                                        │
│    ├── 🤖 implementer                                 │
│    ├── 🤖 test-writer                                 │
│    ├── 🤖 reviewer-spec                               │
│    ├── 🤖 reviewer-quality                            │
│    ├── 🤖 docs-writer                                 │
│    ├── 📦 feature                                     │
│    └── 📦 task-management                             │
│                                                       │
│  Installed: ✓ Yes                                     │
╰───────────────────────────────────────────────────────╯
```

### 2.3 `status` command — Dashboard Style
```
╭─ agentsmd status ─────────────────────────────────────╮
│                                                        │
│  📂 Project: D:\Development\my-project                 │
│                                                        │
│  Installed:                                            │
│    🤖 Agents:     5                                    │
│    📦 Skills:     2                                    │
│    🔄 Workflows:  1                                    │
│                                                        │
│  📋 Backlog:        3 items                            │
│  🔨 In Progress:    1 item                             │
│                                                        │
│  🔧 Tools: copilot, claude-code, opencode             │
╰────────────────────────────────────────────────────────╯
```

---

## Phase 3: Interactive & Animated

### 3.1 Spinners for Network Operations
Every HTTP fetch gets an animated spinner:
```
⣾ Fetching lib index...         →  ✓ Fetched 12 artifacts
⣾ Downloading implementer.md... →  ✓ Downloaded
⣾ Installing dependencies...    →  ✓ 5 agents, 2 skills installed
```
Use `AnsiConsole.Status()` for single operations, `AnsiConsole.Progress()` for multi-step.

### 3.2 Progress Bars for Bulk Operations
When installing a workflow with many deps:
```
Installing workflow 'sdd-tdd'...
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 7/7
  ✓ implementer       0.1.0
  ✓ test-writer       0.1.0
  ✓ reviewer-spec     0.1.0
  ✓ reviewer-quality  0.1.0
  ✓ docs-writer       0.1.0
  ✓ feature           0.1.0
  ✓ task-management   0.1.0
```

### 3.3 Interactive `init` Wizard
Replace static init with an interactive flow using Spectre Prompts:
```
Welcome to agentsmd! 🚀

? Select a workflow to install:
  ❯ sdd-tdd          — Spec-driven TDD with review gates
    content-review    — Content creation & review pipeline
    echo-test         — Echo test for development
    (skip)

? Enable tools:
  ✓ GitHub Copilot   (.github/agents/)
  ✓ Claude Code      (.claude/agents/)
  ✓ OpenCode         (.opencode/agents/)

⣾ Setting up...

✓ Initialized .agentsmd/
✓ Installed workflow 'sdd-tdd' (5 agents, 2 skills)
✓ Generated 15 tool wrappers

Next steps:
  1. Open your AI agent (Copilot/Claude/OpenCode)
  2. Start with: "Begin the sdd-tdd workflow for [your feature]"
```

### 3.4 Interactive `search` with Filtering
```
? Search artifacts: feature█
  ┌──────────┬─────────────────┬─────────────────────────────────┐
  │ Type     │ Name            │ Description                     │
  ├──────────┼─────────────────┼─────────────────────────────────┤
  │ 📦 skill │ feature         │ Building features end-to-end    │
  │ 🤖 agent │ implementer     │ Implements features from spec   │
  └──────────┴─────────────────┴─────────────────────────────────┘

  [Enter] Install  [i] Info  [Esc] Cancel
```

---

## Phase 4: Better Error UX

### 4.1 Friendly Errors with Suggestions
**Before:**
```
Not initialized. Run 'agentsmd init' first.
```

**After:**
```
╭─ ⚠ Not Initialized ──────────────────────────────────╮
│                                                        │
│  This project hasn't been set up with agentsmd yet.    │
│                                                        │
│  Run this to get started:                              │
│                                                        │
│    agentsmd init                                       │
│                                                        │
╰────────────────────────────────────────────────────────╯
```

### 4.2 Did-you-mean Suggestions
```
$ agentsmd agent install implemnter

  ✗ Agent 'implemnter' not found.
  Did you mean: implementer?

  agentsmd agent install implementer
```

---

## Phase 5: Sync Visualization

### 5.1 Tree View for Generated Files
```
Synced 15 wrappers, 2 skills:

📁 .github/agents/
├── implementer.agent.md
├── test-writer.agent.md
├── reviewer-spec.agent.md
├── reviewer-quality.agent.md
└── docs-writer.agent.md
📁 .claude/agents/
├── implementer.md
├── test-writer.md
├── reviewer-spec.md
├── reviewer-quality.md
└── docs-writer.md
📁 .claude/skills/
├── feature/
│   └── SKILL.md
└── task-management/
    └── SKILL.md
📁 .opencode/agents/
├── implementer.md
├── ...
└── docs-writer.md
```

Use Spectre's `Tree` widget for this.

---

## Phase 6: Polish

### 6.1 `--json` Flag (Scriptability)
All commands support `--json` for CI/CD pipelines:
```json
{
  "agents": 5,
  "skills": 2,
  "workflows": 1,
  "tools": ["copilot", "claude-code", "opencode"]
}
```

### 6.2 `--plain` / `--no-color` Flags
Respect `NO_COLOR` env var (per clig.dev). Auto-detect non-TTY and disable rich output.

### 6.3 Version & Update Notification
```
agentsmd v0.2.0

  💡 Update available: v0.3.0
     Run: agentsmd update
```

---

## Implementation Order

| Step | Scope | Effort |
|------|-------|--------|
| 1 | Add `Spectre.Console` NuGet package | S |
| 2 | Create `ConsoleTheme` helper (colors, emoji, table defaults) | S |
| 3 | Replace `search` output with Spectre Table | S |
| 4 | Replace `list` output with Spectre Table | S |
| 5 | Replace `status` with Panel + dashboard layout | M |
| 6 | Add spinners to all `async` network commands | M |
| 7 | Refactor `init` to interactive wizard with Prompts | M |
| 8 | Add progress bar to workflow install (multi-dep) | M |
| 9 | Replace `info` with rich Panel + Tree (deps) | M |
| 10 | Add `sync` tree visualization | S |
| 11 | Add `--json` flag to all commands | M |
| 12 | Add `--plain` / `NO_COLOR` support | S |
| 13 | Add FIGlet logo to `--help` and `init` | S |
| 14 | Add did-you-mean fuzzy matching on not-found | M |
| 15 | Add update notification | M |

**Total: ~15 steps, progressive enhancement — each step ships independently.**

---

## Technical Notes

### Spectre.Console Compatibility
- Works with `PublishTrimmed` and `PublishSingleFile` (just needs trimmer roots)
- Cross-platform: Windows Terminal, iTerm2, GNOME Terminal, etc.
- Auto-detects terminal capabilities (color depth, Unicode support)
- Falls back gracefully on dumb terminals

### Key Spectre.Console APIs
```csharp
// Tables
var table = new Table();
table.AddColumn("Name");
table.AddRow("[blue]implementer[/]");
AnsiConsole.Write(table);

// Spinners
await AnsiConsole.Status().StartAsync("Fetching...", async ctx => {
    ctx.Spinner(Spinner.Known.Dots);
    await FetchData();
});

// Progress
await AnsiConsole.Progress().StartAsync(async ctx => {
    var task = ctx.AddTask("Installing deps");
    task.Increment(14.3); // per dep
});

// Prompts
var workflow = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select a workflow:")
        .AddChoices("sdd-tdd", "content-review", "skip"));

// Panels
AnsiConsole.Write(new Panel("[bold]Status[/]").Header("agentsmd"));

// Trees
var tree = new Tree("📁 .github/agents/");
tree.AddNode("implementer.agent.md");
AnsiConsole.Write(tree);

// FIGlet
AnsiConsole.Write(new FigletText("agentsmd").Color(Color.DodgerBlue1));
```

### Migration Strategy
1. Add `Spectre.Console` alongside existing `System.CommandLine`
2. Replace output incrementally (one command at a time)
3. Keep `System.CommandLine` for parsing — Spectre for rendering
4. Add `IAnsiConsole` abstraction for testability (Spectre supports `TestConsole`)

---

## Design Principles (from clig.dev)

1. **Human-first** — Rich output by default, `--json`/`--plain` for scripts
2. **Conversational** — Suggest next steps, did-you-mean, interactive prompts  
3. **Responsive** — Show spinners immediately, never hang silently
4. **Discoverable** — Lead with examples, suggest commands in context
5. **Robust** — Graceful fallback on dumb terminals, respect `NO_COLOR`
6. **Empathetic** — Friendly errors, clear guidance, celebrate success (✓)
