---
name: observability
type: skill
version: 0.2.0
description: Structured logging and reasoning traces for agentic pipelines
deps: []
tags:
  - logging
  - observability
  - tracing
---

# Observability Skill

Triggered by: pipeline debugging, "why did it do that", long autonomous runs, or any workflow with 3+ agent steps.

## Principles

1. **Reason before concluding**: Show reasoning steps before the conclusion, not after. The trace is evidence; the conclusion is the output.
2. **Typed outputs**: Every agent step emits a typed status (PASS / BLOCKER / WARNING / INFO) with structured data — never narrative-only.
3. **Durable log**: All run entries land in NOTES.md under `## Run Log`. This survives session restarts.

## Log Entry Format

```
[STEP] agent:{name} — {STATUS} — {one-line summary}
  input:  {brief description of what was passed in}
  output: {brief description of what was produced}
  reason: {why this status was assigned}
```

Example:
```
[STEP] agent:reviewer-spec — BLOCKER — Criterion "user can export CSV" not covered
  input:  Task 3 implementation + SPEC.md
  output: BLOCKER — missing test for export edge case
  reason: No test asserts on empty export; criterion 3 only implicitly covered
```

## Pipeline Summary

After each completed pipeline run, append a summary block to NOTES.md:

```markdown
## Run Summary — [workflow name] — [date]
| Step | Agent | Status | Summary |
|---|---|---|---|
| 1 | test-writer | PASS | 4 tests written |
| 2 | implementer | PASS | 3 files changed |
| 3 | reviewer-spec | BLOCKER | criterion 3 missing |
```

## Rules

- Log immediately when a step completes — do not batch logs
- BLOCKER entries must include `reason:` — a BLOCKER without reasoning cannot be acted on
- If a step is skipped (fast-path, "Docs: none"), log it: `[SKIP] agent:{name} — reason`

## When NOT to Use

- Lightweight single-agent tasks where log overhead exceeds benefit
- Interactive, conversational tasks with no pipeline structure
