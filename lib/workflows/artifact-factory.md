---
name: artifact-factory
type: workflow
version: 0.2.0
description: Create and refine new agents, skills, and workflows as reusable building blocks
deps:
  - skill:task-management@>=0.2.0
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

4. Validate quality (checklist — all must pass before continuing)
- [ ] Can it be used standalone without reading other artifacts?
- [ ] Are triggers or invocation conditions unambiguous?
- [ ] Does it duplicate functionality already in another artifact?
- [ ] Is the output contract (what it produces) explicitly defined?
- [ ] Does it have a `## When NOT to Use` section?

5. Smoke test
- Provide an echo-test fixture or minimal example that exercises the artifact end-to-end
- Every shipped artifact must have at least one verifiable smoke test

6. Preview and feedback loop
- Use `preview-delivery` skill to share preview
- Collect and integrate feedback

7. Publish
- Add entry to `lib/index.json`
- Verify install, remove, and sync behavior

## When NOT to Use

- For one-off prompts with no reuse potential — write a `.prompt.md` file instead
- For workspace-specific instructions — use `copilot-instructions.md` instead
