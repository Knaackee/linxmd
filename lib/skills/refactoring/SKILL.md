---
name: refactoring
type: skill
version: 0.1.0
description: Safe refactoring with test coverage
deps:
  - agent:implementer@>=0.1.0
  - agent:reviewer-quality@>=0.1.0
tags:
  - refactoring
  - cleanup
---

# Refactoring Skill

Triggered by: "refactor [target]", "cleanup [target]", "simplify [target]",
or during QUALITY-REVIEW when reviewer suggests REFACTOR changes.

## Process

1. **Ensure tests exist** — run full test suite, confirm green
2. **Identify scope** — what exactly needs to change?
3. **Small steps** — one refactoring at a time, run tests after each
4. **No behavior changes** — tests must stay green throughout
5. **Quality review** — run reviewer-quality when done
6. **Commit** — one commit per logical refactoring step

## Rules

- Never refactor without passing tests first
- Never change behavior during refactoring
- Run tests after EVERY change, no matter how small
- If a test fails: undo the last change, rethink approach
- Keep commits small and focused

## Common Refactorings

- **Extract method** — long method → smaller focused methods
- **Rename** — unclear names → self-documenting names
- **Remove duplication** — repeated code → shared helper
- **Simplify conditionals** — nested ifs → guard clauses or pattern matching
- **Move responsibility** — code in wrong class → move to correct location

## Commit Convention

- Use focused messages like `refactor: extract parser normalization`
- Do not mix behavior changes into refactor commits

## Report

"Refactored [target]: [what changed]. All tests green. [N] commits."

