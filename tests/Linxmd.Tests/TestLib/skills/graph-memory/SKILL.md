---
name: graph-memory
type: skill
version: 1.0.0
description: Stable test graph memory skill.
quickActions:
  - id: load-memory
    label: Load memory
    prompt: Load the latest graph memory context.
    trigger:
      fileMatch: ["**/*.md"]
tags: [test, graph, memory]
---

# Graph Memory Test Skill