---
name: feature-development
type: workflow
version: 2.0.0
description: >
  The primary development workflow. Takes a feature from idea to merge with
  6 human gates, full traceability, and git discipline. Replaces sdd-tdd.
agents:
  - router
  - onboarder
  - spec-writer
  - planner
  - architect
  - test-writer
  - implementer
  - consistency-guardian
  - reviewer-quality
  - docs-writer
  - changelog-writer
  - performance-monitor
skills:
  - task-management
  - trace-writing
  - conventional-commits
  - worktree-management
  - feature-branch
  - preview-delivery
  - observability
  - e2e-testing
gates: 6
quickActions:
  - id: qa-spec-gap-check
    icon: "🔬"
    label: Spec Gap Check
    prompt: Review the current specification for missing acceptance criteria, unclear scope, and unanswered questions. Propose concrete fixes as checklist items.
    trigger:
      fileMatch:
        - '^\.linxmd/specs/.*\.md$'
        - '^\.linxmd/tasks/in-progress/.*/SPEC\.md$'
      languageId: [markdown]
      contentMatch:
        - 'Acceptance Criteria|Open Questions|Out of Scope'
  - id: qa-task-split
    icon: "✂️"
    label: Split Into Task Units
    prompt: Break down the selected task plan into 1-4h implementation tasks with dependencies, risks, and a test note per task.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/in-progress/.*/TASKS\.md$'
      languageId: [markdown]
      contentMatch:
        - '## Tasks|- \[ \]'
  - id: qa-ready-for-impl
    icon: "🚀"
    label: Ready for Implementer
    prompt: Evaluate implementation readiness and return GO or NO-GO with a short reason, missing inputs, and the next concrete step.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/in-progress/.*/SPEC\.md$'
        - '^\.linxmd/tasks/in-progress/.*/TASKS\.md$'
      languageId: [markdown]
tags: [workflow, feature, tdd, development, primary]
---

# Feature Development Workflow

> The primary workflow for building new features. 6 human gates ensure quality at every stage. No shortcuts, no silent bypasses.

## Flow Diagram

```
INTAKE → ORIENT → SPEC → ★GATE 1★ → BRANCH → PLAN → ★GATE 2★
→ RED → ★GATE 3★ → GREEN → ★GATE 4★ → CONSISTENCY → QUALITY
→ PREVIEW → ★GATE 5★ → DOCS → CHANGELOG → ★GATE 6★ → MERGE
```

## Phases

### 1. INTAKE
**Agent**: `router`
**Input**: GitHub issue, idea, brainstorm output, quicknote
**Output**: Raw requirement recorded in `.linxmd/inbox/`
**Action**: Classify the request and route to this workflow.

---

### 2. ORIENT (if new session)
**Agent**: `onboarder`
**Input**: Project codebase
**Output**: Verified `PROJECT.md` (create if missing, update if stale)
**Action**: Read PROJECT.md, recent ADRs, current sprint context. Load memory from `.linxmd/memory/`.

---

### 3. SPEC
**Agent**: `spec-writer`
**Input**: Raw requirement from intake
**Output**: `.linxmd/specs/SPEC-NNN.md`
**Action**: Turn raw requirement into structured specification with:
- Problem statement
- Proposed solution
- Acceptance criteria (3–7, each testable)
- Out-of-scope definition
- Dependencies and risks

### ★ GATE 1: Spec Review ★
**Reviewer**: Human
**Validates**: Requirements correctness, scope, acceptance criteria, feasibility
**Outcome**: Approve / Reject / Request changes

---

### 4. BRANCH + WORKTREE
**Skills**: `worktree-management`, `feature-branch`
**Action**:
- Create feature branch: `feat/<short-desc>`
- Create git worktree (if supported): `.worktrees/<branch-name>`
- Create task file: `.linxmd/tasks/TASK-NNN.md` linked to spec

---

### 5. PLAN
**Agents**: `planner`, `architect`
**Input**: Approved spec
**Output**: Task breakdown with estimates, dependencies, affected files, risks
**Action**:
- Break spec into 1–4h subtasks
- Identify architectural decisions needed (write ADRs)
- Map file changes and dependencies

### ★ GATE 2: Plan Review ★
**Reviewer**: Human
**Validates**: Task ordering, estimates, risks, architectural decisions
**Outcome**: Approve / Reject / Request changes

---

### 6. RED Phase
**Agent**: `test-writer`
**Input**: Approved plan and spec
**Output**: Failing tests (unit + integration + E2E as appropriate)
**Action**:
- Write tests that fail for the RIGHT reason (missing feature)
- Cover all acceptance criteria
- Cover edge cases and error conditions
- Commit: `test(scope): add failing tests for TASK-NNN`

### ★ GATE 3: Test Coverage Review ★
**Reviewer**: Human
**Validates**: Test completeness, edge cases, missing scenarios
**Outcome**: Approve / Add more tests / Request changes

---

### 7. GREEN Phase
**Agent**: `implementer`
**Input**: Failing tests
**Output**: Passing tests with minimal, clean implementation
**Action**:
- Make failing tests pass with simplest correct code
- Add structured logging and trace spans (observability)
- Commit often: `feat(scope): description [TASK-NNN]`
- Write session trace to `.linxmd/traces/`

### ★ GATE 4: Implementation Review ★
**Reviewer**: Human
**Validates**: Code quality, correctness, observability, no scope creep
**Outcome**: Approve / Request changes

---

### 8. CONSISTENCY
**Agent**: `consistency-guardian`
**Input**: Changed files
**Output**: Consistency report
**Action**:
- Check naming conventions
- Find dead code and unused imports
- Verify pattern consistency
- Auto-fix trivial issues, report non-trivial ones

---

### 9. QUALITY
**Agent**: `reviewer-quality`
**Input**: All changes since branch creation
**Output**: Quality review report
**Action**:
- Code quality check (SOLID, DRY, complexity)
- Security check (OWASP Top 10)
- Observability check (logging, tracing, error handling)
- Performance sanity check

---

### 10. PREVIEW
**Agents**: `implementer` (build), `docs-writer` (notes)
**Skill**: `preview-delivery`
**Output**: Preview package in `.linxmd/previews/TASK-NNN/`
**Action**:
- Prepare artifact bundle (screenshots, build output, release notes)
- Optionally start live preview (dev server URL)
- Document how to test

### ★ GATE 5: Preview Acceptance ★
**Reviewer**: Human
**Validates**: Feature works correctly in real environment, UI/UX, edge cases
**Outcome**: Approve / Request changes / Reject

---

### 11. DOCS
**Agent**: `docs-writer`
**Action**: Update READMEs, API docs, PROJECT.md as needed.

---

### 12. CHANGELOG
**Agent**: `changelog-writer`
**Action**: Write changelog entry for the feature, conventional commit format.

### ★ GATE 6: Final Acceptance ★
**Reviewer**: Human
**Validates**: Preview evidence, documentation completeness, changelog accuracy
**Outcome**: Approve to merge / Request final changes

---

### 13. MERGE
**Agent**: `implementer`
**Action**:
- Merge feature branch to main/develop
- Cleanup worktree
- Close task: TASK-NNN → done
- Archive trace to memory
- Update performance baselines if applicable

## Exit Criteria

- [ ] All acceptance criteria met
- [ ] All tests passing
- [ ] All 6 gates approved
- [ ] Trace file written
- [ ] Changelog updated
- [ ] Documentation updated
- [ ] Branch merged and cleaned up
