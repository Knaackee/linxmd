---
name: quality-baseline
type: workflow
version: 0.3.0
description: Raise any project to a quality baseline — test coverage, static analysis, security scan, documentation, observability, and CI/CD, all scored in a single prioritised report
deps:
  - agent:test-writer@>=0.2.0
  - agent:reviewer-quality@>=0.2.0
  - skill:observability@>=0.2.0
  - skill:debugging@>=0.2.0
  - skill:task-management@>=0.2.0
tags:
  - quality
  - audit
  - coverage
  - security
---

# Quality Baseline Workflow

## Overview

A systematic, 6-stage quality audit that produces a scored `QUALITY_REPORT.md` and a prioritised action plan. Run once on a new project, or periodically as a regression check.

This is not a one-pass fix. Each stage produces findings; actions are tracked in TASKS.md and executed incrementally.

## Start Conditions

Triggered by: "run quality baseline", "audit project quality", "raise quality to baseline", or installing this workflow and saying "start".

## Stages

### Stage 1: Test Coverage

1. Run the test suite with coverage enabled
2. Read the coverage report
3. Identify uncovered code paths, branches, and edge cases
4. `agent:test-writer` → write tests for the highest-risk uncovered paths
5. Target: ≥ 80% line coverage, ≥ 70% branch coverage
6. Score: **PASS** (≥ threshold) | **PARTIAL** (60–80%) | **FAIL** (< 60%)

### Stage 2: Static Analysis

1. Run the appropriate analyzer for the detected tech stack:
   - TypeScript: `tsc --noEmit`, `eslint`
   - Python: `ruff`, `mypy`
   - .NET: `dotnet build` warnings, `dotnet-format`
   - Rust: `cargo clippy`
   - Go: `go vet`, `staticcheck`
2. Categorize findings: Error | Warning | Info
3. Fix all Errors; report Warnings with recommended priority
4. Score: **PASS** (zero errors, < 10 warnings) | **PARTIAL** | **FAIL**

### Stage 3: Security Scan

1. Check for known CVEs in dependencies:
   - npm: `npm audit`
   - Python: `pip-audit`
   - .NET: `dotnet list package --vulnerable`
   - Rust: `cargo audit`
2. Apply OWASP Top 10 checklist for the tech stack
3. Scan for secret patterns in committed files (API keys, tokens, connection strings)
4. Score: **PASS** (no High/Critical CVEs, no secrets) | **PARTIAL** (Low CVEs only) | **FAIL** (Critical/High CVEs or secrets found)

### Stage 4: Documentation

Check for:
- Missing or empty README.md
- Undocumented public APIs (functions/methods with no docstring/comment)
- Stale code examples in docs that no longer match the code
- Missing CHANGELOG.md
- Missing architecture overview for non-trivial projects

Score: **PASS** (all present) | **PARTIAL** | **FAIL**

### Stage 5: Observability

Apply `skill:observability` to verify:
1. Structured logging at key boundaries (request start/end, errors, critical paths)
2. Errors logged with context — not swallowed or logged as plain strings
3. Long-running operations log progress checkpoints

Score: **PASS** | **PARTIAL** | **FAIL**

### Stage 6: CI/CD

1. Check for a CI pipeline (`.github/workflows/`, `.gitlab-ci.yml`, `Jenkinsfile`, etc.)
2. Verify it runs on every PR: tests, linting, at minimum
3. Check for a deployment pipeline if applicable

Score: **PASS** (CI present and runs tests) | **PARTIAL** (CI present, no tests) | **FAIL** (no CI)

## Output Report

Produce `QUALITY_REPORT.md`:

```markdown
# Quality Baseline Report

**Date**: YYYY-MM-DD
**Project**: [name]

## Scorecard

| Stage            | Status           | Priority |
|------------------|------------------|----------|
| Test Coverage    | ✅ PASS / ⚠️ PARTIAL / ❌ FAIL | High |
| Static Analysis  | ...              | ...      |
| Security         | ...              | ...      |
| Documentation    | ...              | ...      |
| Observability    | ...              | ...      |
| CI/CD            | ...              | ...      |

## Priority Action Plan

### P1 — Critical (block release)
- [ ] [action]

### P2 — High (fix this sprint)
- [ ] [action]

### P3 — Medium (next sprint)
- [ ] [action]

### P4 — Low (backlog)
- [ ] [action]
```

After producing the report, add all P1 and P2 actions to TASKS.md via `skill:task-management`.

## Max Iterations

Each stage that fails produces findings, not blockers. The workflow continues to all 6 stages even when earlier stages fail — the goal is a complete picture, not an early exit.

## Execution Modes

- **autonomous**: Runs all stages, produces the full report
- **guided**: Pauses after each stage for review

## When NOT to Use

- On a project with no existing code — run `workflow:sdd-tdd` instead
- When targeting a single specific issue — use `workflow:bug-fix` or the relevant skill directly
