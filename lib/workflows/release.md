---
name: release
type: workflow
version: 0.3.0
description: Cut a clean, documented release in one pipeline — calculates the right SemVer bump, finalizes the CHANGELOG, updates version refs, commits, tags, and drafts release notes
deps:
  - agent:changelog-writer@>=0.3.0
  - agent:docs-writer@>=0.2.0
  - skill:project-memory@>=0.3.0
  - skill:task-management@>=0.2.0
tags:
  - release
  - semver
  - changelog
  - git
---

# Release Workflow

## Overview

A structured pipeline for producing a clean, documented release. Every step is explicit and reversible until the final git tag is pushed.

## Start Conditions

Triggered by: "release", "cut a release", "release [version]", or "prepare v[X.Y.Z]".

## Pipeline

### Step 1: Determine Version Bump

1. Read `CHANGELOG.md` → `[Unreleased]` section
2. Classify the highest-severity change present:
   - `Removed` or breaking API change → **MAJOR** bump
   - `Added` (new backward-compatible feature) → **MINOR** bump
   - `Fixed`, `Security`, `Changed` (non-breaking) → **PATCH** bump
3. Read current version from: `package.json`, `pyproject.toml`, `*.csproj`, or ask
4. Calculate new version
5. Output: `Bump: [current] → [new] ([MAJOR|MINOR|PATCH])`
6. **Wait for confirmation** before proceeding

### Step 2: Update Version References

Update the version number in all version-bearing files:
- `package.json` → `"version": "[new]"`
- `pyproject.toml` → `version = "[new]"`
- `*.csproj` → `<Version>[new]</Version>`
- Any version constant file

If no version file is found, ask the user where the version is stored.

### Step 3: Finalize CHANGELOG

Call `agent:changelog-writer`:
1. Move `[Unreleased]` content to a new `## [[new version]] — YYYY-MM-DD` section
2. Add a fresh empty `## [Unreleased]` section at the top
3. Add comparison link at the bottom if the repo supports it:
   `[new]: https://github.com/[owner]/[repo]/compare/v[prev]...v[new]`

### Step 4: Update Documentation

Call `agent:docs-writer`:
1. Verify README version badge or "current version" references are updated
2. Verify any "installation" sections reference the new version
3. Flag any API docs that describe removed or changed behavior

### Step 5: Release Commit

Produce a single release commit staged with only version files and CHANGELOG.md:

```
chore: release v[version]
```

No code changes allowed in a release commit.

### Step 6: Tag

Output the tag command for user review:

```bash
git tag -a v[version] -m "Release v[version]"
```

In **autonomous mode**: execute after confirmation.
In **guided mode**: print the command and wait.

### Step 7: Release Notes

Draft release notes from the CHANGELOG entry for this version:

```markdown
## What's New in v[version]

### Highlights
[2-3 sentences summarizing the most important changes]

### Added
- [bullet points from CHANGELOG]

### Fixed
- [bullet points from CHANGELOG]

---
**Full Changelog**: [link to CHANGELOG.md]
**Install**: [install one-liner for the package manager]
```

## Max Iterations

If the version bump determination is ambiguous (e.g., breaking change hidden in a patch PR), stop and ask the user to classify manually before proceeding.

## Execution Modes

- **autonomous**: Runs all steps; pauses only at Step 1 (version confirm) and Step 6 (tag confirm)
- **guided**: Waits after each step for "next step"

## When NOT to Use

- For pre-release / RC tags — agree on the pre-release versioning format first
- For hotfix releases on older version branches — this workflow assumes single-trunk releases
