---
name: project-start
type: workflow
version: 2.0.0
description: >
  Onboards an existing codebase into the Linxmd agent system. Generates PROJECT.md,
  initializes memory, and seeds the task backlog.
agents:
  - start-state-creator
  - onboarder
  - planner
  - reviewer-spec
skills:
  - start-state-creation
  - project-memory
  - task-management
  - trace-writing
gates: 3
tags: [workflow, onboarding, initialization, project-start, global-setup]
---

# Project Start Workflow

> Take any existing codebase from zero to agent-ready. Analyze, orient, seed, verify.

## Flow Diagram

```
GLOBAL ENV → ★GATE 0★ → ANALYZE → ★GATE 1★ → INIT MEMORY → SEED TASKS → VERIFY → ★GATE 2★
```

## Phases

### 0. GLOBAL ENVIRONMENT CHECK
**Agent**: `onboarder`
**Skill**: `user-profile`
**Action**:
- Check if `~/.linxmd/` exists → create if missing
- Check if `~/.linxmd/user-profile.md` exists
  - If missing → run user-profile interview (max 3 questions at a time)
  - If exists → read and confirm still current (suggest refresh if >3 months old)
- Ensure `~/.linxmd/global/` directory exists

### ★ GATE 0: Human Confirms Profile ★
**Reviewer**: Human
**Validates**:
- Profile information is correct
- Preferences match current expectations
**Outcome**: Approve / Adjust
**Skip condition**: If `user-profile.md` already existed and is <3 months old, this gate is auto-passed.

---

### 1. ANALYZE
**Agent**: `start-state-creator`
**Action**:
- Scan codebase structure
- Detect tech stack from config files (verified, not guessed)
- Read existing documentation (README, CHANGELOG, docs/)
- Identify TODO/FIXME/HACK comments
- Generate draft `PROJECT.md`

### ★ GATE 1: Human Reviews PROJECT.md ★
**Reviewer**: Human
**Validates**:
- Tech stack is correct
- Directory descriptions are accurate
- Architecture observations make sense
- No critical information is missing
**Outcome**: Approve / Enrich / Correct

---

### 2. INIT MEMORY
**Skill**: `project-memory`
**Action**:
- Create `.linxmd/` directory structure:
  ```
  .linxmd/
  ├── memory/
  │   ├── decisions/
  │   └── learnings/
  ├── tasks/
  ├── traces/
  ├── specs/
  └── inbox/
  ```
- Import existing ADRs if found
- Set baseline for consistency-guardian

---

### 3. SEED TASKS
**Agent**: `planner`
**Action**:
- Create initial tasks from:
  - TODO/FIXME comments found during analysis
  - Missing test coverage
  - Missing documentation
  - Security concerns (outdated deps, hardcoded values)
  - Build/CI gaps
- Each task gets proper v2 frontmatter

---

### 4. VERIFY
**Agent**: `reviewer-spec`
**Action**:
- Cross-reference PROJECT.md against actual code
- Verify task accuracy
- Flag discrepancies

### ★ GATE 2: Human Final Approval ★
**Reviewer**: Human
**Validates**: Everything generated is accurate and useful
**Outcome**: Approve / Adjust

## Exit Criteria

- [ ] PROJECT.md exists and is accurate
- [ ] `.linxmd/` directory structure initialized
- [ ] Initial task backlog seeded
- [ ] Everything approved by human
