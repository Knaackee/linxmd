---
name: bug-fix
type: workflow
version: 0.3.0
description: Structured pipeline for reproducing, fixing, and verifying a confirmed bug — with a mandatory regression test and changelog entry so it never comes back silently
deps:
  - agent:implementer@>=0.2.0
  - agent:test-writer@>=0.2.0
  - agent:reviewer-spec@>=0.2.0
  - agent:reviewer-quality@>=0.2.0
  - agent:docs-writer@>=0.2.0
  - agent:changelog-writer@>=0.3.0
  - skill:debugging@>=0.2.0
tags:
  - bugfix
  - debugging
  - tdd
---

# Bug Fix Workflow

## Overview

A focused pipeline for fixing a confirmed bug. Unlike sdd-tdd, this workflow
starts from broken behavior — not a greenfield spec. The regression test is
the spec.

## Start Conditions

Triggered by: "fix", "broken", "regression", "failing test", or a specific error message.

## Pipeline

1. **REPRODUCE** → `debugging` skill → Isolate the exact failure condition
   - Characterize the bug: what input, what state, what output
   - Identify the minimal reproduction case
   - Do not proceed until the cause is understood

2. **REGRESSION TEST** → `test-writer` → Write a failing test that captures the bug exactly
   - The test must fail on the current code
   - The test must pass only after the correct fix is applied
   - Name the test descriptively: `[Scenario]_[Trigger]_[ExpectedBehavior]`
   - Do not proceed until the regression test exists and fails

3. **FIX** → `implementer` → Minimal fix until the regression test passes
   - All previously passing tests must still pass
   - No unrelated changes allowed

4. **SPEC-REVIEW** → `reviewer-spec` → Does the fix address the root cause, not just the symptom?
   - If BLOCKER: route back to implementer

5. **QUALITY-REVIEW** → `reviewer-quality` → No new issues introduced
   - If BLOCKER: route back to implementer

6. **DOCS** → `docs-writer` → Update any documentation affected by the behavior change

7. **CHANGELOG** → `changelog-writer` → Append a `Fixed` entry to `CHANGELOG.md`
   - Format: `- Fix [what was broken] ([reproduction context])`
   - This step is never skipped

8. **COMMIT** → `fix: [description of what was broken]`

## Max Iterations

If the FIX → SPEC-REVIEW cycle fails 3 times for the same bug, stop and report a root cause analysis. Wait for user decision before continuing.

## Execution Modes

- **autonomous**: Runs all steps without pausing. Only stops on BLOCKER or max iterations.
- **guided**: Waits after each step for "next step".

## When NOT to Use

- When the "bug" is actually missing functionality → route to `workflow:sdd-tdd`
- When the fix requires a new spec and new tests from scratch → route to `workflow:sdd-tdd`
