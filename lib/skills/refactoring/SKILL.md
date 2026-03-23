---
name: refactoring
type: skill
level: core
version: 2.0.0
description: >
  Safe code transformation techniques. Extract, rename, simplify, restructure —
  always with tests as safety net. Never change behavior.
tags: [core, refactoring, code-quality, clean-code]
---

# Refactoring Skill

> Change the design without changing the behavior. Every refactoring must be safe, incremental, and test-backed.

## Golden Rules

1. **Tests must pass before, during, and after** — never refactor without a safety net
2. **One refactoring at a time** — commit each transformation separately
3. **Never change behavior** — if behavior changes, it's not a refactoring, it's a feature
4. **Small steps** — prefer 10 tiny safe steps over 1 big risky step

## Common Refactoring Patterns

### Extract Function
**When**: A block of code does a distinct thing within a larger function.
```
Before: 30-line function with 3 responsibilities
After:  3 functions of 10 lines each, called from the original
```

### Extract Variable
**When**: A complex expression is hard to read inline.
```
Before: if (user.age >= 18 && user.verified && !user.banned)
After:  const isEligible = user.age >= 18 && user.verified && !user.banned
```

### Rename
**When**: A name doesn't reveal intent.
- Variables, functions, classes, files
- The cheapest refactoring with the highest readability payoff

### Inline
**When**: An abstraction adds complexity without value.
```
Before: function getAge(user) { return user.age; }
After:  user.age (direct access, no wrapper)
```

### Move
**When**: Code is in the wrong file/module.
- Move closer to where it's used
- Move to align with domain boundaries

### Replace Conditional with Polymorphism
**When**: A switch/if chain selects behavior based on type.
```
Before: if (type === 'A') doA() else if (type === 'B') doB() ...
After:  type.execute() — each type knows its own behavior
```

### Simplify Conditional
**When**: Boolean logic is tangled.
- Guard clauses instead of nested if/else
- Early returns for error cases
- De Morgan's laws for simplification

## Refactoring Checklist

1. [ ] All tests pass (baseline)
2. [ ] Identify the specific refactoring to apply
3. [ ] Apply the transformation
4. [ ] Run tests — they must still pass
5. [ ] Commit: `refactor(scope): description`
6. [ ] Repeat for next transformation

## Thresholds for Refactoring

| Metric | Threshold | Action |
|--------|-----------|--------|
| Function length | > 30 lines | Extract function |
| File length | > 300 lines | Extract module/class |
| Nesting depth | > 3 levels | Guard clauses, extract function |
| Parameters | > 4 | Introduce parameter object |
| Duplication | > 2 copies | Extract shared function |

## Anti-Patterns

- **Refactoring without tests** — you will break things and not know it
- **Big bang refactoring** — "let me rewrite this whole module" → do it incrementally
- **Refactoring and changing behavior** — that's two things, do them separately
- **Refactoring for aesthetics** — only refactor to solve a real problem (readability, maintainability, performance)

