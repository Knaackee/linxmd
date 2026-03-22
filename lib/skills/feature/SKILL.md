---
name: feature
type: skill
version: 0.0.1
description: Feature development with SDD+TDD workflow
deps:
  - skill:task-management@>=0.0.1
tags:
  - feature
  - sdd
  - tdd
---

# Feature Skill

Triggered by: "lets do this", "start", "begin", or naming a backlog item.
Append "(guided)" for guided mode. Triggers on "next task" to continue a guided feature.

Orchestrates the full SDD+TDD workflow:
SPEC → TASKS → Red/Green/Spec-Review/Quality-Review/Docs/Commit → PR.

## 1 — Find the backlog item

Look in `.agentsmd/tasks/backlog/` for the matching file.
If multiple exist and no name given: list them and ask "Which one?"
If exactly one exists: use it without asking.

Read the file:
- Free text → use as feature description
- Issue number (e.g. "Issue: #42") → run `gh issue view 42 --json title,body,comments`
- Both → combine them

## 2 — Draft SPEC.md — wait for approval

Create `.agentsmd/tasks/in-progress/[name]/SPEC.md`
Move backlog file to `.agentsmd/tasks/in-progress/[name]/backlog-original.md`
Create `.agentsmd/tasks/in-progress/[name]/NOTES.md` (empty)

Output: "Here is the SPEC. Review Acceptance Criteria and Non-Goals.
Each criterion becomes a failing test. OK to proceed?"

Wait for approval. Do not create TASKS.md yet.

## 3 — Create TASKS.md and determine mode

**Detect mode:**
- User said "lets do this (guided)" → MODE = guided
- Default → MODE = autonomous

Create `.agentsmd/tasks/in-progress/[name]/TASKS.md`

**If autonomous:**
"Running [N] tasks autonomously. I will only stop on BLOCKER."
→ Proceed directly to task loop.

**If guided:**
"[N] tasks ready. Say 'next task' to begin."
→ Wait for "next task".

## 4 — Task loop

For each unchecked task in TASKS.md:

1. **RED** → invoke test-writer → failing tests
2. **GREEN** → invoke implementer → tests pass
3. **SPEC-REVIEW** → invoke reviewer-spec → all criteria covered?
4. **QUALITY-REVIEW** → invoke reviewer-quality → code quality + security
5. **DOCS** → invoke docs-writer → documentation updated
6. **COMMIT** → check off task → git commit

**Stop conditions (both modes):**
- Implementer blocked → stops, reports, waits
- Spec review BLOCKER → stops, reports, waits
- Quality review BLOCKER → stops, reports, waits

**Autonomous:** proceed to next task after commit.
**Guided:** output "Task [N] done. Say 'next task' to continue."

## 5 — Open PR when all tasks done

Run final checks → open PR:
```
gh pr create --title "feat: [feature name]" --body "$(cat SPEC.md)" --base main
```

Output:
"All [N] tasks complete. PR is open.
Review: `gh pr view --web`
Merge: `gh pr merge` ← you do this"

