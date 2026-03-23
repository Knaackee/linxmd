---
name: brainstorming
type: skill
level: growth
version: 2.0.0
description: >
  Structured ideation techniques: HMW questions, SCAMPER, mind mapping, impact/effort
  matrices, and idea ranking frameworks.
tags: [growth, ideation, creativity, brainstorming, hmw]
---

# Brainstorming Skill

> Structured thinking produces better ideas than random inspiration. Use proven techniques to explore the solution space systematically.

## Techniques

### 1. How Might We (HMW) Questions

Transform problems into opportunity questions:
```
Problem: Users abandon checkout because the form is too long
HMW: How might we reduce checkout friction without losing necessary data?
HMW: How might we make a long form feel short?
HMW: How might we capture data without a form?
```

Rules:
- Start with "How might we..."
- Not too broad ("How might we make users happy?")
- Not too narrow ("How might we add autofill to the email field?")
- Generate 5–10 HMW questions per problem

### 2. SCAMPER

Apply each lens to an existing feature/product:

| Letter | Action | Prompt |
|--------|--------|--------|
| **S** | Substitute | What can we replace? |
| **C** | Combine | What can we merge? |
| **A** | Adapt | What can we borrow from elsewhere? |
| **M** | Modify | What can we change in form/function? |
| **P** | Put to another use | New use cases? |
| **E** | Eliminate | What can we remove? |
| **R** | Reverse | What if we flip the order/structure? |

### 3. Impact/Effort Matrix

Rank every idea on two axes:

```
High Impact │
            │  CONSIDER        DO FIRST
            │
────────────┼───────────────────────────
            │
            │  SKIP            QUICK WIN
            │                         
            └──────────────────────── High Effort
```

### 4. Mind Mapping

Start with the central concept, branch out:
```
              ┌── API rate limiting
    Security ─┤
              └── Input validation
              ┌── Response time
Performance ──┤
              └── Caching strategy
```

### 5. Constraint Removal

1. "If we had unlimited budget, what would we build?"
2. "If we had 10x the team, what would change?"
3. "If there were no technical limitations, what's ideal?"
4. Then add constraints back one by one to find the feasible version.

## Output Format

```markdown
## Brainstorm: <Topic>

### Problem
### HMW Questions (5–10)
### Ideas (unranked, quantity first)
### Impact/Effort Ranking
| # | Idea | Impact (1-5) | Effort (1-5) | Priority |
### Top 3 Recommendations
### Next Steps
```

## Rules

- **Quantity before quality** — generate first, judge later
- **No judgment during generation** — wild ideas welcome
- **Always rank** — unranked brainstorms are not actionable
- **Connect to action** — end with concrete next steps (specs to write, spikes to run)
