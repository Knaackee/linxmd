---
name: content-pipeline
type: pack
version: 2.0.0
description: >
  Content creation bundle for articles, docs, and blog posts. Draft, edit,
  fact-check, and publish with structured review.
workflow: content-review
agents:
  - router
  - drafter
  - editor
  - fact-checker
skills:
  - research
  - trace-writing
  - text-translator
tags: [pack, content, writing, documentation, publishing]
---

# Content Pipeline Pack

> Structured content creation from draft to publish. Write, edit, verify, and ship quality content.

## What's Included

### Workflow
- **content-review** — Brief → Draft → Edit → Fact-check → Publish (3 gates)

### Agents (4)
| Category | Agents |
|----------|--------|
| Core | `drafter`, `editor` |
| Control | `fact-checker` |
| Delivery | `router` |

### Skills (3)
| Level | Skills |
|-------|--------|
| Core | `text-translator` |
| Governance | `trace-writing` |
| Growth | `research` |

## When to Use

- Writing technical blog posts
- Creating documentation pages
- Producing release announcements
- Writing proposals or RFCs

## Install

```bash
linxmd install packs/content-pipeline
```
