---
name: content-review
type: workflow
version: 0.2.0
description: Content creation with review pipeline
deps:
  - agent:drafter@>=0.2.0
  - agent:fact-checker@>=0.2.0
  - agent:editor@>=0.2.0
  - skill:task-management@>=0.2.0
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

1. **DRAFT** → `drafter` creates initial draft from backlog item
2. **FACT-CHECK** → `fact-checker` verifies claims, links, and numbers
3. **TONE-REVIEW** → Inline check (no separate agent): Is the tone appropriate for the target audience? Is the voice consistent throughout? Are any statements likely to alienate or confuse the reader?
4. **EDIT** → `editor` improves language, flow, and structure
5. **METADATA** → For web-published content only: verify title, description, tags, and canonical URL are complete
6. **PUBLISH** → Final version prepared for release

## Stage Contracts

- DRAFT output:
  - Title
  - Audience
  - Core message
  - First full draft
- FACT-CHECK output:
  - Verified claims
  - Unverified claims
  - Required corrections
- EDIT output:
  - Revised content
  - Changelog summary
  - Open issues (if any)

## Execution Modes

- **autonomous**: Runs all stages without pausing
- **guided**: Waits after each stage for review

Guided prompts:
- "Draft complete. Say 'next stage' for fact-check."
- "Fact-check complete. Say 'next stage' for tone-review."
- "Tone-review complete. Say 'next stage' for edit."
- "Edit complete. Say 'next stage' for metadata check (or 'publish' to skip)."
- "Metadata complete. Say 'publish' to finalize."

## When NOT to Use

- For short internal notes or comments with no review requirement — write directly
- For code documentation → use `agent:docs-writer`

## Getting Started

1. `linxmd init` → Initialize project
2. `linxmd add workflow:content-review --yes` → Install workflow
3. `linxmd sync` → Generate tool wrappers
4. Add content task to `.linxmd/tasks/backlog/`
5. Start with "begin content review" or "write content: [topic]"

