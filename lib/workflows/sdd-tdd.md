---
name: sdd-tdd
type: workflow
version: 1.0.0
description: Spec-Driven Development mit TDD Pipeline
deps:
  - agent:test-writer@>=1.0
  - agent:implementer@>=1.0
  - agent:reviewer-spec@>=1.0
  - agent:reviewer-quality@>=1.0
  - agent:docs-writer@>=1.0
  - skill:feature@>=1.0
  - skill:task-management@>=1.0
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

1. **RED** → `test-writer` → Failing Tests schreiben
2. **GREEN** → `implementer` → Minimaler Code bis Tests grün
3. **SPEC-REVIEW** → `reviewer-spec` → Alle Kriterien erfüllt?
4. **QUALITY-REVIEW** → `reviewer-quality` → Code-Qualität + Security
5. **DOCS** → `docs-writer` → Dokumentation aktualisieren
6. **COMMIT** → Alles grün → Commit

## Execution Modes

- **autonomous**: Läuft alle Tasks durch ohne Pause. Stoppt nur bei BLOCKER.
- **guided**: Wartet nach jedem Task auf "next task". User kontrolliert das Tempo.

Default: autonomous. Override: "lets do this (guided)"

## Getting Started

1. `agentsmd init` → Projekt initialisieren
2. `agentsmd workflow install sdd-tdd` → Workflow + alle Dependencies installieren
3. `agentsmd sync` → Tool-Wrappers generieren
4. Idee in `.agentsmd/tasks/backlog/` ablegen
5. Sag "lets do this" → der Workflow startet automatisch
