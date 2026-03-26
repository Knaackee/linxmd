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
3. Phase Plan (Init -> Execute -> Post)
4. Mandatory Gates
5. Exit Criteria
6. Failure and Rollback Path

## Phase Contract (Required)

Every workflow should enforce a shared three-phase lifecycle:

1. Init
2. Execute
3. Post

### Init Phase (Required)

- Load shared context and relevant prior knowledge
- Validate scope, constraints, and gate prerequisites
- Confirm which agents are active in this run

### Execute Phase (Required)

- Run the workflow-specific phase plan
- Keep gate checks explicit at phase boundaries

### Post Phase (Required)

- Consolidate durable knowledge deltas from participating agents
- Persist knowledge via memory skills
- Produce handoff/resume state for continuation

This lifecycle should be represented in each workflow's Phase Plan section.

## Workflow Output Contract (Required)

Each workflow run should emit:

1. Context Snapshot
2. Workflow Result
3. Consolidated Knowledge Delta
4. Next-Step Handoff

### Required Output Artifacts

Each workflow run should produce both artifacts:

1. Consolidated markdown report (human-readable)
2. Consolidated delta JSON (graph sync input)

Recommended paths:

- Markdown: `.linxmd/outputs/workflows/<workflow>/<YYYY-MM-DD>/<run-id>.md`
- Delta: `.linxmd/outputs/workflows/<workflow>/<YYYY-MM-DD>/<run-id>.delta.json`

Workflow deltas should merge agent deltas without losing source provenance.

### Consolidated Knowledge Delta Minimum Fields

- `scope_id`
- `new_claims`
- `updated_claims`
- `invalidated_claims`
- `relations_added`
- `confidence`
- `sources`

### Document Identity and Path Fields (Required)

To preserve global navigation across folder renames, consolidated deltas should include:

- `project_id`
- `project_path_current`
- `project_path_aliases` (optional)
- `doc_ids` (all participating documents)
- `doc_rel_paths` (all participating relative paths)
- `content_hashes` (per document)

Do not use mutable paths as primary identifiers.

### Source-Of-Truth and Transfer Contract (Required)

Workflow-level consolidated delta is the transport artifact that moves durable metadata from markdown outputs into graph-memory.

Required transfer direction per workflow run:

1. Agents produce markdown outputs and agent delta files.
2. Workflow consolidates deltas into one workflow delta.
3. Consolidated workflow delta is written to graph-memory.
4. Markdown remains the full narrative source; graph-memory remains the structured index/relationship layer.

Do not transfer full markdown bodies to graph-memory.

## QuickActions for Workflows

Use quick actions to start or resume workflows from context, for example:

- "Start bug-fix for current failing test"
- "Initialize project-start in uninitialized repo"

Use one of these trigger modes:

- File-scoped quick action: required non-empty `fileMatch` list
- Chat quick action: set `chat: true` in `trigger`

## Lifecycle for Workflows

Use lifecycle hooks to protect consistency:

- `preInstall`: verify required base artifacts are present
- `preUninstall`: block if active tasks still reference this workflow
- `postUpdate`: show migration checklist for changed gates/phases
