---
name: project-memory
type: skill
version: 0.3.0
description: Maintain a structured knowledge base for your project — ADR decisions, CHANGELOG entries, and a known-issues log so context survives context resets, new sessions, and team turnover
deps: []
tags:
  - memory
  - decisions
  - changelog
  - adr
---

# Project Memory Skill

Triggered by: "record this decision", "update the changelog", "why did we…", "what was decided about…", or any long-running project that needs durable, searchable history.

## Why This Exists

Both LLMs and teams suffer from context amnesia. Decisions made months ago get revisited, bugs are fixed twice, and new members have no way to understand *why* the codebase is the way it is. This skill creates a durable, file-based knowledge base that survives context resets, new sessions, and team churn.

## Structure

```
docs/
├── decisions/                  ← Architecture Decision Records (ADRs)
│   ├── 0001-use-postgres.md
│   ├── 0002-adopt-event-sourcing.md
│   └── README.md               ← index of all decisions
├── KNOWN_ISSUES.md             ← open, deferred, resolved issues
CHANGELOG.md                    ← keep-a-changelog format (repo root)
```

## Architecture Decision Records (ADRs)

### When to Write an ADR

Write an ADR when the decision is:
- Hard to reverse
- Confusing to a future team member without context
- The result of a debate where a non-obvious choice was made
- A pattern that will be repeated

**Skip the ADR** for: trivial choices with obvious rationale, short-lived experiments.

### ADR Format

File: `docs/decisions/[NNNN]-[slug].md`

```markdown
# [NNNN] [Title]

**Status**: Proposed | Accepted | Deprecated | Superseded by [NNNN]
**Date**: YYYY-MM-DD
**Deciders**: [who was involved]

## Context

[What problem were we solving? What constraints existed?]

## Options Considered

### Option A: [Name]
[description, pros, cons]

### Option B: [Name]
[description, pros, cons]

## Decision

We chose **Option A** because [rationale].

## Consequences

**Good**: [what improves]
**Bad**: [what gets harder or is now a constraint]
**Neutral**: [trade-offs with no clear winner]
```

### ADR Index (`docs/decisions/README.md`)

Maintained as an auto-updated table:

```markdown
| # | Title | Status | Date |
|---|-------|--------|------|
| 0001 | Use PostgreSQL | Accepted | 2025-01-15 |
```

## CHANGELOG.md (Keep a Changelog Format)

```markdown
# Changelog

All notable changes to this project will be documented in this file.
Format: [Keep a Changelog](https://keepachangelog.com/) | [SemVer](https://semver.org/)

## [Unreleased]

### Added
- [description of new feature]

### Fixed
- [description of bug fix]

## [1.2.0] — 2025-03-01
[...]
```

CHANGELOG entries are written at change time, not at release time.
Use `agent:changelog-writer` to write entries automatically.

## KNOWN_ISSUES.md

```markdown
# Known Issues

## Open
| # | Summary | Severity | Since | Workaround |
|---|---------|----------|-------|-----------|
| KI-001 | Login fails if email contains + | Medium | v1.1.0 | URL-encode email |

## Deferred
| # | Summary | Target | Reason |
|---|---------|--------|--------|

## Resolved
| # | Summary | Fixed In |
|---|---------|---------|
```

## Search Protocol

Before making a significant decision, check the project memory in this order:

### With `linxmd memory` (recommended for projects with many ADRs)

If the project has `linxmd` installed and the memory index exists (`.linxmd/memory.db`):

```bash
linxmd memory search "<relevant keywords>"   # FTS5 full-text search, returns top 5
linxmd memory stats                          # "42 decisions, 8 issues, 120 changelog entries"
linxmd memory recent --type decision         # last 10 ADRs, newest first
```

Build or refresh the index whenever memory files change:
```bash
linxmd memory index    # scans docs/decisions/*.md, CHANGELOG.md, KNOWN_ISSUES.md
```

The index file (`.linxmd/memory.db`) is a derived SQLite cache — add it to `.gitignore`. The markdown files remain the canonical source of truth.

### Without the index (pure markdown)

Load files **selectively** — never load the entire `docs/decisions/` directory at once:

1. Load `docs/decisions/README.md` (the index table, ~3KB) — scan titles for relevant keywords
2. Load only the 1-3 ADR files whose titles match your question
3. For CHANGELOG: load only the `## [Unreleased]` block — not the full history
4. For KNOWN_ISSUES: filter to `## Open` section rows; skip Resolved rows
5. Hard cap: load at most 3 full ADR files per decision. If you need more, the problem requires more research than a single context window can provide.

**Index size limit:** If `docs/decisions/README.md` exceeds 200 rows, move ADRs older than 2 years to `docs/decisions/archive/` and create a matching `docs/decisions/README-archive.md`. Never delete ADRs — only archive them.

## Rules

- Every significant architectural decision gets an ADR — not just tech stack choices
- ADR status must be updated when decisions are revisited (`Deprecated` or `Superseded by NNNN`)
- Never delete an ADR — supersede or deprecate it
- CHANGELOG entries are written at the time of the change, not retroactively at release
- KNOWN_ISSUES.md is updated every time a bug is found or fixed
- When searching for "why we did X", check `docs/decisions/` before asking or guessing

## When NOT to Use

- For trivial decisions with obvious rationale — no ADR needed
- For operational secrets or credentials — use a secrets manager, not markdown
- For temporary notes that don't need to outlive the current session — use `NOTES.md` in task-management instead

## Stability Contract

This skill's file and directory names are a **public API**. Every workflow that records or queries project memory depends on the exact paths below.

| Path | Role |
|------|------|
| `docs/decisions/` | ADR directory — never rename |
| `docs/decisions/README.md` | ADR index — never delete |
| `docs/decisions/[NNNN]-*.md` | ADR naming pattern — never change |
| `CHANGELOG.md` | Changelog — always at project root |
| `KNOWN_ISSUES.md` | Issue log — always at project root |
| `.linxmd/memory.db` | Derived SQLite index — gitignore-able, safe to delete/rebuild |

Breaking changes to any path above require a major version bump and a migration note.
