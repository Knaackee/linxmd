---
name: artifact-factory
type: workflow
version: 0.1.0
description: Create and refine new agents, skills, and workflows as reusable building blocks
deps:
  - skill:task-management@>=0.1.0
  - skill:preview-delivery@>=0.1.0
tags:
  - factory
  - authoring
  - workflow
---

# Artifact Factory Workflow

A single workflow for generating new workflow assets.

## Pipeline

1. Define target artifact
- Choose one: agent, skill, workflow
- Define purpose, inputs, outputs, boundaries

2. Draft artifact
- Produce frontmatter and body
- Ensure dependency references use `type:name` format

3. Validate autonomy
- Check that artifact can be used as a standalone building block
- Remove hidden coupling and context assumptions

4. Preview and feedback loop
- Use `preview-delivery` skill to share preview
- Collect and integrate feedback

5. Publish
- Add entry to `lib/index.json`
- Verify install, remove, and sync behavior
