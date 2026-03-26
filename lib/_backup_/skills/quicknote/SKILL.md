---
name: quicknote
type: skill
level: growth
version: 2.0.0
description: >
  Fast capture of fleeting ideas, links, and thoughts. No overhead —
  just capture and move on. Organize later.
tags: [growth, notes, capture, ideas, quick]
---

# Quicknote Skill

> Capture now, organize later. Quicknotes are for fleeting thoughts, links, ideas that shouldn't be lost but don't deserve a task yet.

## Quicknote Format

```markdown
---
id: NOTE-YYYY-MM-DD-HHMM
date: 2026-03-23T14:30:00Z
tags: [idea, auth, future]
---

Brief note content. One paragraph max.

Optional: link, code snippet, or reference.
```

## Location

```
.linxmd/notes/YYYY-MM-DD-HHMM-short-title.md
```

Example: `.linxmd/notes/2026-03-23-1430-jwt-refresh-idea.md`

## Usage

### Capture During Work
When working on a task and a tangential thought appears:
1. Write a quicknote (< 30 seconds)
2. Continue the actual task
3. Don't context-switch to explore the idea

### Organize Later
Periodically (or during planning sessions):
- Review `.linxmd/notes/`
- Promote worthy notes to tasks, specs, or research topics
- Archive or delete stale notes

## Rules

- **Speed over polish** — no formatting required, raw thoughts are fine
- **One idea per note** — don't bundle unrelated thoughts
- **Always date and tag** — for later filtering
- **Don't interrupt flow** — writing a quicknote should take < 30 seconds
- **Review regularly** — unreviewed notes rot
