# Agent Spec v2.1

This file defines how an agent artifact is structured.

## Required Frontmatter

```yaml
---
name: implementer
type: agent
version: 2.1.0
description: Minimal implementation agent for GREEN phase.
category: core|control|delivery
skills: []
tags: []
quickActions: []
lifecycle: {}
---
```

## Agent-Specific Fields

- `category`: one of `core`, `control`, `delivery`.
- `skills`: skill names this agent relies on.

## Body Structure

Recommended sections:

1. Mission
2. Responsibilities
3. Non-Responsibilities
4. Operating Sequence
5. Gating Rules
6. Output Contract

## QuickActions for Agents

Use quick actions for context shortcuts like:

- "Write tests for this file"
- "Review this change for OWASP risks"
- "Generate changelog from staged diff"

Use `fileMatch` as a required non-empty list and keep regex specific.

## Lifecycle for Agents

Use lifecycle hooks for setup and cleanup guidance, for example:

- `postInstall`: offer kickoff prompt for first use
- `preUninstall`: warn if workflows still reference this agent
