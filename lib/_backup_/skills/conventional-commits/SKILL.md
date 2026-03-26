---
name: conventional-commits
type: skill
level: governance
version: 2.0.0
description: >
  Commit message format following Conventional Commits standard.
  type(scope): message — enforced on every commit.
quickActions:
  - id: qa-commit-message-from-task
    icon: "📌"
    label: Commit Message from Task
    prompt: Propose a conventional commit message from the documented task outcome with optional body and breaking-change note when needed.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
      contentMatch:
        - 'TASK-|changed|fixed|added|removed'
tags: [governance, git, commits, changelog, versioning]
---

# Conventional Commits Skill

> Every commit follows the same format. This enables automated changelogs, semantic versioning, and clear project history.

## Format

```
type(scope): short description

[optional body]

[optional footer(s)]
```

## Types

| Type | When | Bumps |
|------|------|-------|
| `feat` | New feature or capability | MINOR |
| `fix` | Bug fix | PATCH |
| `docs` | Documentation only | - |
| `style` | Formatting, whitespace (no logic change) | - |
| `refactor` | Code change that neither fixes a bug nor adds a feature | - |
| `perf` | Performance improvement | PATCH |
| `test` | Adding or fixing tests | - |
| `chore` | Build process, dependencies, tooling | - |
| `ci` | CI/CD configuration | - |
| `revert` | Reverts a previous commit | - |

## Scope

The scope is the area of the codebase affected:
- `auth`, `api`, `db`, `ui`, `config`, `deps`, etc.
- Use consistent scopes within a project (document in PROJECT.md)

## Examples

```
feat(auth): add JWT refresh token endpoint [TASK-042]

Implements the /auth/refresh endpoint that accepts a valid refresh
token and returns a new access token.

Closes #42
```

```
fix(api): return 404 instead of 500 for missing user [TASK-038]
```

```
test(auth): add failing tests for token expiry edge cases [TASK-042]
```

## Breaking Changes

```
feat(api)!: change user endpoint response format

BREAKING CHANGE: The /users endpoint now returns a paginated
response object instead of a flat array. Clients must update
to read data from the `data` field.
```

Breaking changes bump MAJOR. Mark with `!` after scope and include `BREAKING CHANGE:` in footer.

## Rules

- **Subject line ≤ 72 characters**
- **Imperative mood**: "add feature" not "added feature"
- **No period** at the end of the subject
- **Reference tasks**: include `[TASK-NNN]` in the subject
- **One logical change per commit**
