---
name: research
type: skill
level: growth
version: 2.0.0
description: >
  Structured research methodology: scope, collect, analyze, synthesize.
  Produces actionable research reports with sourced findings.
tags: [growth, research, investigation, analysis, decision-support]
---

# Research Skill

> Good decisions need good information. This skill defines how to research a topic systematically and produce actionable findings.

## Research Process

### Phase 1: Scope
Define clearly:
- **Question**: What are we trying to answer?
- **Depth**: Quick survey (1h) | Standard investigation (4h) | Deep dive (1d+)
- **Boundaries**: What's in scope? What's not?
- **Success criteria**: What does "answered" look like?

### Phase 2: Collect
Gather sources in priority order:

| Priority | Source Type | Trust Level |
|----------|-----------|-------------|
| 1 | Official documentation | High |
| 2 | Peer-reviewed papers, RFCs | High |
| 3 | GitHub repos with eval results / benchmarks | High |
| 4 | Established tech blogs (specific authors) | Medium |
| 5 | Stack Overflow / GitHub Issues | Medium |
| 6 | Reddit / HN / Twitter | Low (discovery only) |

Rules:
- **Minimum 3 sources** for any recommendation
- **Prefer reproducible evidence** — repos with benchmarks > blog posts with opinions
- **Note recency** — technology evolves fast, date every source
- **Save sources** — include URL, title, date, and key quote

### Phase 3: Analyze
For each finding:
- What does it claim?
- What evidence supports it?
- Does it contradict other findings?
- How relevant is it to our specific situation?

For comparisons, use a structured matrix:
```markdown
| Criterion | Option A | Option B | Option C |
|-----------|----------|----------|----------|
| Performance | ... | ... | ... |
| Complexity | ... | ... | ... |
| Community | ... | ... | ... |
| Maturity | ... | ... | ... |
```

### Phase 4: Synthesize
Produce a report:
```markdown
## Research: <Question>
### Summary (1–3 sentences)
### Key Findings (numbered, sourced)
### Comparison Matrix (if applicable)
### Recommendation
### Confidence Level: high | medium | low
### Open Questions
### Sources (numbered list with URLs)
```

## Rules

- **Cite everything** — no unsourced claims
- **Distinguish fact from opinion** — label clearly
- **Time-box** — declare research depth upfront, don't spiral
- **Be honest about uncertainty** — "I don't know" is valid
- **Connect to project** — every finding relates back to our context
