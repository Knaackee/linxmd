---
name: sdd-tdd
type: workflow
version: 0.0.1
description: Spec-Driven Development with TDD pipeline
deps:
  - agent:test-writer@>=0.0.1
  - agent:implementer@>=0.0.1
  - agent:reviewer-spec@>=0.0.1
  - agent:reviewer-quality@>=0.0.1
  - agent:docs-writer@>=0.0.1
  - skill:feature@>=0.0.1
  - skill:task-management@>=0.0.1
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

## Pipeline

For each task in TASKS.md:

1. **RED** → `test-writer` → Write failing tests
2. **GREEN** → `implementer` → Minimal code until tests pass
3. **SPEC-REVIEW** → `reviewer-spec` → All criteria met?
4. **QUALITY-REVIEW** → `reviewer-quality` → Code quality + security
5. **DOCS** → `docs-writer` → Update documentation
6. **COMMIT** → All green → Commit

## Execution Modes

- **autonomous**: Runs all tasks without pausing. Only stops on BLOCKER.
- **guided**: Waits after each task for "next task". User controls the pace.

Default: autonomous. Override: "lets do this (guided)"

## Getting Started

1. `agentsmd init` → Initialize project
2. `agentsmd workflow install sdd-tdd` → Install workflow + all dependencies
3. `agentsmd sync` → Generate tool wrappers
4. Add an idea to `.agentsmd/tasks/backlog/`
5. Say "lets do this" → the workflow starts automatically

