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

### Mandatory Continue Memory Skills

For all Continue-oriented agents, `skills` should include both:

- `graph`
- `graph-memory`

This is required to enforce consistent graph-memory usage across all agents.

## Body Structure

Recommended sections:

1. Mission
2. Responsibilities
3. Non-Responsibilities
4. Operating Sequence (Init -> Execute -> Post)
5. Gating Rules
6. Output Contract

## Operating Sequence Contract (Required)

Every agent definition should follow the same three-phase sequence:

1. Init
2. Execute
3. Post

### Init Phase (Required)

- Retrieve relevant knowledge from graph-memory first (via `graph` + `graph-memory` skills)
- Fall back to project/file context only if graph-memory interaction fails
- Define scope and constraints for this run
- Declare intended work focus

### Execute Phase (Required)

- Perform the agent's core task
- Keep temporary notes separate from durable knowledge

### Post Phase (Required)

- Extract durable knowledge deltas (new, updated, invalidated)
- Persist knowledge using configured memory skills
- Provide handoff to next agent/workflow step

This sequence should be explicit in each agent artifact's Operating Sequence section.

## Output Contract (Required)

Each agent run should emit these blocks:

1. Context Snapshot
2. Work Result
3. Knowledge Delta
4. Handoff

### Required Output Artifacts

Each agent run should produce both artifacts:

1. Human-readable markdown output (project memory)
2. Machine-readable delta JSON (graph sync input)

Recommended paths:

- Markdown: `.linxmd/outputs/<agent>/<YYYY-MM-DD>/<run-id>.md`
- Delta: `.linxmd/outputs/<agent>/<YYYY-MM-DD>/<run-id>.delta.json`

This dual format preserves auditability while keeping graph updates token-efficient.

### Knowledge Delta Minimum Fields

- `scope_id`
- `new_claims`
- `updated_claims`
- `invalidated_claims`
- `relations_added`
- `confidence`
- `sources`

### Document Identity and Path Fields (Required)

To support folder renames and stable long-term navigation, every delta should include:

- `project_id` (stable, rename-safe)
- `project_path_current` (current absolute or workspace-relative root)
- `project_path_aliases` (optional previous paths)
- `doc_id` (stable document id)
- `doc_rel_path` (path relative to project root)
- `content_hash` (content fingerprint)

Do not use `project_path_current` as primary identity. Identity should be `project_id` + `doc_id`.

### Source-Of-Truth and Transfer Contract (Required)

These fields are graph-memory identity fields and should be treated as graph-owned metadata:

- `project_id`
- `project_path_current`
- `project_path_aliases`
- `doc_id`
- `doc_rel_path`
- `content_hash`

Agent obligations per run:

1. Read current identity metadata from graph-memory first.
2. Write updated identity metadata to graph-memory via delta.
3. Mirror the same identity metadata into markdown frontmatter for auditability.

### Markdown vs Graph Boundary (Required)

Store in markdown:

- Full narrative document content
- Human-readable rationale and long-form explanations
- Review comments and contextual notes

Store in graph-memory:

- Stable identifiers and path metadata
- Summaries, claims, decisions, learnings, and relations
- Temporal validity (`valid_from`, `valid_until`) and provenance

Never store full markdown document bodies in graph-memory.

### Markdown Header Metadata (Recommended)

Top-level frontmatter in output markdown should include:

- `run_id`
- `agent`
- `scope_id`
- `project_id`
- `project_path_current`
- `doc_id`
- `doc_rel_path`
- `started_at`
- `finished_at`
- `status`

## QuickActions for Agents

Use quick actions for context shortcuts like:

- "Write tests for this file"
- "Review this change for OWASP risks"
- "Generate changelog from staged diff"

Use one of these trigger modes:

- File-scoped quick action: required non-empty `fileMatch` list
- Chat quick action: set `chat: true` in `trigger`

## Lifecycle for Agents

Use lifecycle hooks for setup and cleanup guidance, for example:

- `postInstall`: offer kickoff prompt for first use
- `preUninstall`: warn if workflows still reference this agent
