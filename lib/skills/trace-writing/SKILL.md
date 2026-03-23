---
name: trace-writing
type: skill
level: governance
version: 2.0.0
description: >
  Session trace file format, mandatory content, and archival rules.
  Every agent session produces a trace — this skill defines the standard.
tags: [governance, traces, audit, session, documentation]
---

# Trace Writing Skill

> Every agent session leaves a trace. This is the audit trail that makes agent work transparent, reviewable, and learnable.

## Trace File Standard

### Location
```
.linxmd/traces/YYYY-MM-DD-HH-MM-<agent>-<task-id>.md
```

Example: `.linxmd/traces/2026-03-23-14-30-implementer-TASK-042.md`

### Frontmatter

```yaml
---
id: TRACE-2026-03-23-1430-implementer-TASK-042
agent: implementer
task: TASK-042
session-start: 2026-03-23T14:30:00Z
session-end: 2026-03-23T16:45:00Z
branch: feat/jwt-auth
commits:
  - "test(auth): add failing tests for JWT flow"
  - "feat(auth): implement JWT generation and validation"
distilled: false
---
```

### Mandatory Sections

```markdown
## What Was Done
Short summary of completed work. 2–5 sentences.

## Files Modified
- `path/to/file.ts` — description of change
- `path/to/other.ts` — description of change

## Decisions Made
- Decision 1: chose X because Y (alternative was Z)
- Decision 2: ...

## Technical Debt Noted
- Issue description — logged as TASK-NNN (or "not yet logged")

## Tests Status
- Unit: X passing, Y failing
- Integration: X passing
- E2E: not applicable / X passing

## Still Open
- Item 1: needs follow-up
- Item 2: blocked on X
```

### Optional Sections

```markdown
## Dead Code Removed
## Performance Observations
## Learnings
## Questions for Human
```

## Rules

- **Write at session end** — don't forget, even for short sessions
- **Be honest** — include failures and uncertainties, not just successes
- **Be concise** — scannable in 30 seconds
- **Link tasks** — reference TASK-NNN for anything logged
- **Never skip** — even "I investigated but found nothing" is a valid trace
