# Linxmd Library — v2.0

> AI-augmented development framework: agents, skills, workflows, and packs.

## What Is This?

Linxmd is a curated library of **agents**, **skills**, **workflows**, and **packs** that turn LLM-based coding assistants into structured, auditable, human-guided development partners.

This is not "let the AI do everything." This is "give the AI clear roles, rules, memory, and human oversight so it produces reliable, high-quality work."

## Quick Install

```bash
linxmd install packs/fullstack-tdd        # Complete TDD development bundle
linxmd install packs/new-project-kickstart # Onboarding bundle for new projects
linxmd install agents/implementer          # Or install individual artifacts
```

---

## Getting Started

### First Time Ever? (Global Setup)

The very first time you use Linxmd, the system needs to know who you are. This happens **once** and is shared across all projects.

```
┌─────────────────────────────────────────────────────────────┐
│  YOU                          ONBOARDER AGENT               │
│                                                             │
│  "Set up Linxmd"  ──────────► Creates ~/.linxmd/            │
│                               Creates ~/.linxmd/global/     │
│                                                             │
│                    ◄────────  "What's your name and role?"  │
│  "Ben, senior dev" ────────►                                │
│                    ◄────────  "Preferred language? TDD?"     │
│  "Deutsch, strict" ────────►                                │
│                    ◄────────  "Review depth? Auto-fix?"      │
│  "Thorough, yes"   ────────►                                │
│                                                             │
│                    ◄────────  📄 ~/.linxmd/user-profile.md  │
│  "Looks good ✓"    ────────►  ★ GATE 0 passed              │
└─────────────────────────────────────────────────────────────┘
```

**Result:** `~/.linxmd/user-profile.md` exists. All agents now adapt to your preferences (language, verbosity, review depth, coding style). This step is auto-skipped for future projects.

---

### Starting a New Project (Green-Field)

Use the **`new-project-kickstart`** pack or the **`project-start`** workflow.

```
Phase 0 ─ GLOBAL ENV CHECK       (onboarder)
          ~/.linxmd/ exists? → skip or interview
          ★ GATE 0: Profile confirmed

Phase 1 ─ ANALYZE                (start-state-creator)
          Scans directory structure
          Detects tech stack from config files
          Reads existing README, CHANGELOG
          Generates draft PROJECT.md
          ★ GATE 1: You review PROJECT.md

Phase 2 ─ INIT MEMORY            (project-memory skill)
          Creates .linxmd/ directory:
          ├── memory/decisions/
          ├── memory/learnings/
          ├── tasks/
          ├── traces/
          ├── specs/
          └── inbox/

Phase 3 ─ SEED TASKS             (planner)
          Creates initial task backlog from:
          • TODOs/FIXMEs found in code
          • Missing test coverage
          • Missing documentation
          • Security gaps

Phase 4 ─ VERIFY                 (reviewer-spec)
          Cross-checks everything
          ★ GATE 2: You approve the full setup
```

**Result after `project-start`:**
```
your-project/
├── PROJECT.md              ← Every agent reads this first
├── .linxmd/
│   ├── memory/             ← Project knowledge base
│   ├── tasks/TASK-001.md   ← Seeded backlog
│   ├── traces/             ← Session records
│   └── specs/              ← Future specifications
└── (your existing code)
```

---

### Onboarding an Existing Project (Brown-Field)

Identical flow, but the **`start-state-creator`** agent does more work:

| What it detects | How |
|---|---|
| Tech stack | Reads `package.json`, `*.csproj`, `Cargo.toml`, `go.mod`, etc. |
| Architecture | Maps directory structure, identifies patterns |
| Test setup | Finds test framework, coverage config, existing tests |
| Existing docs | Imports README, CHANGELOG, docs/ folder content |
| Code debt | Finds TODO/FIXME/HACK comments, outdated deps |
| CI/CD | Reads `.github/workflows/`, `Jenkinsfile`, etc. |

Use the **`existing-project-rescue`** pack if the codebase needs cleanup — it bundles a quality audit on top of the onboarding.

---

### After Setup: Daily Development

Once a project is onboarded, you work through **workflows**. The **Router** agent classifies your request and picks the right one:

```
You say                        → Workflow               → What happens
──────────────────────────────────────────────────────────────────────────
"Add dark mode"                → feature-development    → Spec → Plan → Test → Code → Review → Merge
"Login is broken"              → bug-fix                → Reproduce → Fix → Regression test → Merge
"Should we use Postgres?"      → research-spike         → Research → ADR → Decision
"Release v2.1.0"               → release                → Changelog → Build → Tag → Publish
"Clean up dead code"           → consistency-sprint     → Scan → Auto-fix → Manual fix → Review
"Write a blog post"            → content-review         → Draft → Edit → Fact-check → Publish
"How healthy is this project?" → quality-baseline       → Full audit → Report → Action plan
```

### Example: Building a Feature

```
 You: "Add dark mode toggle"
  │
  ▼
 Router → feature-development workflow
  │
  ├─ 1. spec-writer     writes SPEC-042.md with acceptance criteria
  │     ★ GATE: You approve the spec
  │
  ├─ 2. planner         breaks spec into TASK-042a, 042b, 042c (1-4h each)
  │     ★ GATE: You approve the plan
  │
  ├─ 3. architect       writes ADR if design decisions needed
  │
  ├─ 4. test-writer     writes failing tests (RED phase)
  │     ★ GATE: You approve test strategy
  │
  ├─ 5. implementer     makes tests pass (GREEN phase)
  │     consistency-guardian sweeps for cleanup
  │     reviewer-quality checks security + quality
  │     ★ GATE: You approve the code
  │
  ├─ 6. preview         you see the result in context
  │     ★ GATE: You confirm it works
  │
  └─ 7. merge           docs-writer updates docs, changelog-writer updates CHANGELOG
        ★ GATE: You approve the merge
```

Every step produces a **trace** in `.linxmd/traces/` — full audit trail of what happened, what was decided, and what was learned.

---

## Core Concepts

| Concept | What It Is | Count | Example |
|---------|-----------|-------|---------|
| **Agent** | A persona with a specific role, rules, and skills | 20 | `implementer`, `reviewer-quality` |
| **Skill** | Reusable knowledge an agent applies. Stateless, composable. | 25 | `debugging`, `conventional-commits` |
| **Workflow** | Ordered agent sequence with mandatory human gates | 8 | `feature-development`, `bug-fix` |
| **Pack** | Pre-assembled bundle of agents + skills + workflow | 6 | `fullstack-tdd`, `quality-sprint` |

## Architecture Overview

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full system design, including:
- The 12 governing principles
- Memory architecture (3 tiers: Session → Project → Global)
- Gate system (mandatory human checkpoints)
- Agent categories (Core / Control / Delivery)
- Skill levels (Core / Governance / Growth)

See [PRINCIPLES.md](PRINCIPLES.md) for the non-negotiable rules every artifact follows.

Artifact structure and frontmatter specs:
- Shared frontmatter schema: [FRONTMATTER-SPEC.md](FRONTMATTER-SPEC.md)
- Agents: [agents/SPEC.md](agents/SPEC.md)
- Skills: [skills/SPEC.md](skills/SPEC.md)
- Workflows: [workflows/SPEC.md](workflows/SPEC.md)

## Directory Structure

```
lib/
├── PRINCIPLES.md           ← The 12 governing principles
├── ARCHITECTURE.md         ← Full system design document
├── README.md               ← This file
├── index.json              ← Machine-readable registry of all artifacts
│
├── agents/                 ← 20 agent definitions
│   ├── Core (7):    implementer, test-writer, architect, planner,
│   │                spec-writer, drafter, editor
│   ├── Control (4): reviewer-quality, reviewer-spec,
│   │                consistency-guardian, fact-checker
│   └── Delivery (9): router, docs-writer, changelog-writer, onboarder,
│                     brainstormer, researcher, start-state-creator,
│                     performance-monitor, memory-distiller
│
├── skills/                 ← 25 skill definitions (SKILL.md per folder)
│   ├── Core (11):       debugging, refactoring, task-management, api-design,
│   │                    project-memory, user-profile, context-management,
│   │                    code-translator, design-tokens, i18n, text-translator
│   ├── Governance (6):  observability, conventional-commits, trace-writing,
│   │                    preview-delivery, consistency-check, performance-profiling
│   └── Growth (8):      worktree-management, feature-branch, e2e-testing,
│                        brainstorming, research, market-analysis,
│                        quicknote, start-state-creation
│
├── workflows/              ← 8 workflow orchestrations
│   └── feature-development, bug-fix, project-start, research-spike,
│       release, consistency-sprint, content-review, quality-baseline
│
└── packs/                  ← 6 ready-to-use bundles
    └── fullstack-tdd, quality-sprint, content-pipeline, i18n-ready,
        new-project-kickstart, existing-project-rescue
```

## Key Standards

| Standard | What | Where |
|----------|------|-------|
| **PROJECT.md** | Agent orientation document — every agent reads this first | Project root |
| **User Profile** | Personal preferences — language, style, review depth | `~/.linxmd/user-profile.md` |
| **Trace Files** | Per-session audit trail of every agent action | `.linxmd/traces/` |
| **Task Frontmatter v2** | Unified YAML schema for all tasks | `.linxmd/tasks/` |
| **Conventional Commits** | `type(scope): message` on every commit | Enforced by agents |
| **Memory** | 3-tier knowledge system (Session → Project → Global) | `.linxmd/memory/` + `~/.linxmd/` |

## Principles (Summary)

1. Human-in-the-loop is constitutional
2. Four hard gates: Spec, Safety, Preview, Benchmark
3. Memory is the operating system (3 tiers)
4. Dual traceability (agent traces + code observability)
5. Preview before ship
6. Eval-driven development
7. Security and policy are first-class
8. Git discipline is enforced
9. Clear role separation (Core / Control / Delivery)
10. Skills in three levels (Core / Governance / Growth)
11. Incident-to-learning loop
12. Metrics-driven steering

See [PRINCIPLES.md](PRINCIPLES.md) for the full version.

---

*Linxmd v2.0 — Built 2026-03-23 after researching 20+ state-of-the-art agent frameworks.*
