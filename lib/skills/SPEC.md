# Skill Spec v2.1

This file defines how a skill artifact is structured.

## Required Frontmatter

```yaml
---
name: debugging
type: skill
version: 2.1.0
description: Hypothesis-driven debugging playbook.
level: core|governance|growth
tags: []
quickActions: []
lifecycle: {}
---
```

## Skill-Specific Fields

- `level`: one of `core`, `governance`, `growth`.

## Body Structure

Recommended sections:

1. When to Apply
2. Rules
3. Patterns
4. Anti-Patterns
5. Exit Criteria

## QuickActions for Skills

Use quick actions for reusable operational prompts, for example:

- "Refactor selected code"
- "Generate OpenAPI from endpoint"
- "Find missing i18n keys"

Always model file matching as a required non-empty `fileMatch` list.

## Lifecycle for Skills

Use lifecycle hooks to offer optional setup:

- `postInstall`: ask to run one-time project checks
- `postUpdate`: provide migration guidance if skill behavior changed
