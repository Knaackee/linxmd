---
name: quality-sprint
type: pack
version: 2.0.0
description: >
  Codebase cleanup and quality improvement bundle. Consistency sweep, quality
  audit, performance baselines, and improvement planning.
workflow: quality-baseline
agents:
  - consistency-guardian
  - reviewer-quality
  - performance-monitor
  - fact-checker
  - docs-writer
  - planner
  - implementer
skills:
  - consistency-check
  - refactoring
  - observability
  - performance-profiling
  - trace-writing
  - conventional-commits
  - task-management
tags: [pack, quality, cleanup, audit, consistency]
---

# Quality Sprint Pack

> Dedicated bundle for codebase cleanup and quality improvement. Run periodically to keep code health high.

## What's Included

### Workflows
- **quality-baseline** — Full project audit and improvement planning
- **consistency-sprint** — Focused codebase cleanup

### Agents (7)
| Category | Agents |
|----------|--------|
| Control | `consistency-guardian`, `reviewer-quality`, `fact-checker` |
| Delivery | `performance-monitor`, `docs-writer`, `planner`, `implementer` |

### Skills (7)
| Level | Skills |
|-------|--------|
| Core | `refactoring`, `task-management` |
| Governance | `consistency-check`, `observability`, `performance-profiling`, `trace-writing`, `conventional-commits` |

## When to Use

- After a major feature sprint (clean up accumulated debt)
- Before a release (ensure quality bar is met)
- Quarterly maintenance (keep baselines current)
- New team member onboarding (understand current state)

## Install

```bash
linxmd install packs/quality-sprint
```
