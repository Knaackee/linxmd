---
name: project-memory
type: skill
level: core
version: 2.0.0
description: >
  Three-tier memory architecture: Session (traces), Project (.linxmd/memory/),
  and Global (~/.linxmd/). How to read, write, and maintain project memory.
quickActions:
  - id: qa-memory-entry
    label: Record Decision
    prompt: Extract lasting decisions and lessons into concise memory entries with context, outcome, and when to revisit.
    trigger:
      fileMatch:
        - '^PROJECT\.md$'
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
      contentMatch:
        - 'Decision|Learning|Risk|Assumption'
tags: [core, memory, knowledge-management, context, persistence]
---

# Project Memory Skill

> Memory bridges the gap between sessions. Three tiers ensure nothing valuable is lost and agents start every session with full context.

## Three-Tier Architecture

### Tier 1 — Session Memory (Ephemeral)

```
.linxmd/traces/<session>.md
```

- Written by every agent at the end of every session
- Contains: what was done, decisions made, files modified, still-open items
- Cleared after distillation by `memory-distiller` (marked, not deleted)
- Format: see `trace-writing` skill

### Tier 2 — Project Memory (Persistent)

```
.linxmd/memory/
├── decisions/          ← ADRs (Architecture Decision Records)
│   ├── ADR-001.md
│   └── ADR-002.md
├── learnings/          ← "We tried X, it failed because Y"
│   └── 2026-03-23-auth-approach.md
├── patterns.md         ← Recurring patterns worth codifying
├── antipatterns.md     ← Mistakes to avoid (with context)
└── benchmarks/         ← Performance baselines
    └── baseline.json
```

**Decisions** — When you choose between alternatives, document it:
```markdown
---
id: ADR-NNN
title: "Why we chose X"
status: accepted       # proposed | accepted | deprecated | superseded
date: 2026-03-23
---
## Context → ## Decision → ## Consequences
```

**Learnings** — When something worked or failed, capture it:
```markdown
## Learning: <title>
- Context: What were we doing?
- What happened: What went right/wrong?
- Lesson: What should we do next time?
```

### Tier 3 — Global Memory (Cross-Project)

```
~/.linxmd/
├── user-profile.md     ← Identity, preferences, communication style (see user-profile skill)
├── global/
│   ├── patterns.md     ← Patterns that work across all your projects
│   └── antipatterns.md ← Mistakes to avoid everywhere
└── config.md           ← Tool defaults (optional)
```

Global memory is personal and persists across all projects. Agents read it at startup to personalize their behavior.

#### Global Environment Bootstrap

The `~/.linxmd/` directory is created during `project-start` workflow (Phase 0) or manually. The `onboarder` agent checks for its existence and runs the `user-profile` skill interview if `user-profile.md` is missing.

#### Promoting Project Knowledge to Global

When a learning or pattern proves useful across multiple projects:
1. `memory-distiller` flags the candidate (tag: `promote-candidate`)
2. Human reviews and approves promotion
3. Entry is copied to `~/.linxmd/global/patterns.md` or `antipatterns.md`
4. Original project entry gets a `promoted: true` marker

Never auto-promote. Cross-project patterns require human judgment.

## Agent Startup Protocol

Every agent follows this at session start:
1. Read `PROJECT.md` (mandatory — stop if missing)
2. Read `~/.linxmd/user-profile.md` (optional — adapt if present)
3. Read relevant `.linxmd/memory/decisions/` ADRs
4. Read relevant `.linxmd/memory/learnings/`
5. Read recent `.linxmd/traces/` for ongoing task context

## Agent Shutdown Protocol

Every agent follows this at session end:
1. Write session trace to `.linxmd/traces/`
2. Update task status in `.linxmd/tasks/`
3. If a significant decision was made, write an ADR
4. If something was learned, write a learning

## Memory Hygiene

- **Don't hoard** — only distill genuinely useful information
- **Deduplicate** — check before adding a new learning/pattern
- **Keep it scannable** — entries should be readable in seconds
- **Link back** — every memory entry traces to its source
- **Review periodically** — deprecated decisions should be marked
