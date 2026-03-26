---
name: bug-fix
type: workflow
version: 2.0.0
description: >
  Reproduce-first bug fixing workflow. Write a failing test before investigating,
  fix minimally, add regression tests.
agents:
  - router
  - implementer
  - test-writer
  - consistency-guardian
  - reviewer-quality
  - docs-writer
  - changelog-writer
skills:
  - debugging
  - trace-writing
  - conventional-commits
  - task-management
gates: 3
quickActions:
  - id: qa-repro-template
    icon: "🐛"
    label: Create Repro Steps
    prompt: Create a reproducible bug report template with environment, exact steps, expected behavior, actual behavior, and affected scope.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
      contentMatch:
        - 'bug|error|stack trace|reproduce'
  - id: qa-regression-tests
    icon: "🛡️"
    label: Regression Test Scope
    prompt: Propose a regression test scope from the bug context, prioritize by risk, and include at least one failing-first scenario.
    trigger:
      fileMatch:
        - '^\.linxmd/specs/.*\.md$'
        - '^\.linxmd/tasks/in-progress/.*/SPEC\.md$'
      languageId: [markdown]
tags: [workflow, bug-fix, debugging, tdd]
---

# Bug Fix Workflow

> Reproduce first, investigate second, fix minimally. Every bug becomes a permanent regression test.

## Flow Diagram

```
TRIAGE → REPRODUCE → ★GATE 1★ → INVESTIGATE → FIX → REGRESSION
→ CONSISTENCY → QUALITY → ★GATE 2★ → DOCS+CHANGELOG → ★GATE 3★ → MERGE
```

## Phases

### 1. TRIAGE
**Agent**: `router`
**Action**:
- Classify severity: critical / major / minor / cosmetic
- Link to PROJECT.md context
- Create task with bug type and priority

Priority mapping:
| Severity | Priority | Response |
|----------|----------|----------|
| Critical | P0 | Immediate — blocks users |
| Major | P1 | This sprint — significant impact |
| Minor | P2 | Next sprint — inconvenience |
| Cosmetic | P3 | Backlog — visual only |

---

### 2. REPRODUCE
**Agent**: `implementer` + `debugging` skill
**Action**:
- Write a failing test that **reproduces the exact bug**
- The test must fail for the right reason (the actual bug)
- This test IS the acceptance criterion for the fix
- Commit: `test(scope): reproduce issue #NNN`

### ★ GATE 1: Reproduction Confirmed ★
**Reviewer**: Human
**Validates**: Test reproduces the actual bug, not a different issue
**Outcome**: Confirm / Adjust reproduction

---

### 3. INVESTIGATE
**Agent**: `implementer` + `debugging` skill
**Action**:
- Use binary search to find root cause (see `debugging` skill)
- Document root cause in trace
- Identify impact scope — what else might be affected?

---

### 4. FIX
**Agent**: `implementer`
**Action**:
- Minimal change that makes the reproduction test pass
- Fix the root cause, not the symptom
- Commit: `fix(scope): resolve issue #NNN`
- No scope creep — related issues get their own tasks

---

### 5. REGRESSION
**Agent**: `test-writer`
**Action**:
- Add additional regression tests for:
  - The specific failure mode
  - Related edge cases
  - Similar patterns in the codebase

---

### 6. CONSISTENCY + QUALITY
**Agents**: `consistency-guardian`, `reviewer-quality`
**Action**: Quick sweep — did the fix introduce side effects?

### ★ GATE 2: Fix Verified ★
**Reviewer**: Human
**Validates**: Root cause addressed, no side effects, regression tests adequate
**Outcome**: Approve / Request changes

---

### 7. DOCS + CHANGELOG
**Agents**: `docs-writer`, `changelog-writer`
**Action**: Update changelog, update docs if the bug affected documented behavior.

### ★ GATE 3: Final Acceptance ★
**Reviewer**: Human
**Validates**: Fix complete, documented, ready to merge
**Outcome**: Approve to merge

---

### 8. MERGE
**Agent**: `implementer`
**Action**: Merge, close task, write trace, clean up branch.

## Incident-to-Learning Loop

After merge, the `memory-distiller` agent extracts:
- **Root cause** → learning entry
- **Why it wasn't caught** → antipattern entry
- **Prevention** → suggested process improvement
