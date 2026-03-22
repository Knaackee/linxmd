# agentsmd CLI — Optimization Plan v2

## Status

Phase 1 (Foundation) and Phase 2 (Rich Output) are **done** (v0.2.0).  
This document now focuses on **Phase 3: Interactive CLI + Command Simplification**.

---

## Problem: Too Many Commands

Currently **20 commands** (5 global + 3 types × 5 subcommands):

```
agentsmd init
agentsmd search [query]
agentsmd list
agentsmd sync
agentsmd status
agentsmd agent install|uninstall|list|search|info <name>
agentsmd skill install|uninstall|list|search|info <name>
agentsmd workflow install|uninstall|list|search|info <name>
```

**Issues:**
1. `search` and `agent search` / `skill search` / `workflow search` are almost identical
2. `list` and `agent list` / `skill list` / `workflow list` are almost identical
3. `info` is just `search` + detail view — could be one flow
4. Users must know the type before running a command
5. No way to act on results — search showed, now user must type another command

---

## Proposed: Simplified Command Structure

### New Commands (7 instead of 20)

```
agentsmd init                           # Interactive setup wizard
agentsmd add [query]                    # Browse → select → install (interactive)
agentsmd remove [query]                 # Browse installed → select → uninstall
agentsmd list [--type agent|skill|wf]   # Show installed (filterable)
agentsmd sync                           # Generate wrappers (unchanged)
agentsmd status                         # Dashboard (unchanged)
agentsmd update                         # Update installed artifacts to latest
```

### What Changes

| Old | New | Why |
|-----|-----|-----|
| `search` | `add` | Search is useless without action. `add` = search + install in one flow |
| `agent/skill/workflow search` | `add --type agent` | One command with optional filter |
| `agent/skill/workflow install <name>` | `add <name>` | Auto-detects type, or asks if ambiguous |
| `agent/skill/workflow uninstall <name>` | `remove <name>` | Auto-detects type |
| `agent/skill/workflow list` | `list --type agent` | One command with optional filter |
| `agent/skill/workflow info <name>` | Part of `add` flow | Info shown inline before install confirmation |
| `agent/skill/workflow` (5 subcmds each) | **Removed** | Replaced by unified `add`/`remove`/`list` |

### Non-Interactive Mode (CI/Scripts)

All commands keep working non-interactively with explicit args:

```bash
agentsmd add sdd-tdd --yes              # Direct install, no prompts
agentsmd remove test-writer --yes       # Direct uninstall
agentsmd list --type agent --json       # Machine-readable output
```

---

## Interactive Flows

### `agentsmd add` — Browse & Install

```
$ agentsmd add

? Search library: feature█

  Use ↑↓ to navigate, Space to select, Enter to install, Esc to cancel

  ┌───┬──────────┬──────────────────┬─────────┬─────────────────────────────────┐
  │   │ Type     │ Name             │ Version │ Description                     │
  ├───┼──────────┼──────────────────┼─────────┼─────────────────────────────────┤
  │   │ 📦 skill │ feature          │  1.0.0  │ Feature development with        │
  │   │          │                  │         │ SDD+TDD workflow                │
  │ ❯ │ 🤖 agent │ implementer      │  1.0.0  │ Implements minimal code until   │
  │   │          │                  │         │ tests pass                      │
  │   │ 🔄 wflow │ sdd-tdd          │  1.0.0  │ Spec-Driven Development with    │
  │   │          │                  │         │ TDD pipeline                    │
  └───┴──────────┴──────────────────┴─────────┴─────────────────────────────────┘

  ℹ implementer v1.0.0 — no dependencies
```

Pressing **Enter** on a selection:

```
? Install implementer? (agent, v1.0.0)  [Y/n] y

  ⣾ Installing implementer...
  ✓ Installed agent 'implementer' v1.0.0
  ✓ Synced: 3 wrapper(s)

? Install more?  [Y/n] n
```

Pressing **Enter** on a workflow with deps:

```
? Install sdd-tdd? This will also install 7 dependencies:
    🤖 test-writer, implementer, reviewer-spec, reviewer-quality, docs-writer
    📦 feature, task-management
  [Y/n] y

  Installing sdd-tdd...
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 8/8
  ✓ test-writer      1.0.0
  ✓ implementer      1.0.0
  ✓ reviewer-spec    1.0.0
  ✓ reviewer-quality 1.0.0
  ✓ docs-writer      1.0.0
  ✓ feature          1.0.0
  ✓ task-management  1.0.0
  ✓ sdd-tdd          1.0.0

  ✓ Synced: 15 wrapper(s), 2 skill(s) copied.
```

### `agentsmd add <name>` — Direct Install

If name is unambiguous:
```
$ agentsmd add sdd-tdd

  ✓ Found workflow 'sdd-tdd' v1.0.0
  7 dependencies will also be installed.

  ? Proceed? [Y/n] y
  ...
```

If name matches multiple types:
```
$ agentsmd add echo-test

  Multiple matches found:
  ❯ 🤖 agent    echo-test  0.0.1
    📦 skill    echo-test  0.0.1
    🔄 workflow echo-test  0.0.1

  Use ↑↓ to select, Enter to confirm
```

### `agentsmd remove` — Browse & Uninstall

```
$ agentsmd remove

  Select artifacts to remove (Space to toggle, Enter to confirm):

  ┌───┬──────────┬──────────────────┬─────────┐
  │   │ Type     │ Name             │ Version │
  ├───┼──────────┼──────────────────┼─────────┤
  │ ☐ │ 🤖 agent │ test-writer      │  1.0.0  │
  │ ☑ │ 🤖 agent │ implementer      │  1.0.0  │
  │ ☐ │ 📦 skill │ feature          │  1.0.0  │
  │ ☐ │ 🔄 wflow │ sdd-tdd          │  1.0.0  │
  └───┴──────────┴──────────────────┴─────────┘

  ? Remove 1 artifact? [Y/n] y

  ✓ Uninstalled agent 'implementer'
  ✓ Synced: 12 wrapper(s), 2 skill(s) copied.
```

### `agentsmd init` — Interactive Wizard

```
$ agentsmd init

                                 _                       _
   __ _  __ _  ___ _ __ | |_ ___ _ __ ___   __| |
  / _` |/ _` |/ _ \ '_ \| __/ __| '_ ` _ \ / _` |
 | (_| | (_| |  __/ | | | |_\__ \ | | | | | (_| |
  \__,_|\__, |\___|_| |_|\__|___/_| |_| |_|\__,_|
        |___/
  AI Agent Workflow Manager

  ✓ Created .agentsmd/ structure.

  ? Select a workflow to start with:
    ❯ sdd-tdd        — Spec-Driven Development with TDD pipeline
      content-review  — Content creation with review pipeline
      (skip)

  ? Enable tool integrations:
    [✓] GitHub Copilot
    [✓] Claude Code
    [✓] OpenCode

  ⣾ Installing sdd-tdd and 7 dependencies...
  ✓ Installed workflow 'sdd-tdd' (5 agents, 2 skills)
  ✓ Generated 15 tool wrappers

  🎉 Ready! Start your AI agent and say:
     "Begin the sdd-tdd workflow for [your feature]"
```

---

## Backward Compatibility Strategy

### Phase A: Add new commands alongside old ones

```
agentsmd add          ← NEW (interactive)
agentsmd remove       ← NEW (interactive)
agentsmd search       ← REMOVED (fully replaced by 'add')
agentsmd agent install <n>  ← KEPT (deprecated, still works)
```

Old `agent/skill/workflow` subcommands print a one-time hint:
```
💡 Tip: Use 'agentsmd add' for an interactive experience.
```

### Phase B: Remove old commands (v1.0.0)

Drop `agent/skill/workflow` subcommand trees entirely.

---

## Implementation Plan

### Step 1: `agentsmd add` (interactive browse + install)
- Fetch index with spinner
- Spectre `TextPrompt` for search query (live filtering optional, or simple input)
- Spectre `SelectionPrompt` to pick from filtered results
- Show info inline (deps, description)
- Confirm → install with progress bar
- Support `agentsmd add <name>` for direct install
- Support `--yes` / `-y` for non-interactive

### Step 2: `agentsmd remove` (interactive browse + uninstall)
- Spectre `MultiSelectionPrompt` to toggle artifacts
- Confirm → uninstall batch
- Support `agentsmd remove <name>` for direct uninstall
- Support `--yes` / `-y` for non-interactive

### Step 3: Interactive `init` wizard
- Spectre `SelectionPrompt` for workflow selection
- Spectre `MultiSelectionPrompt` for tool integrations
- Combined install + sync in one flow

### Step 4: Consolidate `list`
- Add `--type` / `-t` filter option
- Add `--json` for machine output

### Step 5: Deprecate old commands
- `agent/skill/workflow` subcommands print deprecation hint
- `search` command removed entirely (replaced by `add`)

### Step 6: Add `update` command
- Compare installed versions with lib index
- Show updatable artifacts
- Interactive selection for what to update

---

## Technical: Spectre.Console Prompts API

```csharp
// Selection (single pick with arrows)
var choice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select a workflow:")
        .PageSize(10)
        .AddChoices("sdd-tdd", "content-review", "(skip)"));

// Multi-selection (space to toggle)
var selected = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title("Select artifacts to remove:")
        .PageSize(10)
        .AddChoices(installedNames));

// Text input
var query = AnsiConsole.Prompt(
    new TextPrompt<string>("Search library:")
        .AllowEmpty());

// Confirm
var proceed = AnsiConsole.Confirm("Install sdd-tdd?");
```

**Key constraint:** `SelectionPrompt` / `MultiSelectionPrompt` require a TTY. When stdin is not interactive (piped), fall back to non-interactive mode. Spectre handles this — `AnsiConsole.Profile.Capabilities.Interactive` tells you.

---

## Command Cheat Sheet (Before → After)

```
BEFORE (20 commands)                    AFTER (7 commands)
────────────────────                    ──────────────────
agentsmd init                        →  agentsmd init (interactive wizard)
agentsmd search [q]                  →  REMOVED (use 'agentsmd add [q]')
agentsmd agent search [q]            →  agentsmd add [q] --type agent
agentsmd skill search [q]            →  agentsmd add [q] --type skill
agentsmd workflow search [q]         →  agentsmd add [q] --type workflow
agentsmd agent install <n>           →  agentsmd add <n> [-y]
agentsmd skill install <n>           →  agentsmd add <n> [-y]
agentsmd workflow install <n>        →  agentsmd add <n> [-y]
agentsmd agent info <n>              →  (shown inline in 'add' flow)
agentsmd skill info <n>              →  (shown inline in 'add' flow)
agentsmd workflow info <n>           →  (shown inline in 'add' flow)
agentsmd agent uninstall <n>         →  agentsmd remove <n> [-y]
agentsmd skill uninstall <n>         →  agentsmd remove <n> [-y]
agentsmd workflow uninstall <n>      →  agentsmd remove <n> [-y]
agentsmd agent list                  →  agentsmd list --type agent
agentsmd skill list                  →  agentsmd list --type skill
agentsmd workflow list               →  agentsmd list --type workflow
agentsmd list                        →  agentsmd list
agentsmd sync                        →  agentsmd sync
agentsmd status                      →  agentsmd status
(none)                               →  agentsmd update (NEW)
```

---

## Design Principles

1. **Search → Act, not Search → Type → Act** — The result of browsing should be actionable
2. **Type is a filter, not a namespace** — Users think "install sdd-tdd", not "workflow install sdd-tdd"
3. **Interactive by default, scriptable with flags** — `--yes`, `--json`, `--type`
4. **Progressive disclosure** — Simple flow by default, details on demand
5. **Fewer commands to memorize** — `add`/`remove`/`list` covers 90% of use cases
