---
name: task-management
type: skill
version: 0.2.0
description: Task system with backlog, specs, and task tracking
deps: []
tags:
  - tasks
  - backlog
  - management
---

# Task Management Skill

## Structure

```
.linxmd/tasks/
├── backlog/              ← one file per idea (free text or Issue: #NNN)
└── in-progress/
    └── [name]/
        ├── SPEC.md       ← acceptance criteria, edge cases, non-goals
        ├── TASKS.md      ← checklist — one task = one commit
        └── NOTES.md      ← agent notes, blockers, open decisions
```

## Show Backlog

Triggered by: "backlog", "show backlog", "what's in the backlog"

List `.linxmd/tasks/backlog/` and `.linxmd/tasks/in-progress/`:

```
Backlog ([N] items):
1. [filename] — [first line]
2. [filename] — [first line]

In progress:
- [name] — Task [X]/[Y] complete
```

## Add to Backlog

Triggered by: "add to backlog [text]"

Create `.linxmd/tasks/backlog/[slug].md` with the provided text.
Output: "Added to backlog: [name]"

## SPEC.md Format

```markdown
# SPEC: [Feature Name]
**Mode**: [autonomous | guided]
**Source**: [backlog file]
**Created**: [date]

## What we're building
[2-3 sentences from the user's perspective]

## Acceptance Criteria
- [ ] [concrete, testable criterion]

## Edge Cases
- [empty / null / missing inputs]

## Non-Goals
- [explicitly out of scope]

## Open Questions
- [decisions needed]
```

## TASKS.md Format

```markdown
# TASKS: [Feature Name]

Each task = RED → GREEN → SPEC-REVIEW → QUALITY-REVIEW → DOCS → COMMIT

- [ ] **Task 1**: [name]
  - Criteria: [which Acceptance Criteria this covers]
  - Tests:    [unit / integration / E2E]
  - Docs:     [which file to update, or "none"]
  - Commit:   `[type]: [message]`
```

## NOTES.md Structure

```markdown
# NOTES: [Feature Name]

## Open Decisions
- [question] — waiting on: [person/date]

## Blocked
- [task name] — blocked by: [exact blocker] — unblocks when: [condition]

## Run Log
[STEP] agent:name — STATUS — summary

## Agent Notes
[free text observations from agents during execution]
```

**Blocked rule**: A blocked item must name the exact blocker and the condition that would unblock it. Vague entries ("blocked on stuff") are not valid.

## Definition of Done

**Code task**: All AC tests green + no regressions + reviewer-spec PASS + reviewer-quality PASS + docs updated
**Docs task**: File updated + all links resolve + code examples verified + reviewed by one other agent or person
**Design/Spec task**: SPEC.md approved + all Open Questions resolved + Non-Goals explicitly listed

## Status

Triggered by: "status", "progress", "what are we working on"

Show all in-progress features with their progress.

## Done

Done = all tasks checked off + PR open + docs updated + tests green.
No "done" folder — done lives in git history.

