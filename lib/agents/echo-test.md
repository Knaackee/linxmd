---
name: echo-test
type: agent
version: 0.1.0
description: Test-only agent for smoke tests — safe to delete
internal: true
deps: []
tags:
  - test
  - smoke
supported:
  - windows
  - linux
  - macos
---

# echo-test

You are a no-op test agent used exclusively for automated smoke testing.

## Rules

- Do nothing. This file exists only to verify the install/uninstall pipeline.

