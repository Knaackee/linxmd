---
name: content-review
type: workflow
version: 0.0.1
description: Content creation with review pipeline
deps:
  - skill:task-management@>=0.0.1
tags:
  - content
  - writing
  - review
---

# Content Review Workflow

## Overview

A workflow for content creation with a structured review pipeline.
Not tied to programming — works for any content that needs drafting,
fact-checking, and editing.

## Pipeline

1. **DRAFT** → Create initial content draft
2. **FACT-CHECK** → Verify all facts and claims
3. **EDIT** → Improve language, style, and structure
4. **PUBLISH** → Final version ready

## Execution Modes

- **autonomous**: Runs all stages without pausing
- **guided**: Waits after each stage for review

## Getting Started

1. `agentsmd init` → Initialize project
2. `agentsmd workflow install content-review` → Install workflow
3. `agentsmd sync` → Generate tool wrappers
4. Add content task to `.agentsmd/tasks/backlog/`
5. Start with "lets do this"

