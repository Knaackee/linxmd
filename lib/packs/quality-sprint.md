---
name: quality-sprint
type: pack
version: 1.0.0
description: Baseline quality audit, project memory, and automated routing
tags:
  - quality
  - audit
  - routing
artifacts:
  - workflow:quality-baseline
  - skill:project-memory
  - agent:router
---

# Pack: quality-sprint

Install a quality-focused sprint kit in one command.

| Artifact | Purpose |
|---|---|
| `workflow:quality-baseline` | Run a structured quality audit of the codebase |
| `skill:project-memory` | Maintain persistent cross-session project context |
| `agent:router` | Route tasks to the right agent automatically |
