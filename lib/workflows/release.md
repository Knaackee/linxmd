---
name: release
type: workflow
version: 2.0.0
description: >
  Prepare and ship a release. Version bump, changelog finalization, build,
  tag, and publish with human sign-off.
agents:
  - changelog-writer
  - docs-writer
  - reviewer-quality
  - performance-monitor
  - implementer
skills:
  - conventional-commits
  - preview-delivery
  - trace-writing
gates: 2
quickActions:
  - id: qa-release-readiness
    label: Release Readiness
    prompt: Evaluate release readiness and list blockers, open gates, unresolved risks, and required sign-offs.
    trigger:
      fileMatch:
        - '^PROJECT\.md$'
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
  - id: qa-release-notes-draft
    label: Draft Release Notes
    prompt: Draft release notes grouped by Added, Changed, Fixed, and Removed, based on completed tasks and notable technical impact.
    trigger:
      fileMatch:
        - '^CHANGELOG\.md$'
      languageId: [markdown]
tags: [workflow, release, publish, deploy, versioning]
---

# Release Workflow

> Ship with confidence. Every release is versioned, documented, tested, and approved.

## Flow Diagram

```
PREPARE → QUALITY CHECK → BENCHMARK → ★GATE 1★ → BUILD → TAG
→ CHANGELOG → DOCS → ★GATE 2★ → PUBLISH
```

## Phases

### 1. PREPARE
**Agent**: `changelog-writer`
**Action**:
- Review all entries in `[Unreleased]` section of CHANGELOG.md
- Determine version bump (major/minor/patch) based on changes:
  - Breaking changes → major
  - New features → minor
  - Bug fixes only → patch
- Move entries to new version section: `## [X.Y.Z] - YYYY-MM-DD`

---

### 2. QUALITY CHECK
**Agent**: `reviewer-quality`
**Action**:
- All tests pass
- No critical or major issues in last review
- No known regressions
- Security audit clean

---

### 3. BENCHMARK
**Agent**: `performance-monitor`
**Action**:
- Run full benchmark suite
- Compare against last release baseline
- Flag any regressions

### ★ GATE 1: Release Readiness ★
**Reviewer**: Human
**Validates**: Quality check clean, benchmarks acceptable, changelog accurate
**Outcome**: Ready / Fix issues first / Delay release

---

### 4. BUILD
**Agent**: `implementer`
**Action**:
- Update version in project config files
- Run full test suite one more time
- Build release artifacts
- Commit: `chore(release): bump version to X.Y.Z`

---

### 5. TAG
**Agent**: `implementer`
**Action**: `git tag vX.Y.Z`

---

### 6. CHANGELOG + DOCS
**Agents**: `changelog-writer`, `docs-writer`
**Action**:
- Finalize changelog with version number and date
- Update any version references in documentation
- Write release summary

### ★ GATE 2: Publish Approval ★
**Reviewer**: Human
**Validates**: Everything looks correct, ready to make public
**Outcome**: Publish / Hold

---

### 7. PUBLISH
**Agent**: `implementer`
**Action**:
- Push tag
- Publish to registry (npm, NuGet, crates.io, etc.)
- Create GitHub release with notes
- Update performance baselines for next cycle

## Exit Criteria

- [ ] Version bumped
- [ ] All tests and benchmarks pass
- [ ] Changelog finalized
- [ ] Tag created
- [ ] Published to registry
- [ ] GitHub release created
