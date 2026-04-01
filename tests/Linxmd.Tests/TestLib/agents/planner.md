---
name: planner
type: agent
version: 1.0.0
description: Stable test planner agent.
deps:
  - skill:graph
  - skill:graph-memory
  - skill:task-management
  - skill:context-management
  - skill:trace-writing
quickActions:
  - id: plan-task
    label: Plan task
    prompt: Create a plan for the selected task.
    trigger:
      fileMatch: ["**/*.md"]
tags: [test, planner]
---

# Planner Test Agent