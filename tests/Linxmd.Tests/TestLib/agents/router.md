---
name: router
type: agent
version: 1.0.0
description: Stable test router agent.
deps:
  - skill:graph
  - skill:graph-memory
  - skill:task-management
  - skill:context-management
  - skill:trace-writing
quickActions:
  - id: route-request
    label: Route request
    prompt: Route the current request.
    trigger:
      fileMatch: ["**/*"]
tags: [test, router]
---

# Router Test Agent