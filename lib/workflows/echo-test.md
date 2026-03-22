---
name: echo-test
type: workflow
version: 0.0.1
description: Test-only workflow for smoke tests — safe to delete
deps:
  - agent:echo-test@>=0.0.1
  - skill:echo-test@>=0.0.1
tags:
  - test
  - smoke
---

# echo-test workflow

Test-only workflow that depends on echo-test agent and echo-test skill.

## Steps

1. Install echo-test agent
2. Install echo-test skill
3. Done — this workflow exists purely for testing dependency resolution.

