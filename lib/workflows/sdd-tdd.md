---
name: sdd-tdd
type: workflow
version: 0.1.0
description: Spec-Driven Development with TDD pipeline
deps:
  - agent:test-writer@>=0.1.0
  - agent:implementer@>=0.1.0
  - agent:reviewer-spec@>=0.1.0
  - agent:reviewer-quality@>=0.1.0
  - agent:docs-writer@>=0.1.0
  - skill:task-management@>=0.1.0
  - skill:preview-delivery@>=0.1.0
tags:
  - development
  - tdd
  - sdd
---

# SDD+TDD Workflow

## Overview

Spec-Driven Development defines WHAT is built (SPEC.md = source of truth).
Test-Driven Development defines HOW it is built (Red → Green → Refactor).

Every Acceptance Criterion in SPEC.md becomes a failing test first.
No implementation exists before its test exists and fails.

## Start Conditions

Triggered by: "lets do this", "start", "begin", or naming a backlog item.
Append "(guided)" for guided mode.

1. Find matching backlog item in `.linxmd/tasks/backlog/`
2. Create `.linxmd/tasks/in-progress/[name]/`
3. Move source item to `backlog-original.md`
4. Draft SPEC.md and wait for approval
5. Create TASKS.md and NOTES.md

## Pipeline

For each task in TASKS.md:

1. **RED** → `test-writer` → Write failing tests
2. **GREEN** → `implementer` → Minimal code until tests pass
3. **SPEC-REVIEW** → `reviewer-spec` → All criteria met?
4. **QUALITY-REVIEW** → `reviewer-quality` → Code quality + security
5. **DOCS** → `docs-writer` → Update documentation
6. **COMMIT** → All green → Commit

## Stop Conditions

- Implementer reports BLOCKER
- Reviewer-spec returns BLOCKER
- Reviewer-quality returns BLOCKER

In all three cases: stop, report, and wait for user decision.

## Execution Modes

- **autonomous**: Runs all tasks without pausing. Only stops on BLOCKER.
- **guided**: Waits after each task for "next task". User controls the pace.

Default: autonomous. Override: "lets do this (guided)"

Guided mode behavior:
- Start with: "[N] tasks ready. Say 'next task' to begin."
- After each successful task: "Task [N] done. Say 'next task' to continue."

Autonomous mode behavior:
- Start with: "Running [N] tasks autonomously. I will only stop on BLOCKER."
- Continue until all tasks are done or blocked

## Finish

After all tasks complete:
1. Run final checks
2. Optionally run preview-delivery for review feedback loops
3. Open PR with SPEC summary

## Getting Started

1. `linxmd init` → Initialize project
2. `linxmd add workflow:sdd-tdd --yes` → Install workflow + all dependencies
3. `linxmd sync` → Generate tool wrappers
4. Add an idea to `.linxmd/tasks/backlog/`
5. Say "lets do this" → the workflow starts automatically

