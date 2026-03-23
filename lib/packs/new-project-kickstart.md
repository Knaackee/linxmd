---
name: new-project-kickstart
type: pack
version: 2.0.0
description: >
  Everything needed to start a new project from scratch with the full Linxmd
  agent system. Onboarding, initial structure, and transition to development.
workflow: project-start
agents:
  - start-state-creator
  - onboarder
  - planner
  - reviewer-spec
  - spec-writer
  - architect
skills:
  - start-state-creation
  - project-memory
  - task-management
  - trace-writing
  - brainstorming
  - research
tags: [pack, new-project, kickstart, onboarding, initialization]
---

# New Project Kickstart Pack

> Go from empty directory (or existing codebase) to a fully agent-ready project. PROJECT.md, memory initialized, backlog seeded, ready to build.

## What's Included

### Workflows
- **project-start** — The onboarding workflow (analyze → init → seed → verify)
- **research-spike** — For initial technology decisions

### Agents (6)
| Category | Agents |
|----------|--------|
| Core | `spec-writer`, `architect`, `planner` |
| Control | `reviewer-spec` |
| Delivery | `start-state-creator`, `onboarder` |

### Skills (6)
| Level | Skills |
|-------|--------|
| Core | `project-memory`, `task-management` |
| Governance | `trace-writing` |
| Growth | `start-state-creation`, `brainstorming`, `research` |

## Typical Flow

1. **Install this pack**
2. **Run `project-start` workflow** → generates PROJECT.md, `.linxmd/`, initial tasks
3. **Run research spikes** → for any technology decisions needed
4. **Write first spec** → define the first feature
5. **Install `fullstack-tdd` pack** → and start building

## When to Use

- Starting a brand new project
- Onboarding an existing codebase to the Linxmd system
- Re-bootstrapping a project after major restructuring

## Install

```bash
linxmd install packs/new-project-kickstart
```
