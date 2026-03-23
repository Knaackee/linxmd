---
name: debugging
type: skill
level: core
version: 2.0.0
description: >
  Systematic debugging methodology: reproduce first, isolate, fix, verify.
  Binary search for root cause, minimal reproduction cases.
tags: [core, debugging, troubleshooting, root-cause]
---

# Debugging Skill

> Systematic approach to finding and fixing bugs. Reproduce first, isolate the cause, fix minimally, verify completely.

## Methodology

### Step 1: Reproduce
Before anything else, **reproduce the bug reliably**:
- Write a failing test that demonstrates the bug
- The test IS the acceptance criterion for the fix
- If you can't reproduce it, gather more information before proceeding

### Step 2: Isolate
Use binary search to narrow down the root cause:
1. Identify the earliest point where behavior is correct
2. Identify the latest point where behavior is incorrect
3. Bisect: check the midpoint
4. Repeat until the cause is a single function/line

Techniques:
- **Logging**: Add targeted log statements at bisection points
- **Breakpoints**: Use debugger to inspect state at key locations
- **Reduction**: Simplify the reproduction case to eliminate noise
- **Git bisect**: If regression, find the introducing commit

### Step 3: Understand
Before fixing, understand:
- **Root cause** — the real bug, not the symptom
- **Impact scope** — what else might be affected?
- **Why it wasn't caught** — missing test? Missing validation? Design flaw?

### Step 4: Fix
- Minimal change that addresses the root cause
- Do NOT fix symptoms — fix the cause
- If the fix is large, break it into logical commits

### Step 5: Verify
- The reproduction test must now pass
- All existing tests must still pass
- Add regression tests for the specific failure mode
- Check related code paths for the same class of bug

## Common Bug Patterns

| Pattern | Symptom | Root Cause |
|---------|---------|------------|
| Off-by-one | Works for N but fails for N+1 | Boundary condition in loop/array |
| Race condition | Intermittent failures | Shared mutable state without synchronization |
| Null reference | Random crashes | Missing null check on optional data |
| Type coercion | Wrong results | Implicit type conversion (e.g., string "0" ≠ number 0) |
| State leak | Test order dependent | Shared state between tests |
| Encoding | Garbled text | UTF-8/ASCII mismatch |

## Anti-Patterns

- **Shotgun debugging**: Changing random things and hoping it works → Use binary search instead
- **Print debugging only**: Console.log everywhere → Use a debugger for complex issues
- **Fixing symptoms**: "Make the error go away" → Find and fix the root cause
- **No reproduction test**: "I fixed it, trust me" → Write a test that proves it

