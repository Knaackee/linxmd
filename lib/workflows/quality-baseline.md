---
name: quality-baseline
type: workflow
version: 2.0.0
description: >
  Establish or re-establish a quality baseline for the project. Full audit
  of code quality, security, performance, and documentation.
agents:
  - reviewer-quality
  - consistency-guardian
  - performance-monitor
  - fact-checker
  - docs-writer
skills:
  - observability
  - consistency-check
  - performance-profiling
  - trace-writing
gates: 2
quickActions:
  - id: qa-quality-checklist
    icon: "📋"
    label: Quality Checklist
    prompt: Build a quality checklist for correctness, security, performance, observability, and documentation from the current context.
    trigger:
      fileMatch:
        - '^\.linxmd/specs/.*\.md$'
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
  - id: qa-risk-register
    icon: "⚠️"
    label: Risk Register
    prompt: Create a risk register with probability, impact, owner, and mitigation actions. Prioritize top risks first.
    trigger:
      fileMatch:
        - '^\.linxmd/tasks/in-progress/.*/NOTES\.md$'
      languageId: [markdown]
tags: [workflow, quality, baseline, audit, security]
---

# Quality Baseline Workflow

> Full project audit. Establish the current quality level and create a plan to improve. Run periodically or after major changes.

## Flow Diagram

```
AUDIT → REPORT → ★GATE 1★ → PLAN IMPROVEMENTS → ★GATE 2★
```

## Phases

### 1. AUDIT
**Agents**: All control + delivery agents
**Action**: Run a comprehensive audit:

| Area | Agent | Checks |
|------|-------|--------|
| Code Quality | `reviewer-quality` | SOLID, DRY, complexity, standards compliance |
| Security | `reviewer-quality` | OWASP Top 10, dependency audit, secrets scan |
| Consistency | `consistency-guardian` | Naming, dead code, imports, patterns |
| Performance | `performance-monitor` | Full benchmark suite, establish baselines |
| Documentation | `fact-checker` | Accuracy of README, API docs, PROJECT.md |
| Observability | `reviewer-quality` | Logging, tracing, error handling, health signals |
| Test Coverage | `reviewer-quality` | Coverage report, missing test areas |

---

### 2. REPORT
**Output**: Comprehensive quality report:

```markdown
# Quality Baseline Report — <date>

## Summary
| Area | Status | Score | Details |
|------|--------|-------|---------|
| Code Quality | ⚠️ | 7/10 | 5 functions over 30 lines |
| Security | ✅ | 9/10 | All clean except 1 outdated dep |
| Consistency | ❌ | 5/10 | 23 naming issues, 8 dead functions |
| Performance | ✅ | 8/10 | Baselines established |
| Documentation | ⚠️ | 6/10 | API docs 40% incomplete |
| Observability | ❌ | 4/10 | 60% of endpoints lack logging |
| Test Coverage | ⚠️ | 7/10 | 72% line coverage, target 80% |

## Detailed Findings
(per area...)

## Measurements
(benchmark data, coverage data, metrics)
```

### ★ GATE 1: Baseline Review ★
**Reviewer**: Human
**Validates**: Report accuracy, priority assessment
**Outcome**: Accept baseline / Adjust priorities

---

### 3. PLAN IMPROVEMENTS
**Agent**: `planner`
**Action**:
- Create improvement tasks ordered by impact
- Group into sprints
- Each task has clear acceptance criteria and measurable outcome

### ★ GATE 2: Improvement Plan ★
**Reviewer**: Human
**Validates**: Plan is realistic, priorities are correct
**Outcome**: Approve / Adjust

## Exit Criteria

- [ ] Full audit completed
- [ ] Quality scores established per area
- [ ] Performance baselines stored
- [ ] Improvement tasks created and prioritized
- [ ] Human approved baseline and plan
