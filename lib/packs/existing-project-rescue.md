---
name: existing-project-rescue
type: pack
version: 2.0.0
description: >
  Rescue bundle for neglected or inherited codebases. Full audit, cleanup plan,
  consistency sweep, and documentation recovery.
workflow: quality-baseline
agents:
  - start-state-creator
  - onboarder
  - consistency-guardian
  - reviewer-quality
  - performance-monitor
  - fact-checker
  - docs-writer
  - planner
  - implementer
skills:
  - start-state-creation
  - project-memory
  - consistency-check
  - refactoring
  - observability
  - performance-profiling
  - task-management
  - trace-writing
  - conventional-commits
tags: [pack, rescue, legacy, cleanup, audit, onboarding]
---

# Existing Project Rescue Pack

> For inherited, neglected, or undocumented codebases. Analyze → Audit → Plan → Fix. Bring any project up to quality standards.

## What's Included

### Workflows
- **project-start** — Bootstrap the agent system
- **quality-baseline** — Full quality audit
- **consistency-sprint** — Codebase cleanup

### Agents (9)
| Category | Agents |
|----------|--------|
| Core | `planner`, `implementer` |
| Control | `consistency-guardian`, `reviewer-quality`, `fact-checker` |
| Delivery | `start-state-creator`, `onboarder`, `performance-monitor`, `docs-writer` |

### Skills (9)
| Level | Skills |
|-------|--------|
| Core | `project-memory`, `refactoring`, `task-management` |
| Governance | `consistency-check`, `observability`, `performance-profiling`, `trace-writing`, `conventional-commits` |
| Growth | `start-state-creation` |

## Rescue Sequence

1. **Phase 1: Understand** (Week 1)
   - Run `project-start` workflow → generate PROJECT.md
   - Understand what exists, what's broken, what's missing

2. **Phase 2: Audit** (Week 1–2)
   - Run `quality-baseline` workflow → full quality report
   - Identify highest-impact issues
   - Establish performance baselines

3. **Phase 3: Plan** (Week 2)
   - Create prioritized improvement tasks
   - Group into phases: critical → important → nice-to-have

4. **Phase 4: Fix** (Week 2+)
   - Run `consistency-sprint` → clean up dead code, naming, patterns
   - Fix critical issues first
   - Add missing tests for untested code
   - Add missing documentation

## When to Use

- Inherited codebase with no documentation
- Legacy project being modernized
- Project returning from long maintenance hiatus
- "Nobody knows how this works" situations

## Install

```bash
linxmd install packs/existing-project-rescue
```
