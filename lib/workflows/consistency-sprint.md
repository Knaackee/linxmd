---
name: consistency-sprint
type: workflow
version: 2.0.0
description: >
  Dedicated codebase sweep for dead code, naming issues, pattern violations,
  and import hygiene. Clean up without changing behavior.
agents:
  - consistency-guardian
  - reviewer-quality
  - implementer
skills:
  - consistency-check
  - refactoring
  - trace-writing
  - conventional-commits
gates: 2
tags: [workflow, consistency, cleanup, dead-code, maintenance]
---

# Consistency Sprint Workflow

> Dedicated time to clean the codebase. No new features — just naming, dead code, patterns, and hygiene.

## Flow Diagram

```
SCAN → REPORT → ★GATE 1★ → FIX (auto + manual) → VERIFY → ★GATE 2★ → MERGE
```

## Phases

### 1. SCAN
**Agent**: `consistency-guardian`
**Action**:
- Full codebase sweep for:
  - Dead code (unreachable, unused functions/variables)
  - Naming inconsistencies
  - Unused imports
  - Pattern violations
  - File/function length violations
  - Nesting depth violations
- Produce comprehensive report

---

### 2. REPORT
**Agent**: `consistency-guardian`
**Output**: Categorized findings with auto-fix recommendations

```markdown
## Consistency Sprint Report

### Auto-fixable (safe)
| # | File | Issue | Fix |
|---|------|-------|-----|
| 1 | src/old.ts | Unused import | Remove |
| 2 | src/util.ts | Dead function | Remove |

### Requires Human Decision
| # | File | Issue | Why Manual |
|---|------|-------|-----------|
| 1 | src/api/Users.ts | Wrong casing | Affects 12 importers |
| 2 | src/core/engine.ts | 400-line file | Needs thoughtful split |
```

### ★ GATE 1: Approve Scope ★
**Reviewer**: Human
**Validates**: Which auto-fixes to apply, which manual fixes to include, what to defer
**Outcome**: Approve scope / Adjust

---

### 3. FIX
**Agents**: `consistency-guardian` (auto), `implementer` (manual)
**Action**:
- Apply approved auto-fixes
- Implement approved manual fixes
- Each fix is its own commit: `refactor(scope): description`
- Run tests after every fix

---

### 4. VERIFY
**Agent**: `reviewer-quality`
**Action**:
- Confirm no behavior changes
- Confirm all tests pass
- Confirm fixes match approved scope

### ★ GATE 2: Final Review ★
**Reviewer**: Human
**Validates**: Changes are correct, no behavior changes, tests pass
**Outcome**: Approve to merge

---

### 5. MERGE
**Agent**: `implementer`
**Action**: Merge, write trace, update consistency baseline.

## Exit Criteria

- [ ] All approved issues fixed
- [ ] No behavior changes
- [ ] All tests passing
- [ ] Trace written with full change list
