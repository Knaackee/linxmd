---
name: preview-delivery
type: skill
version: 0.1.0
description: Build previews, share links or binaries, collect feedback, and iterate
deps: []
tags:
  - preview
  - feedback
  - delivery
---

# Preview Delivery Skill

Use this skill when you need a fast review loop for websites or binaries.

## Flow

1. Build preview artifact
- Web: build and run preview server
- Binary: build release binary for target platform

2. Publish preview
- Web option: expose via secure tunnel or temporary hosting
- Binary option: attach artifact to release, issue, or direct file transfer channel

3. Collect feedback
- Ask for structured feedback: what works, what breaks, what is missing
- Capture feedback in `.linxmd/tasks/in-progress/<feature>/NOTES.md`

Feedback template:
- Works: [list]
- Broken: [list]
- Missing: [list]
- Priority: [high|medium|low]

4. Iterate
- Prioritize feedback by severity
- Apply fixes
- Re-publish preview with changelog note

5. Stop rule
- Wait explicitly for reviewer confirmation before final release.
