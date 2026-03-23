# Linxmd Principles

> Version 2.0 — Last updated 2026-03-23

These principles govern every agent, skill, workflow, and pack in the Linxmd library. They are non-negotiable. Every artifact must be designed and maintained in alignment with them.

---

## 1. Human-in-the-Loop Is Constitutional

Agents are powerful assistants, not autonomous actors. Every workflow has explicit **GATE** points where a human reviews, approves, modifies, or rejects before the next phase begins. There is no "run to completion" without human checkpoints.

The human is not a bottleneck — the human is the quality gate.

```
Idea → [HUMAN] → Spec → [HUMAN] → Plan → [HUMAN] → Code → [HUMAN] → Ship
```

---

## 2. Four Hard Gates

Every meaningful workflow enforces at minimum these four gates:

| Gate | When | What the Human Validates |
|------|------|--------------------------|
| **Spec Gate** | After specification is drafted | Requirements, acceptance criteria, scope boundaries |
| **Safety Gate** | After implementation + tests | Code quality, OWASP compliance, test coverage |
| **Preview Gate** | After preview is prepared | Functional correctness in a real environment |
| **Benchmark Gate** | Before merge | Performance, no regressions, documentation complete |

Gates are blocking. Agents stop and wait. No silent bypasses.

---

## 3. Memory Is the Operating System

Agents start every session with **zero context**. Memory bridges the gap:

| Tier | Location | Scope | Purpose |
|------|----------|-------|---------|
| **Session** | `.linxmd/traces/` | Ephemeral | What happened this session |
| **Project** | `.linxmd/memory/` + `PROJECT.md` | Persistent | Decisions, learnings, project state |
| **Global** | `~/.linxmd/` | Cross-project | User profile, preferences, patterns |

Every agent **reads** Project memory at start. Every agent **writes** a session trace at end. The `memory-distiller` agent compacts sessions into project memory periodically.

---

## 4. Dual Traceability

Two complementary systems ensure nothing is lost:

**Agent-level traces** — What the agent did and decided:
- Decision records, session summaries, files modified, still-open items
- Stored in `.linxmd/traces/`

**Code-level observability** — Whether the software actually works:
- Structured logging, distributed tracing, error tracking, health signals
- Enforced by the `observability` skill, checked by `reviewer-quality`

An agent that ships a feature without adequate logging has **not finished the task**.

---

## 5. Preview Before Ship

Every software change gets a preview phase before merge:
- **Artifact bundle**: Compiled output, screenshots, release notes, migration notes
- **Live preview**: Dev-server URL (Tailscale or similar) for hands-on testing
- **Both** (recommended for significant changes)

The human validates in the preview environment. If not approved, work routes back for fixes.

---

## 6. Eval-Driven Development

Every repository maintains its own benchmark suite:
- Repo-specific test benchmarks (not just "tests pass" but "performance is within bounds")
- Before/after metrics for every change
- Regressions are blocking — no merge if benchmarks degrade

The `performance-monitor` agent and `performance-profiling` skill enforce this.

---

## 7. Security and Policy Are First-Class

Security is not an afterthought:
- OWASP Top 10 is checked on every review
- Credentials never appear in code or logs
- Dependencies are audited
- The `reviewer-quality` agent has a mandatory security checklist

Runtime policy:
- Agents operate within defined scope — no scope creep
- Out-of-scope findings are logged as new tasks, never silently fixed
- Tool usage requires explicit permission where configured

---

## 8. Git Discipline Is Enforced

| Rule | Detail |
|------|--------|
| **Feature branches** | `feat/<desc>`, `fix/<id>`, `chore/<what>` |
| **Worktrees** | Isolated workspaces for parallel work |
| **Conventional commits** | `type(scope): message` — always |
| **Commit often** | Every logical unit = one commit. No mega-commits. |
| **Changelog-as-you-go** | Updated at every merge, not just at release |

---

## 9. Clear Role Separation

Agents are organized into three categories:

| Category | Purpose | Examples |
|----------|---------|---------|
| **Core** | Do the actual work | `implementer`, `test-writer`, `architect`, `planner` |
| **Control** | Ensure quality and compliance | `reviewer-quality`, `reviewer-spec`, `consistency-guardian` |
| **Delivery** | Package, document, and ship | `docs-writer`, `changelog-writer`, `preview-delivery` |

No agent crosses category boundaries. An implementer never reviews its own code. A reviewer never implements fixes.

---

## 10. Skills in Three Levels

| Level | Purpose | Examples |
|-------|---------|---------|
| **Core** | Fundamental capabilities every agent may need | `debugging`, `refactoring`, `task-management` |
| **Governance** | Quality, compliance, and process enforcement | `observability`, `conventional-commits`, `consistency-check` |
| **Growth** | Ideation, research, and knowledge expansion | `brainstorming`, `research`, `market-analysis` |

---

## 11. Incident-to-Learning Loop

When something goes wrong:
1. **Reproduce** — Write a failing test that captures the bug
2. **Investigate** — Find root cause, document in trace
3. **Fix** — Minimal change that makes the test pass
4. **Learn** — Write a learning to `.linxmd/memory/learnings/`
5. **Prevent** — Add regression tests, update `antipatterns.md`

Every incident becomes a permanent organizational learning.

---

## 12. Metrics-Driven Steering

Track what matters:

| Metric | What It Measures |
|--------|-----------------|
| **Gate pass rate** | How often work passes human review on first attempt |
| **Trace completeness** | Do all sessions leave proper traces? |
| **Test coverage delta** | Is coverage increasing with every feature? |
| **Regression rate** | How often do changes break existing functionality? |
| **Time-to-first-gate** | How quickly does work reach human review? |
| **Memory utilization** | Are agents reading and contributing to project memory? |

These metrics guide continuous improvement of agents, skills, and workflows.

---

*These principles are the constitution of the Linxmd system. Every artifact in `lib/` must be evaluated against them. When in doubt, re-read this document.*
