# Linxmd v2 — System Architecture

> The definitive guide to how agents, skills, workflows, packs, and memory work together.

---

## 1. Design Philosophy

Linxmd v2 is a **human-gated, agent-assisted** development framework. AI agents do the heavy lifting — writing specs, tests, code, docs — but **humans approve every critical checkpoint**.

Three governing ideas:

1. **Agents execute, humans decide.** No commit, merge, or release happens without explicit sign-off.
2. **Everything is traceable.** Every session produces a trace file; every decision links to an artifact.
3. **Quality is structural, not optional.** Security, consistency, and performance checks are built into the workflow — not afterthoughts.

The full set of 12 principles lives in [PRINCIPLES.md](PRINCIPLES.md).

---

## 2. System Layers

```
┌─────────────────────────────────────────────────┐
│                    PACKS                         │  ← Pre-assembled bundles
│   fullstack-tdd · quality-sprint · content-...  │    for common scenarios
├─────────────────────────────────────────────────┤
│                  WORKFLOWS                       │  ← Ordered phase sequences
│   feature-development · bug-fix · release · ... │    with hard gates
├─────────────────────────────────────────────────┤
│                   AGENTS                         │  ← Autonomous actors with
│   implementer · test-writer · architect · ...   │    defined responsibilities
├─────────────────────────────────────────────────┤
│                   SKILLS                         │  ← Reusable knowledge units
│   debugging · refactoring · observability · ... │    agents reference
├─────────────────────────────────────────────────┤
│                   MEMORY                         │  ← Persistent knowledge
│   Session traces · Project memory · Global      │    across sessions
└─────────────────────────────────────────────────┘
```

Each layer depends only on the layer(s) below it. Packs compose workflows + agents + skills. Workflows orchestrate agents. Agents use skills. Memory underlies everything.

---

## 3. Agent Model

### 3.1 Categories

| Category     | Purpose                        | Examples                                              |
|-------------|-------------------------------|-------------------------------------------------------|
| **Core**     | Create artifacts               | implementer, test-writer, architect, planner, spec-writer, drafter, editor |
| **Control**  | Guard quality standards        | reviewer-quality, reviewer-spec, consistency-guardian, fact-checker |
| **Delivery** | Package output for humans      | router, docs-writer, changelog-writer, onboarder, brainstormer, researcher, start-state-creator, performance-monitor, memory-distiller |

### 3.2 Agent Lifecycle

Every agent follows the same startup sequence:

```
1. Read PROJECT.md         → Understand the project
2. Read user-profile       → Adapt to preferences
3. Read task / instruction → Know what to do
4. Read memory (traces)    → Avoid repeating mistakes
5. Execute                 → Apply skills, produce output
6. Write trace             → Record what happened
```

### 3.3 Hard Rules for All Agents

- **Never merge without approval.** Create a branch or PR, never push to main.
- **Never skip a gate.** If the workflow says "GATE: Human review", stop and wait.
- **Always leave a trace.** No silent sessions.
- **Use conventional commits.** `type(scope): message` — no exceptions.
- **Read PROJECT.md first.** Every session starts with orientation.

---

## 4. Skill System

### 4.1 Three Levels

| Level          | Purpose                          | Count | Examples                                               |
|---------------|----------------------------------|-------|--------------------------------------------------------|
| **Core**       | Essential craft knowledge         | 11    | debugging, refactoring, task-management, api-design, project-memory, user-profile, context-management, code-translator, design-tokens, i18n, text-translator |
| **Governance** | Standards and compliance          | 6     | observability, conventional-commits, trace-writing, preview-delivery, consistency-check, performance-profiling |
| **Growth**     | Optional expansions               | 8     | worktree-management, feature-branch, e2e-testing, brainstorming, research, market-analysis, quicknote, start-state-creation |

### 4.2 Skill Format

Each skill lives in `skills/<name>/SKILL.md` and contains:

- **When to Apply** — triggers and contexts
- **Rules** — mandatory standards
- **Patterns** — reusable templates and examples
- **Anti-Patterns** — what to avoid
- **Exit Criteria** — how to know the skill was applied correctly

Agents reference skills by name. A skill never "runs" — it's knowledge that an agent applies.

---

## 5. Workflow Engine

### 5.1 Workflow Catalog

| Workflow               | Purpose                              | Gates | Primary Agents                                |
|-----------------------|--------------------------------------|-------|-----------------------------------------------|
| **feature-development** | Full feature from idea to merge       | 6     | spec-writer → planner → architect → test-writer → implementer → reviewer-quality |
| **bug-fix**            | Reproduce-first bug resolution        | 3     | test-writer → implementer → reviewer-quality   |
| **project-start**      | Onboard codebase, generate PROJECT.md | 3     | onboarder → start-state-creator → planner       |
| **research-spike**     | Investigate before committing         | 2     | researcher → architect                          |
| **release**            | Version, tag, publish                 | 2     | changelog-writer → reviewer-quality → publish   |
| **consistency-sprint** | Codebase-wide cleanup sweep           | 2     | consistency-guardian → implementer              |
| **content-review**     | Write → edit → publish content        | 3     | drafter → editor → fact-checker                 |
| **quality-baseline**   | Full project health audit             | 2     | reviewer-quality → consistency-guardian → docs-writer |

### 5.2 Gate System

Gates are **mandatory human checkpoints** where the workflow pauses and waits for approval.

```
Phase 1  →  Phase 2  →  🚧 GATE  →  Phase 3  →  Phase 4  →  🚧 GATE  →  ...
                         │                                      │
                         └─ Human reviews                       └─ Human approves
                            spec / plan / code                     preview / merge
```

**Gate types across the system:**

| Gate       | Where Used             | Human Verifies                           |
|-----------|------------------------|------------------------------------------|
| Profile Gate | project-start        | User profile correct, preferences match   |
| Spec Gate  | feature-development    | Requirements complete, scope correct      |
| Plan Gate  | feature-development    | Tasks reasonable, estimates acceptable    |
| Test Gate  | feature-development    | Test strategy covers acceptance criteria  |
| Impl Gate  | feature-development, bug-fix | Code correct, clean, secure          |
| Preview Gate | feature-development, content-review | Output looks right in context    |
| Final Gate | All workflows          | Ready to merge / publish / release        |

### 5.3 Workflow Selection

The **Router** agent classifies incoming requests and routes to the appropriate workflow:

```
"Add dark mode"           → feature-development
"Login doesn't work"      → bug-fix
"Set up new project"      → project-start
"Should we use Postgres?" → research-spike
"Release v2.1.0"          → release
"Clean up dead code"      → consistency-sprint
"Write blog post"         → content-review
"How healthy is this?"    → quality-baseline
```

---

## 6. Pack System

Packs are **pre-assembled bundles** for common scenarios. Instead of manually selecting agents, skills, and workflows, pick a pack and get everything configured.

| Pack                      | Workflow            | Agents | Skills | Best For                         |
|--------------------------|---------------------|--------|--------|----------------------------------|
| **fullstack-tdd**        | feature-development  | 12     | 15     | Standard feature development      |
| **quality-sprint**       | quality-baseline     | 7      | 7      | Codebase health improvement       |
| **content-pipeline**     | content-review       | 4      | 3      | Articles, docs, proposals         |
| **i18n-ready**           | feature-development  | 3      | 5      | Internationalized features        |
| **new-project-kickstart** | project-start       | 6      | 6      | Green-field or first-time setup   |
| **existing-project-rescue** | quality-baseline  | 9      | 9      | Legacy codebase improvement       |

---

## 7. Memory Architecture

Memory is the connective tissue that makes agents learn across sessions.

### 7.1 Three Tiers

```
┌─────────────────────────────────────────┐
│ Tier 3: Global Memory                   │  ← Cross-project patterns
│ ~/.linxmd/global/                       │    (rare updates)
├─────────────────────────────────────────┤
│ Tier 2: Project Memory                  │  ← Project-scoped knowledge
│ .linxmd/memory/                         │    (grows over time)
│   decisions.md                          │
│   patterns.md                           │
│   gotchas.md                            │
├─────────────────────────────────────────┤
│ Tier 1: Session Traces                  │  ← Per-session records
│ .linxmd/traces/                         │    (immutable after close)
│   2026-03-23-feature-dark-mode.md       │
└─────────────────────────────────────────┘
```

### 7.2 Lifecycle

| Event                | Memory Action                                           |
|---------------------|---------------------------------------------------------|
| Session start        | Agent reads PROJECT.md + relevant traces + project memory |
| During session       | Agent writes to current session trace                    |
| Session end          | Trace file closed and marked immutable                   |
| Memory distillation  | `memory-distiller` extracts patterns → project memory    |
| Cross-project sync   | Manual promotion of patterns → global memory             |

### 7.3 PROJECT.md

The **PROJECT.md** file is the root orientation document every agent reads first. Created by `onboarder` or `start-state-creator`.

Required sections:
- **Project** — Name, purpose, status
- **Tech Stack** — Languages, frameworks, key dependencies
- **Architecture** — High-level structure, key patterns
- **Standards** — Coding conventions, commit format, branch strategy
- **Team** — Human roles and preferences
- **Open Decisions** — Pending architectural or team choices

---

## 8. Standards Reference

### 8.1 Task Frontmatter v2

Every task file uses this YAML frontmatter:

```yaml
---
id: TASK-042
title: Implement dark mode toggle
status: backlog | in-progress | review | done | blocked
priority: low | medium | high | critical
size: XS | S | M | L | XL
estimate: 2h
assignee: implementer
depends: [TASK-040, TASK-041]
tags: [ui, dark-mode]
created: 2026-03-23
updated: 2026-03-23
---
```

### 8.2 Conventional Commits

```
type(scope): description

feat(auth):     add OAuth2 login flow
fix(api):       handle null response in user endpoint
refactor(ui):   extract ThemeProvider component
test(auth):     add OAuth2 integration tests
docs(readme):   update installation instructions
chore(deps):    bump typescript to 5.4
```

### 8.3 Session Trace Format

```markdown
---
session: 2026-03-23-dark-mode
agent: implementer
workflow: feature-development
started: 2026-03-23T09:00:00Z
ended: 2026-03-23T11:30:00Z
status: completed
---

## Goal
Implement dark mode toggle component.

## Decisions
- Used CSS custom properties over Tailwind dark: prefix (ADR-007)

## Changes
- src/components/ThemeToggle.tsx (created)
- src/styles/tokens.css (modified)

## Issues
- None

## Learnings
- CSS custom properties cascade better for dynamic theming
```

### 8.4 Artifact Frontmatter v2.1

All artifact types (`agent`, `skill`, `workflow`, `pack`) share an extensible frontmatter schema.

```yaml
---
name: example-artifact
type: skill
version: 2.1.0
description: Example
deps: []
tags: []
supported: []
quickActions: []
lifecycle: {}
---
```

#### Quick Actions

- `quickActions` are context-aware prompt shortcuts.
- `trigger.fileMatch` is a list of regex patterns with OR semantics.
- `trigger.fileExclude` is a list of regex patterns with OR semantics.
- Trigger groups (`fileMatch`, `workspaceHas`, `languageId`, etc.) are combined with AND semantics.

#### Lifecycle Hooks

Supported events:

- `preInstall`
- `postInstall`
- `preUninstall`
- `postUninstall`
- `preUpdate`
- `postUpdate`

Each hook supports `id`, `label`, `prompt`, `blocking`, and `requiresConfirmation`.

Normative schema and examples are maintained in:

- [FRONTMATTER-SPEC.md](FRONTMATTER-SPEC.md)
- [agents/SPEC.md](agents/SPEC.md)
- [skills/SPEC.md](skills/SPEC.md)
- [workflows/SPEC.md](workflows/SPEC.md)

---

## 9. Directory Layout

```
lib/
├── PRINCIPLES.md          ← 12 governing principles
├── ARCHITECTURE.md        ← This document
├── README.md              ← Quick start and overview
├── index.json             ← Machine-readable artifact registry
├── agents/                ← 20 agent definitions
│   ├── implementer.md
│   ├── test-writer.md
│   ├── architect.md
│   └── ...
├── skills/                ← 25 skill definitions
│   ├── debugging/
│   │   └── SKILL.md
│   ├── refactoring/
│   │   └── SKILL.md
│   └── ...
├── workflows/             ← 8 workflow orchestrations
│   ├── feature-development.md
│   ├── bug-fix.md
│   └── ...
└── packs/                 ← 6 ready-to-use bundles
    ├── fullstack-tdd.md
    ├── quality-sprint.md
    └── ...
```

---

## 10. Evolution Strategy

This system is designed to grow:

### Adding a New Agent

1. Create `lib/agents/<name>.md` with frontmatter (type: agent, category, skills, tags)
2. Add entry to `lib/index.json`
3. Reference from relevant workflows and packs

### Adding a New Skill

1. Create `lib/skills/<name>/SKILL.md` with frontmatter (type: skill, level, tags)
2. Add entry to `lib/index.json`
3. Assign to relevant agents

### Adding a New Workflow

1. Create `lib/workflows/<name>.md` with frontmatter (type: workflow, agents, gates, tags)
2. Add entry to `lib/index.json`
3. Optionally create a pack that uses it

### Version Strategy

- All artifacts start at `2.0.0` (v2 system)
- Bump patch for fixes, minor for additions, major for breaking changes
- `index.json` tracks version per artifact

---

## 11. Quick Reference

### Agent Count: 20

| Core (7)       | Control (4)            | Delivery (9)           |
|----------------|------------------------|------------------------|
| implementer    | reviewer-quality       | router                 |
| test-writer    | reviewer-spec          | docs-writer            |
| architect      | consistency-guardian    | changelog-writer       |
| planner        | fact-checker           | onboarder              |
| spec-writer    |                        | brainstormer           |
| drafter        |                        | researcher             |
| editor         |                        | start-state-creator    |
|                |                        | performance-monitor    |
|                |                        | memory-distiller       |

### Skill Count: 25

| Core (11)            | Governance (6)         | Growth (8)              |
|---------------------|------------------------|-------------------------|
| debugging           | observability          | worktree-management     |
| refactoring         | conventional-commits   | feature-branch          |
| task-management     | trace-writing          | e2e-testing             |
| api-design          | preview-delivery       | brainstorming           |
| project-memory      | consistency-check      | research                |
| user-profile        | performance-profiling  | market-analysis         |
| context-management  |                        | quicknote               |
| code-translator     |                        | start-state-creation    |
| design-tokens       |                        |                         |
| i18n                |                        |                         |
| text-translator     |                        |                         |

### Workflow Count: 8

feature-development · bug-fix · project-start · research-spike · release · consistency-sprint · content-review · quality-baseline

### Pack Count: 6

fullstack-tdd · quality-sprint · content-pipeline · i18n-ready · new-project-kickstart · existing-project-rescue
