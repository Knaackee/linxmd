---
name: research-spike
type: workflow
version: 2.0.0
description: >
  Structured investigation before committing to an approach. Research, analyze,
  recommend — then let the human decide before any code is written.
agents:
  - router
  - researcher
  - architect
skills:
  - research
  - trace-writing
gates: 2
tags: [workflow, research, spike, investigation, analysis]
---

# Research Spike Workflow

> Investigate before committing. A spike produces knowledge, not code. The human decides the approach based on evidence.

## Flow Diagram

```
SCOPE → RESEARCH → ANALYZE → ★GATE 1★ → RECOMMEND → ★GATE 2★
```

## Phases

### 1. SCOPE
**Agent**: `router`
**Action**:
- Define the research question clearly
- Set depth level: quick (2h), standard (4h), deep (8h+)
- Identify what decisions depend on this research
- Create task with research type

---

### 2. RESEARCH
**Agent**: `researcher`
**Action**:
- Gather sources (official docs, repos, papers, benchmarks)
- Prioritize sources with reproducible evidence
- Document findings as they're discovered
- Track sources for citation

---

### 3. ANALYZE
**Agent**: `researcher` + `architect`
**Action**:
- Synthesize findings into comparison matrix
- Evaluate against project-specific criteria
- Identify trade-offs for each option
- Note unknowns and risks

### ★ GATE 1: Findings Review ★
**Reviewer**: Human
**Validates**: Research is thorough, sources are credible, analysis is fair
**Outcome**: Sufficient / Need more research / Redirect focus

---

### 4. RECOMMEND
**Agent**: `architect`
**Action**:
- Based on findings, recommend an approach
- Write an ADR (Architecture Decision Record) with:
  - Context
  - Options evaluated
  - Decision and rationale
  - Consequences
- Save to `.linxmd/memory/decisions/`

### ★ GATE 2: Decision ★
**Reviewer**: Human
**Validates**: Recommendation makes sense, trade-offs are acceptable
**Outcome**: Accept recommendation / Choose different option / Need more research

## Exit Criteria

- [ ] Research report with sources
- [ ] Comparison matrix
- [ ] ADR written (even if decision is "not yet")
- [ ] Human has enough information to decide
- [ ] Trace written
