---
name: worktree-management
type: skill
level: governance
version: 2.0.0
description: >
  Git worktree creation, usage, and cleanup for isolated parallel work.
  Each feature gets its own worktree — no cross-contamination.
tags: [governance, git, worktree, isolation, parallel-work]
---

# Worktree Management Skill

> Git worktrees give each feature an isolated workspace. No branch switching, no stash headaches, no cross-contamination.

## What Are Worktrees?

A git worktree is a separate working directory linked to the same repository. You can have multiple worktrees checking out different branches simultaneously.

## Commands

### Create a Worktree

```bash
# From the main working directory
git worktree add ../project-worktrees/feat-user-auth feat/user-auth

# Or create branch and worktree together
git worktree add -b feat/user-auth ../project-worktrees/feat-user-auth main
```

**Convention**: Use sibling directory `../project-worktrees/<branch-name>` to avoid gitignore complications.

### List Worktrees

```bash
git worktree list
```

### Remove a Worktree

```bash
# After merging the feature branch
git worktree remove ../project-worktrees/feat-user-auth

# Force remove if needed
git worktree remove --force ../project-worktrees/feat-user-auth
```

### Prune Stale References

```bash
git worktree prune
```

## Directory Convention

```
project/                           ← Main working directory (main branch)
project-worktrees/
├── feat-user-auth/                ← Worktree for auth feature
├── fix-login-redirect/            ← Worktree for login fix
└── chore-dependency-update/       ← Worktree for deps update
```

## Workflow Integration

1. **Start task** → Create worktree for the feature branch
2. **Work** → All changes happen in the worktree directory
3. **Commit** → Commits go to the feature branch
4. **Review/Merge** → From main directory, merge the feature branch
5. **Cleanup** → Remove the worktree, delete the branch if merged

## Rules

- **One worktree per feature** — no mixing work across features
- **Always clean up** — remove worktrees after merge
- **Sibling directory** — worktrees go next to the project, not inside it
- **Don't modify main** — main worktree stays on main/develop
- **Run prune periodically** — clean up stale references

## Troubleshooting

| Problem | Solution |
|---------|---------|
| "fatal: is already checked out" | That branch is already in a worktree — use it or remove it |
| Worktree points to missing branch | `git worktree prune` to clean up |
| Can't delete worktree with uncommitted changes | Commit or stash first, then remove |
