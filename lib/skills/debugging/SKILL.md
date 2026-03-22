---
name: debugging
type: skill
version: 0.2.0
description: Systematic debugging with hypothesis tracking
deps: []
tags:
  - debugging
  - troubleshooting
---

# Debugging Skill

Triggered by: test failures, runtime errors, or explicit "debug [problem]" requests.

## Process

0. **Rubberduck step**: Verbalize the problem in one sentence — what was expected, what actually happened, what you have already tried. This forces problem clarification before any fix attempt.
1. **Read logs first** — check logs/ for recent entries
2. **Form one hypothesis**: "The error occurs because X"
3. **Apply one atomic fix** targeting that hypothesis
4. **Verify**: run tests or reproduce the issue
5. **Evaluate**:
   - Fixed → continue with next task
   - Not fixed → form a NEW hypothesis (never repeat the same fix)
   - No new hypothesis → STOP and report

## Techniques

- Trace the first failing assertion, not the last emitted error
- Isolate scope: reproduce with the smallest test or input surface
- Use stack traces and boundary logs to validate assumptions
- Bisect recent changes when a regression appears after a known good state
- Prefer debugger breakpoints for state-dependent failures

## Rules

- One hypothesis at a time
- One atomic fix per hypothesis
- Never repeat a fix that already failed
- Always log what was tried and the result
- Maximum 5 attempts before escalating to user
- If behavior is unclear, write or update a failing test before another code change

## Log Format

```
[TIMESTAMP] [DEBUG] Hypothesis: [X]
  Fix: [what was changed]
  Result: [pass / still failing — new symptom]
```

## Performance Debugging

Apply only after a measured baseline shows a problem — never optimize before profiling.

1. Measure first: establish a baseline with profiling or benchmarks
2. Identify the hot path: which function contributes most to total time?
3. Form a hypothesis: "Reducing allocations in X will halve latency because Y"
4. Apply one targeted change and re-measure
5. Accept the change only if the measured improvement matches the hypothesis

## Escalation

After 5 failed hypotheses or when stuck:
"I've tried [N] approaches. Here's what I know:
- [hypothesis 1]: [result]
- [hypothesis 2]: [result]
What would you like to try?"

