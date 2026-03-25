---
name: consistency-check
type: skill
level: governance
version: 2.0.0
description: >
  Codebase consistency verification: naming conventions, dead code detection,
  import hygiene, pattern compliance, and structural rules.
quickActions:
  - id: qa-consistency-batch
    icon: "🧹"
    label: Batch Consistency Check
    prompt: Find inconsistent terminology, IDs, status values, and references across current docs and task artifacts.
    trigger:
      fileMatch:
        - '^\.linxmd/specs/.*\.md$'
        - '^\.linxmd/tasks/.*\.md$'
      languageId: [markdown]
tags: [governance, consistency, naming, dead-code, imports, patterns]
---

# Consistency Check Skill

> Consistency is trust. When code follows predictable patterns, developers (human and AI) can reason about it faster and make fewer mistakes.

## Check Categories

### 1. Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Files (components) | PascalCase | `UserProfile.tsx` |
| Files (utilities) | camelCase or kebab-case | `formatDate.ts` / `format-date.ts` |
| Variables | camelCase | `userName`, `isActive` |
| Constants | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT` |
| Classes | PascalCase | `UserService` |
| Functions | camelCase | `getUserById()` |
| Interfaces | PascalCase, no I prefix | `UserProfile` (not `IUserProfile`) |
| Enums | PascalCase (name), UPPER_SNAKE (values) | `enum Status { ACTIVE, INACTIVE }` |

**Note**: These are defaults. The project's `PROJECT.md` defines the authoritative conventions. Always defer to project standards.

### 2. Dead Code Detection

Look for:
- **Unreachable code**: after `return`, `throw`, `break` without conditions
- **Commented-out code**: should be deleted, not preserved (git has history)
- **Unused functions/methods**: defined but never called
- **Unused variables**: assigned but never read
- **Unused imports**: imported but not referenced
- **Unused type definitions**: defined but never used

### 3. Import Hygiene

- No unused imports
- No duplicate imports
- Consistent import ordering (external → internal → relative)
- No circular dependencies

### 4. Pattern Compliance

- Error handling follows the project's established pattern
- Logging follows the observability standard
- File organization matches existing structure
- Test file naming matches the convention (`.test.ts`, `.spec.ts`, etc.)

### 5. Structural Rules

| Rule | Threshold | Action |
|------|-----------|--------|
| Function length | > 30 lines | Flag for extraction |
| File length | > 300 lines | Flag for splitting |
| Nesting depth | > 3 levels | Flag for guard clauses |
| Parameters | > 4 | Flag for parameter object |
| Cyclomatic complexity | > 10 | Flag for simplification |

## Output Format

```markdown
## Consistency Report

### Summary: X issues found (Y auto-fixable, Z require human decision)

### Issues
| # | Category | Severity | Location | Description | Auto-fix? |
|---|----------|----------|----------|-------------|-----------|

### Auto-Fixed Items
- Description of each auto-fix applied

### Items Requiring Decision
- Description and options for human
```
