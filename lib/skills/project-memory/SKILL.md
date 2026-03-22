---
name: project-memory
type: skill
version: 1.0.0
description: Query and maintain a structured project knowledge base (ADRs, changelog, known issues) via the linxmd memory CLI — decisions survive context resets, new sessions, and team turnover
deps: []
tags:
  - memory
  - decisions
  - changelog
  - adr
---

# Project Memory Skill

Triggered by: "record this decision", "update the changelog", "why did we…", "what was decided about…", "search the project history", or any long-running project that needs durable, searchable history.

## Why This Exists

Both LLMs and teams suffer from context amnesia. Decisions made months ago get revisited, bugs are fixed twice, and new members have no way to understand *why* the codebase is the way it is. This skill creates a durable, file-based knowledge base that survives context resets, new sessions, and team churn — with a full-text CLI search layer on top.

## Agent Protocol

Run this protocol at the start of every session where project history is relevant:

### 1. Ensure the index is fresh

```bash
linxmd memory index
```

This is idempotent and always safe to run. It scans all memory source files and rebuilds the SQLite index from scratch. If `.linxmd/memory.db` does not exist yet, this creates it.

### 2. Orient: what was recently recorded?

```bash
linxmd memory recent --limit 5
```

Shows the 5 most recently indexed entries across all types. Use this to catch decisions made since your last session without needing specific keywords.

### 3. Before any architectural decision — search first

```bash
linxmd memory search "<topic keywords>"
```

If results appear: read the matched source `.md` file(s) before deciding — the snippet shows context, but the full ADR has the rationale.
If no results: no prior decision on this topic exists — proceed, then record a new ADR.

### 4. After recording a new ADR

```bash
linxmd memory index   # rebuild so the new ADR is immediately searchable
```

### 5. Recovery: index missing or corrupt

```bash
linxmd memory index   # always recreates from the markdown source files
```

The markdown files are the canonical source of truth. The db is always derived.

---

## CLI Command Reference

All commands accept `--project <dir>` (alias `-p`) to specify the project root when running from a subdirectory.

| Command | Use when | Default |
|---------|----------|---------|
| `linxmd memory index` | Session start; after writing new ADR/changelog entry | — |
| `linxmd memory search <query>` | You know the topic — keyword-driven lookup | `--limit 5` |
| `linxmd memory recent` | Session start orientation; "what changed lately?" | `--limit 10` |
| `linxmd memory stats` | Sanity check — how many entries are indexed? | — |

### Options

```
linxmd memory search <query> [--limit|-n <N>]
linxmd memory recent         [--limit|-n <N>] [--type decision|changelog|issue]
linxmd memory index          [--project|-p <dir>]
linxmd memory stats          [--project|-p <dir>]
```

### Example: stats output

```
  changelog        120
  decision          42
  issue              8
```

(One line per type, sorted alphabetically — not a summary sentence.)

---

## FTS5 Quick Reference

`linxmd memory search` uses SQLite FTS5. Use these patterns for precise queries:

| Pattern | Example | Matches |
|---------|---------|---------|
| Keyword | `postgres` | any entry containing "postgres" |
| Phrase | `"event sourcing"` | exact phrase |
| Boolean AND | `auth jwt` | both words (space = AND) |
| Boolean OR | `auth OR oauth` | either word |
| Boolean NOT | `auth NOT ldap` | first but not second |
| Prefix | `deploy*` | "deploy", "deployment", "deployer" |
| Column filter | `title:postgres` | keyword only in title field |

**Tip:** Start with a single keyword. Narrow with phrases or AND if too many results appear.

---

## What Gets Indexed

The indexer scans exactly these paths (relative to project root):

| Path | Indexed as | Notes |
|------|-----------|-------|
| `docs/decisions/*.md` | `decision` | One entry per file; `README.md` is skipped |
| `CHANGELOG.md` | `changelog` | One entry per `## [version]` block |
| `KNOWN_ISSUES.md` | `issue` | One entry per table row |

Custom paths (e.g. `docs/notes/*.md`) are **not** indexed in the current version. Adding custom source paths requires a CLI upgrade.

---

## Memory File Structure

```
docs/
├── decisions/                  ← Architecture Decision Records (ADRs)
│   ├── 0001-use-postgres.md
│   ├── 0002-adopt-event-sourcing.md
│   └── README.md               ← index table of all decisions
CHANGELOG.md                    ← keep-a-changelog format (repo root)
KNOWN_ISSUES.md                 ← open, deferred, resolved issues (repo root)
.linxmd/
└── memory.db                   ← derived SQLite index (git-ignorable)
```

---

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

```markdown
| # | Title | Status | Date |
|---|-------|--------|------|
| 0001 | Use PostgreSQL | Accepted | 2025-01-15 |
```

**Index size limit:** If this file exceeds 200 rows, move ADRs older than 2 years to `docs/decisions/archive/` and create `docs/decisions/README-archive.md`. Never delete ADRs — only archive them.

---

## CHANGELOG.md (Keep a Changelog Format)

```markdown
# Changelog

All notable changes to this project will be documented in this file.
Format: [Keep a Changelog](https://keepachangelog.com/) | [SemVer](https://semver.org/)

## [Unreleased]

### Added
- [description of new feature]

## [1.2.0] — 2025-03-01
[...]
```

CHANGELOG entries are written at change time, not retroactively at release.
Use `agent:changelog-writer` to write entries automatically.

---

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

---

## Fallback: Pure Markdown (no linxmd installed)

Use only when `linxmd` is not available in the project. Load files **selectively** — never load the entire `docs/decisions/` directory at once:

1. Load `docs/decisions/README.md` — scan the index table for relevant titles
2. Load only the 1–3 ADR files whose titles match your question
3. For CHANGELOG: load only the `## [Unreleased]` block
4. For KNOWN_ISSUES: read only the `## Open` table rows; skip Resolved
5. Hard cap: at most 3 full ADR files per question. If you need more, the problem is too broad for a single context window.

---

## Rules

- Every significant architectural decision gets an ADR — not just tech stack choices
- ADR status must be updated when decisions are revisited (`Deprecated` or `Superseded by NNNN`)
- Never delete an ADR — supersede or deprecate it
- CHANGELOG entries are written at the time of the change, not retroactively at release
- KNOWN_ISSUES.md is updated every time a bug is found or fixed
- Always run `linxmd memory search` before asking "why did we…" or making a reversible decision

## When NOT to Use

- For trivial decisions with obvious rationale — no ADR needed
- For operational secrets or credentials — use a secrets manager, not markdown
- For temporary notes that don't outlive the current session — use `NOTES.md` in task-management instead

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
