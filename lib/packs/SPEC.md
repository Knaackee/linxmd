# Pack Spec v2.1

This file defines how a pack artifact is structured.

## Required Frontmatter

```yaml
---
name: coninue
type: pack
version: 2.1.0
description: Bootstrap pack for Continue-first development.
deps: []
tags: []
---
```

## Pack-Specific Fields

Recommended frontmatter fields for packs:

- `workflow`: primary workflow name included by this pack
- `agents`: ordered list of included agents
- `skills`: list of included skills

Example:

```yaml
workflow: feature-development
agents: [product-manager, router, spec-writer, planner]
skills: [graph, graph-memory, task-management, trace-writing]
```

## Body Structure

Recommended sections:

1. Purpose
2. Included Artifacts
3. Entry Point
4. Expansion Path
5. Exit Criteria

## Pack Constraints

- A pack is a collection artifact only.
- Packs should not define behavior logic beyond composition.
- Packs should not define quick actions.
- Packs should not define lifecycle hooks.

## QuickActions and Lifecycle

For packs, both are intentionally omitted:

- Do not include `quickActions` in pack frontmatter.
- Do not include `lifecycle` in pack frontmatter.

Operational behavior belongs to workflows, agents, and skills.
