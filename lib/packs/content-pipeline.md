---
name: content-pipeline
type: pack
version: 1.0.0
description: Complete draft → fact-check → edit → review stack
tags:
  - content
  - writing
  - review
artifacts:
  - agent:drafter
  - agent:fact-checker
  - agent:editor
  - workflow:content-review
---

# Pack: content-pipeline

Install a complete content production pipeline in one command.

| Artifact | Purpose |
|---|---|
| `agent:drafter` | Draft documents and structured content |
| `agent:fact-checker` | Verify claims and flag inconsistencies |
| `agent:editor` | Polish prose and enforce style |
| `workflow:content-review` | Orchestrated draft → fact-check → edit loop |
