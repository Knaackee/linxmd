---
name: debugging
type: skill
version: 1.0.0
description: Systematisches Debugging mit Hypothesen-Tracking
deps: []
tags:
  - debugging
  - troubleshooting
---

# Debugging Skill

Triggered by: test failures, runtime errors, or explicit "debug [problem]" requests.

## Process

1. **Read logs first** — check logs/ for recent entries
2. **Form one hypothesis**: "The error occurs because X"
3. **Apply one atomic fix** targeting that hypothesis
4. **Verify**: run tests or reproduce the issue
5. **Evaluate**:
   - Fixed → continue with next task
   - Not fixed → form a NEW hypothesis (never repeat the same fix)
   - No new hypothesis → STOP and report

## Rules

- One hypothesis at a time
- One atomic fix per hypothesis
- Never repeat a fix that already failed
- Always log what was tried and the result
- Maximum 5 attempts before escalating to user

## Log Format

```
[TIMESTAMP] [DEBUG] Hypothesis: [X]
  Fix: [what was changed]
  Result: [pass / still failing — new symptom]
```

## Escalation

After 5 failed hypotheses or when stuck:
"I've tried [N] approaches. Here's what I know:
- [hypothesis 1]: [result]
- [hypothesis 2]: [result]
What would you like to try?"
