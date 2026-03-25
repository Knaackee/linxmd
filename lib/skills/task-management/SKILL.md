---
name: task-management
type: skill
level: core
version: 2.0.0
description: >
  Unified task frontmatter v2 schema, task lifecycle, status transitions,
  and task file management in .linxmd/tasks/.
quickActions:
  - id: qa-task-frontmatter-fix
    icon: "🔧"
    label: Fix Task Frontmatter
    prompt: Correct task frontmatter to match schema, preserve intent, and list assumptions where data is missing.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/.*\.md$'
      languageId: [markdown]
      contentMatch:
        - 'status:|priority:|estimate:'
  - id: qa-task-state-transition
    icon: "🔀"
    label: Validate State Transition
    prompt: Validate allowed state transitions and flag conflicts between status, blockers, and dependency fields.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/.*\.md$'
      languageId: [markdown]
      contentMatch:
        - 'status:|blocked-by:|blocks:'
tags: [core, tasks, frontmatter, lifecycle, kanban]
---

# Task Management Skill

> Every unit of work is a task with machine-readable frontmatter. Tasks flow through a defined lifecycle with clear status transitions.

## Task Frontmatter v2 Schema

```yaml
---
id: TASK-NNN
title: "Short descriptive title"
type: feature          # feature | bug | spike | chore | research | review
status: backlog        # backlog | planned | in-progress | review | done | blocked
priority: critical     # critical | high | medium | low
sprint: 2026-S13
branch: feat/short-desc
spec: .linxmd/specs/SPEC-NNN.md
estimate: 2h           # 1h | 2h | 4h (max per task)
blocked-by: []
blocks: []
acceptance:
  - "Criterion one"
  - "Criterion two"
tags: [relevant, tags]
assigned: implementer
created: 2026-03-23
updated: 2026-03-23
---
```

## Status Transitions

```
backlog → planned → in-progress → review → done
                  ↕                ↕
               blocked          blocked
```

Valid transitions:
| From | To | Triggered By |
|------|----|-------------|
| backlog | planned | Planner assigns to sprint |
| planned | in-progress | Agent starts work |
| in-progress | review | Agent completes implementation |
| in-progress | blocked | Dependency unavailable |
| review | done | Human approves at gate |
| review | in-progress | Human requests changes |
| blocked | in-progress | Blocker resolved |
| blocked | planned | Re-prioritized |

## Task File Location

```
.linxmd/tasks/TASK-NNN.md
```

## Task Sizing Rules

| Estimate | Guidance |
|----------|----------|
| 1h | Trivial: rename, config change, copy update |
| 2h | Small: single function, single endpoint, single component |
| 4h | Medium: feature slice, multi-file change, integration work |
| > 4h | Break it down into subtasks |

## Task Body Template

```markdown
# TASK-NNN: <Title>

## Description
What needs to be done and why.

## Affected Files
- `path/to/file.ts` — create/modify: description
- `path/to/test.ts` — create: tests for the above

## Subtasks (if applicable)
- [ ] Subtask 1
- [ ] Subtask 2

## Risks
- Risk description and mitigation

## Notes
Additional context, links, references.
```

## ID Assignment
- Use sequential numbering: TASK-001, TASK-002, ...
- Check `.linxmd/tasks/` for the highest existing number before assigning.
- Never reuse IDs, even for deleted tasks.

