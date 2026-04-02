# Template Spec v2.1

This file defines how a template artifact is structured.

## Required Frontmatter

```yaml
---
name: agent-core
type: template
version: 2.1.0
description: Copies an example agent artifact into the project.
tags: []
---
```

## Template Layout

Each template lives in its own directory:

```text
templates/<name>/
├── template.md
└── files/
```

`template.md` contains the frontmatter and a short explanation of what the template provides.

`files/` contains the actual files that will be copied into the project root.

## Copy Behavior

- Template files are copied unchanged.
- Relative paths under `files/` are preserved.
- Templates are installable artifacts and are stored in `.linxmd/templates/<name>/`.
- `linxmd new template:<name>` copies the files from an installed template into the project.
- Template files are not synced into `.github/agents/`, `.claude/`, or `.opencode/`.

## Body Structure

Recommended sections:

1. Intent
2. Included Files
3. Usage Notes

## Example

```text
templates/agent-core/
├── template.md
└── files/
    └── lib/
        └── agents/
            └── example-agent.md
```