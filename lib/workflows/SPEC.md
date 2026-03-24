# Workflow Spec v2.1

This file defines how a workflow artifact is structured.

## Required Frontmatter

```yaml
---
name: feature-development
type: workflow
version: 2.1.0
description: End-to-end development workflow with human gates.
agents: []
skills: []
gates: 0
tags: []
quickActions: []
lifecycle: {}
---
```

## Workflow-Specific Fields

- `agents`: ordered list of participating agents.
- `skills`: cross-cutting skills required by the workflow.
- `gates`: number of required human approval gates.

## Body Structure

Recommended sections:

1. Intent
2. Entry Criteria
3. Phase Plan
4. Mandatory Gates
5. Exit Criteria
6. Failure and Rollback Path

## QuickActions for Workflows

Use quick actions to start or resume workflows from context, for example:

- "Start bug-fix for current failing test"
- "Initialize project-start in uninitialized repo"

## Lifecycle for Workflows

Use lifecycle hooks to protect consistency:

- `preInstall`: verify required base artifacts are present
- `preUninstall`: block if active tasks still reference this workflow
- `postUpdate`: show migration checklist for changed gates/phases
