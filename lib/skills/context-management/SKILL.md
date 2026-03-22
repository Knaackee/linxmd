---
name: context-management
type: skill
version: 0.2.0
description: Strategies for maintaining coherent context across long sessions and large codebases
deps: []
tags:
  - context
  - sessions
  - token-budget
---

# Context Management Skill

Triggered by: long sessions drifting, agent losing track of constraints, "start fresh", or any task that spans more than 3 commits.

## Techniques

### Summarize Before Starting a New Task
Before beginning a new task, summarize all completed tasks into NOTES.md under `## Run Log`. Keep each entry to one line: what was done, what commit it landed in.

### Working Set Declaration
At the start of each task, declare the working set: the minimal list of files relevant to this task. Only load those files. Do not scan the whole repository unless explicitly searching.

```
Working set for Task [N]:
- src/[file] — reason
- tests/[file] — reason
```

### Token Budget Awareness
Estimate token cost as you go. When context consumption exceeds 80% of the session limit:
1. Write a checkpoint to NOTES.md (current task state, open decisions, next step)
2. Shed unneeded context (close files no longer in working set)
3. Reload only what the next step needs

### Checkpoint Pattern
Before any risky or long-running step, write a checkpoint to NOTES.md:

```markdown
## Checkpoint — [task name] — [timestamp]
- Completed: [what is done]
- In progress: [current step]
- Next: [what comes after]
- Open decisions: [list or "none"]
```

This enables re-entry if the session is interrupted.

### Spec Drift Prevention
Re-read SPEC.md at the start of each new task. Do not rely on memory of the spec from a previous task. Implementation tends to drift from the spec in long sessions.

## Rules

- Never assume context from a previous session is still accurate — verify by reading
- NOTES.md is the durable memory — write to it, don't rely on in-context recall
- When in doubt, read the file rather than recalling its content

## When NOT to Use

- Single-task sessions with a small, well-defined scope
- When the entire codebase fits comfortably in one context window
