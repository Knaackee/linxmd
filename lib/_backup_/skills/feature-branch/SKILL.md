---
name: feature-branch
type: skill
level: governance
version: 2.0.0
description: >
  Branch naming conventions, PR strategy, and merge discipline.
  Predictable, clean git history.
tags: [governance, git, branching, pr, merge]
---

# Feature Branch Skill

> Predictable branch names, clean PRs, and disciplined merging. Every branch tells you what it's for at a glance.

## Branch Naming Convention

```
type/short-description
```

| Type | Usage | Example |
|------|-------|---------|
| `feat/` | New feature | `feat/user-authentication` |
| `fix/` | Bug fix | `fix/login-redirect-loop` |
| `chore/` | Maintenance, tooling | `chore/update-dependencies` |
| `docs/` | Documentation changes | `docs/api-reference-update` |
| `refactor/` | Code restructuring | `refactor/auth-module-cleanup` |
| `test/` | Test additions/fixes | `test/jwt-edge-cases` |
| `perf/` | Performance work | `perf/query-optimization` |
| `spike/` | Research/proof of concept | `spike/graphql-evaluation` |

## Rules

- **Lowercase, hyphen-separated**: `feat/user-auth` not `feat/UserAuth`
- **Short but descriptive**: max 50 characters for the description part
- **Reference task if applicable**: in commit messages, not in branch name
- **Branch from**: `main` or `develop` (as defined in PROJECT.md)
- **One branch per task**: don't mix tasks in a branch

## PR Strategy

### PR Title
Same format as conventional commit:
```
feat(auth): implement JWT authentication [TASK-042]
```

### PR Description Template
```markdown
## What
Brief description of what this PR does.

## Why
Link to spec: SPEC-NNN
Link to task: TASK-NNN

## How
Key implementation decisions.

## Testing
How was this tested? What's covered?

## Preview
Link to preview or screenshots.

## Checklist
- [ ] Tests pass
- [ ] No regressions
- [ ] Documentation updated
- [ ] Changelog updated
```

## Merge Strategy

| Situation | Strategy |
|-----------|----------|
| Feature into main/develop | **Squash merge** — clean, single commit |
| Long-lived branch | **Merge commit** — preserve branch history |
| Hotfix | **Fast-forward** if possible, merge commit otherwise |

After merge:
- Delete the feature branch
- Clean up the worktree
- Close the associated task
