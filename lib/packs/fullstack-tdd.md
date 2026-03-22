---
name: fullstack-tdd
type: pack
version: 1.0.0
description: Full TDD pipeline with routing and context management
tags:
  - tdd
  - workflow
  - routing
artifacts:
  - workflow:sdd-tdd
  - agent:router
  - skill:task-management
  - skill:context-management
---

# Pack: fullstack-tdd

Install a complete TDD development stack in one command.

| Artifact | Purpose |
|---|---|
| `workflow:sdd-tdd` | Spec-driven development + TDD cycle |
| `agent:router` | Route tasks to the right agent automatically |
| `skill:task-management` | Break down and track tasks |
| `skill:context-management` | Load and prune project context |
