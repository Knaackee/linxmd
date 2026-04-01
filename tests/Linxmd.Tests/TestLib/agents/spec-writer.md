---
name: spec-writer
type: agent
version: 1.0.0
description: Stable test spec writer agent.
deps:
  - skill:graph
  - skill:graph-memory
  - skill:task-management
  - skill:context-management
  - skill:trace-writing
quickActions:
  - id: write-spec
    label: Write spec
    prompt: Write a specification for the selected request.
    trigger:
      fileMatch: ["**/*.md"]
tags: [test, spec]
---

# Spec Writer Test Agent