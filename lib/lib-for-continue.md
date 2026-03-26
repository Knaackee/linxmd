# lib-for-continue — Linxmd × OpenCode Continue Integration Plan

> Ziel: Kompletter Softwareentwicklungsprozess als agents/workflows/skills/packs — mit graph-memory als first-class Gedächtnis.

---

## 1. Ist-Zustand

### Was existiert

| Bereich | Ort | Status |
|---------|-----|--------|
| 20 Agents (v2.0) | `lib/_backup_/agents/` | Markdown-only Memory, kein graph-memory |
| 25 Skills (v2.0) | `lib/_backup_/skills/` | graph-memory existiert als Skill (v1.0), wird aber von keinem Agent referenziert |
| 8 Workflows (v2.0) | `lib/_backup_/workflows/` | Gate-basiert, rein file-basiert orchestriert |
| 6 Packs (v2.0) | `lib/_backup_/packs/` | Bundles ohne graph-memory |
| graph-memory Skill | `lib/_backup_/skills/graph-memory/` | Definiert API-Endpunkte für Continue, aber nirgends eingebunden |
| Specs & Frontmatter | `lib/agents/SPEC.md`, etc. | Frontmatter v2.1 Schema steht |

### Was Continue bietet

| Capability | CLI Command | Nutzen |
|-----------|-------------|--------|
| Session Context | `opencode-continue graph-memory context <scopeId> --depth 2` | ~200 Token Kontext-Injection beim Agent-Start |
| Entities lesen | `opencode-continue graph-memory nodes ...` | Knoten nach Typ/Scope lesen |
| Beziehungen lesen | `opencode-continue graph-memory edges ...` | Relationen zwischen Entitäten analysieren |
| Cypher Query | `opencode-continue graph-memory query "..."` | Präzise Graph-Abfragen (read-only) |
| Node Write | `opencode-continue graph-memory write-node ...` | Entities schreiben (Decision, Fact, Learning, Technology, Person) |
| Edge Write | `opencode-continue graph-memory write-edge ...` | Beziehungen (PREFERS, DEPENDS_ON, IMPLEMENTS, BLOCKED_BY) |
| Invalidate | `opencode-continue graph-memory invalidate <nodeId>` | Temporale Invalidierung (nie löschen) |

### Zentrale Lücke

**graph-memory ist definiert, aber von 0 Agents genutzt.** Kein Workflow hat einen Memory-Lifecycle. Agents starten "kalt" und lesen manuell Files.

---

## 2. Designentscheidungen

### D1: graph-memory wird Pflicht-Skill (graceful degradation)

Jeder Agent bekommt `graph-memory` als Skill. Startup-Protokoll:
1. `opencode-continue graph-memory context <scopeId> --depth 2` → Kontext laden (~200 Tokens)
2. Falls unavailable: Fallback auf `project-memory` (Markdown)
3. Agent arbeitet normal weiter

**Konsequenz**: Agents müssen doppeltes Startup-Protokoll haben (graph-first, markdown-fallback).

### D2: Memory-Lifecycle in Workflows verankern

Jeder Workflow bekommt explizite Memory-Phasen:
- **Session-Start**: Context Injection für alle beteiligten Agents
- **Session-End**: Trace → Graph Sync, Learnings schreiben, Nodes aktualisieren

### D3: Agents konsolidieren (20 → 15)

Einige Backup-Agents sind zu granular oder überlappen. Für Continue-Integration:

| Behalten (15) | Zusammenlegen / Entfernen |
|---|---|
| **product-manager** | Neu: Stage-0 Product Shaping |
| **router** | — |
| **planner** | — |
| **spec-writer** | — |
| **architect** | — |
| **test-writer** | — |
| **implementer** | — |
| **reviewer** (merged) | `reviewer-quality` + `reviewer-spec` → ein Agent mit Modi |
| **consistency-guardian** | — |
| **docs-writer** | `changelog-writer` wird Teil von `docs-writer` |
| **researcher** | `fact-checker` wird Modus von `researcher` |
| **onboarder** | `start-state-creator` wird Teil von `onboarder` |
| **memory-distiller** | — |
| **drafter** | `editor` wird Modus von `drafter` |
| **performance-monitor** | — |

**Begründung**: Weniger Agents = weniger Kontextwechsel, klarere Verantwortung, einfachere Packs.

### D4: Skills konsolidieren (25 → 17)

| Behalten (17) | Status |
|---|---|
| **graph** | Neu, CLI-only Interaktion mit graph-memory |
| **graph-memory** | Upgrade auf v2.0, Pflicht-Skill |
| **project-memory** | Fallback + Markdown-Fulltext |
| **task-management** | Backlog, Frontmatter v2 |
| **debugging** | Reproduce → Fix Methodik |
| **refactoring** | Safe Transformation |
| **conventional-commits** | Commit-Format |
| **trace-writing** | Audit Trail |
| **context-management** | LLM-Kontext optimieren |
| **api-design** | REST/HTTP Patterns |
| **observability** | Logging, Tracing, Health |
| **consistency-check** | Naming, Dead Code, Patterns |
| **performance-profiling** | Benchmarks, Baselines |
| **e2e-testing** | Critical Path Tests |
| **feature-branch** | Branch + PR Discipline |
| **research** | Systematische Recherche |
| **brainstorming** | Strukturierte Ideation |

**Entfernt / Integriert**:
- `user-profile` → in `graph-memory` (Person-Node + Preferences)
- `start-state-creation` → in `onboarder` Agent direkt
- `preview-delivery` → in Workflow-Gate direkt
- `worktree-management` → in `feature-branch` integriert
- `quicknote` → `graph-memory` quickCapture Pattern
- `code-translator`, `text-translator`, `i18n`, `design-tokens`, `market-analysis` → on-demand, nicht im Core

### D5: Workflows streamlinen (8 → 5)

| Workflow | Zweck | Agents | Gates |
|----------|-------|--------|-------|
| **project-start** | Neues/bestehendes Projekt onboarden | onboarder, planner, reviewer | 2 |
| **feature-development** | Idea → Merge (TDD) | spec-writer, planner, architect, test-writer, implementer, consistency-guardian, reviewer, docs-writer | 5 |
| **bug-fix** | Reproduce → Fix → Regression Test | researcher, test-writer, implementer, reviewer | 3 |
| **research-spike** | Untersuchung → ADR | researcher, architect | 2 |
| **release** | Version → Tag → Publish | docs-writer, reviewer, performance-monitor | 2 |

**Entfernt / Integriert**:
- `consistency-sprint` → wird Phase in `feature-development` (nach GREEN)
- `content-review` → wird Modus von `feature-development` (wenn Typ = docs)
- `quality-baseline` → wird Modus von `project-start` (Rescue-Variante)

### D6: Packs vereinfachen (6 → 4)

| Pack | Workflow | Agents | Skills | Use Case |
|------|----------|--------|--------|----------|
| **fullstack-tdd** | feature-development | alle 15 | alle 17 | Standardentwicklung |
| **quick-fix** | bug-fix | 5 | 8 | Schnelle Fehlerbehebung |
| **project-kickstart** | project-start | 4 | 6 | Green-field + Brown-field |
| **research** | research-spike | 3 | 4 | Investigation + ADR |

---

## 3. Neues Agent Startup Protokoll

Jeder Agent führt beim Start aus:

```
1. GRAPH CONTEXT
   opencode-continue graph-memory context <scopeId> --depth 2
   → Injiziert: aktive Work-Items, letzte Entscheidungen, Learnings, Preferences
   → ~200 Tokens, ersetzt 10+ File-Reads

2. FALLBACK (wenn Graph-Command fehlschlägt)
   Read PROJECT.md
   Read ~/.linxmd/user-profile.md
   Read .linxmd/memory/decisions/ (relevant ADRs)

3. WORK CONTEXT
   Read zugewiesenen Kontext (Datei/Work-Item/Anfrage)
   Query: MATCH (n {id:$entityId})-[:BLOCKED_BY|DEPENDS_ON|RELATES_TO*1..2]->(dep)
          RETURN dep

4. WORK
   Agent führt seine Aufgabe aus

5. SESSION END
   Write Trace (.linxmd/traces/)
   opencode-continue graph-memory write-node → Neue Learnings/Decisions als Nodes
   opencode-continue graph-memory write-edge → Beziehungen speichern
```

---

## 4. Graph-Memory Patterns pro Agent

| Agent | Read (Startup) | Write (Session-End) |
|-------|----------------|---------------------|
| **product-manager** | Problemraum, Constraints, bestehende Entscheidungen | Product-Brief-Facts, Scope- und NFR-Entscheidungen |
| **router** | User-Prefs, aktive Tasks | Routing-Decision als Fact |
| **planner** | Dependencies, Blockers, Estimates-History | Task-Nodes, Dependency-Edges |
| **spec-writer** | Ähnliche Specs, bisherige Acceptance Patterns | Spec-Node, IMPLEMENTS-Edges |
| **architect** | Bestehende ADRs, Technology-Prefs, Superseded Decisions | Decision-Node, SUPERSEDES-Edges |
| **test-writer** | Coverage-Baselines, Test-Patterns pro Projekt | TestSuite-Node, COVERS-Edges |
| **implementer** | Blocked-By Chain, relevante ADRs, Coding-Prefs | Commit-Nodes, IMPLEMENTS-Edges |
| **reviewer** | Vergangene Review-Findings, Security-Patterns | Finding-Nodes, VIOLATES-Edges |
| **consistency-guardian** | Naming-Rules, Pattern-Standards | Violation-Nodes (auto-fixed vs. reported) |
| **docs-writer** | Aktuelle Docs-Struktur, Changelog-History | DocUpdate-Nodes |
| **researcher** | Vergangene Research zu ähnlichen Fragen | Research-Node, RELATES_TO-Edges |
| **onboarder** | Bestehende Projekt-Nodes, Tech-Stack | Project-Node, USES-Edges (Technologies) |
| **memory-distiller** | Unverarbeitete Traces | Learning-Nodes, Pattern-Nodes, Antipattern-Nodes |
| **drafter** | Tone/Stil-Prefs, vorherige Drafts | Draft-Node |
| **performance-monitor** | Baseline-Benchmarks, Trend-Daten | Benchmark-Nodes, REGRESSES/IMPROVES-Edges |

---

## 5. Roadmap — Einzelschritte

41 Artifacts, strikt sequentiell. Jeder Schritt = 1 Datei, 1 Review.

```
 SKILLS (17)          AGENTS (15)          WORKFLOWS (5)     PACKS (4)    META
 ─────────────        ──────────────       ──────────────     ──────────   ────
 ① graph              ⑱ product-manager    ㉝ project-start   ㊳ fullstack  ㊷ index.json
 ② graph-memory       ⑲ router             ㉞ feature-dev     ㊴ quick-fix
 ③ project-memory     ⑳ onboarder          ㉟ bug-fix         ㊵ kickstart
 ④ task-management    ㉑ planner            ㊱ research-spike  ㊶ research
 ⑤ trace-writing      ㉒ spec-writer        ㊲ release
 ⑥ conventional-c.    ㉓ architect
 ⑦ context-mgmt       ㉔ test-writer
 ⑧ debugging          ㉕ implementer
 ⑨ refactoring        ㉖ reviewer
 ⑩ api-design         ㉗ consistency-g.
 ⑪ observability      ㉘ docs-writer
 ⑫ consistency-chk    ㉙ researcher
 ⑬ perf-profiling     ㉚ memory-distiller
 ⑭ e2e-testing        ㉛ drafter
 ⑮ feature-branch     ㉜ perf-monitor
 ⑯ research
 ⑰ brainstorming
```

### Skills (1–17)

| # | Datei | Kurzbeschreibung |
|---|-------|-----------------|
| 1 | `skills/graph/SKILL.md` | CLI-only Interaktion mit `opencode-continue graph-memory` |
| 2 | `skills/graph-memory/SKILL.md` | Memory-Policy: was dauerhaft gespeichert, validiert, invalidiert wird |
| 3 | `skills/project-memory/SKILL.md` | Markdown-Memory (3 Tiers) als Fallback + Sync mit Graph |
| 4 | `skills/task-management/SKILL.md` | Task-Frontmatter v2, Backlog-Lifecycle, Graph-Task-Nodes |
| 5 | `skills/trace-writing/SKILL.md` | Session-Audit-Trail, Trace → Graph Sync am Session-Ende |
| 6 | `skills/conventional-commits/SKILL.md` | Commit-Format, Commit → Graph-Node Pattern |
| 7 | `skills/context-management/SKILL.md` | LLM-Kontext optimieren, Graph-first Loading |
| 8 | `skills/debugging/SKILL.md` | Reproduce → Isolate → Fix, Graph-Learnings schreiben |
| 9 | `skills/refactoring/SKILL.md` | Safe Transform, Pattern-Graph abgleichen |
| 10 | `skills/api-design/SKILL.md` | REST/HTTP Patterns, Naming, Status Codes, OpenAPI |
| 11 | `skills/observability/SKILL.md` | Structured Logging, Tracing, Health Endpoints |
| 12 | `skills/consistency-check/SKILL.md` | Naming, Dead Code, Import-Hygiene, Pattern-Compliance |
| 13 | `skills/performance-profiling/SKILL.md` | Benchmarks, Baselines, Regression-Detection, Graph-Trends |
| 14 | `skills/e2e-testing/SKILL.md` | Critical Path Tests, Playwright/Cypress, Test-Pyramide |
| 15 | `skills/feature-branch/SKILL.md` | Branch-Naming, PR-Discipline, Worktree-Integration |
| 16 | `skills/research/SKILL.md` | Systematische Recherche, Quellen, Comparison Matrix |
| 17 | `skills/brainstorming/SKILL.md` | HMW, SCAMPER, Ideation, Impact/Effort Ranking |

### Agents (18–32)

| # | Datei | Kurzbeschreibung |
|---|-------|-----------------|
| 18 | `agents/product-manager.md` | Stage-0 Product Shaping mit Scope, Erfolgskriterien, NFRs |
| 19 | `agents/router.md` | Klassifiziert Requests → Workflow-Zuweisung |
| 20 | `agents/onboarder.md` | Projekt-Analyse, PROJECT.md, .linxmd/ Setup (merged: +start-state-creator) |
| 21 | `agents/planner.md` | Specs → Tasks (1–4h), Dependencies, Estimates |
| 22 | `agents/spec-writer.md` | Ideen → strukturierte Specs mit Acceptance Criteria |
| 23 | `agents/architect.md` | ADRs, System-Design, Component-Boundaries |
| 24 | `agents/test-writer.md` | RED Phase — Failing Tests schreiben |
| 25 | `agents/implementer.md` | GREEN Phase — Tests grün machen, minimal, clean |
| 26 | `agents/reviewer.md` | Code+Spec Review, Security, Quality (merged: quality+spec) |
| 27 | `agents/consistency-guardian.md` | Post-Feature Sweep, Auto-Fix trivial, Report non-trivial |
| 28 | `agents/docs-writer.md` | README, API-Docs, Changelog (merged: +changelog-writer) |
| 29 | `agents/researcher.md` | Deep-Dive Investigation, Sourced Reports (merged: +fact-checker) |
| 30 | `agents/memory-distiller.md` | Traces → kompakte Learnings, ADRs, Patterns als Graph-Nodes |
| 31 | `agents/drafter.md` | First Drafts + Refinement (merged: +editor) |
| 32 | `agents/performance-monitor.md` | Benchmarks, Baseline-Vergleich, Regression-Detection |

### Workflows (33–37)

| # | Datei | Agents | Gates |
|---|-------|--------|-------|
| 33 | `workflows/project-start.md` | product-manager → onboarder → planner → reviewer | 2 |
| 34 | `workflows/feature-development.md` | product-manager → router → spec-writer → planner → architect → test-writer → implementer → consistency-guardian → reviewer → docs-writer | 5 |
| 35 | `workflows/bug-fix.md` | researcher → test-writer → implementer → reviewer | 3 |
| 36 | `workflows/research-spike.md` | product-manager → researcher → architect | 2 |
| 37 | `workflows/release.md` | docs-writer → reviewer → performance-monitor | 2 |

### Packs (38–41) + Meta

| # | Datei | Bündelt |
|---|-------|---------|
| 38 | `packs/fullstack-tdd.md` | feature-development + alle 15 Agents + alle 17 Skills |
| 39 | `packs/quick-fix.md` | bug-fix + 5 Agents + 8 Skills |
| 40 | `packs/project-kickstart.md` | project-start + 4 Agents + 6 Skills |
| 41 | `packs/research.md` | research-spike + 4 Agents + 4 Skills |
| 42 | `index.json` | Neubau für alle 41 Artifacts |

---

## 6. Dateistruktur (Ziel)

```
lib/
├── ARCHITECTURE.md           ← Aktualisiert für Continue-Integration
├── FRONTMATTER-SPEC.md       ← Unverändert (v2.1)
├── PRINCIPLES.md             ← Unverändert
├── README.md                 ← Aktualisiert (neue Counts, Continue-Referenz)
├── index.json                ← Neugebaut für 15+17+5+4 Artifacts
├── _backup_/                 ← Bisherige v2.0 Artifacts (read-only Referenz)
│
├── agents/                   ← 15 Agents (v3.0, graph-memory enabled)
│   ├── SPEC.md
│   ├── product-manager.md
│   ├── router.md
│   ├── planner.md
│   ├── spec-writer.md
│   ├── architect.md
│   ├── test-writer.md
│   ├── implementer.md
│   ├── reviewer.md           ← Merged: quality + spec
│   ├── consistency-guardian.md
│   ├── docs-writer.md        ← Merged: + changelog-writer
│   ├── researcher.md         ← Merged: + fact-checker
│   ├── onboarder.md          ← Merged: + start-state-creator
│   ├── memory-distiller.md
│   ├── drafter.md            ← Merged: + editor
│   └── performance-monitor.md
│
├── skills/                   ← 17 Skills (v3.0, graph-memory aware)
│   ├── SPEC.md
│   ├── graph/SKILL.md
│   ├── graph-memory/SKILL.md
│   ├── project-memory/SKILL.md
│   ├── task-management/SKILL.md
│   ├── trace-writing/SKILL.md
│   ├── conventional-commits/SKILL.md
│   ├── context-management/SKILL.md
│   ├── debugging/SKILL.md
│   ├── refactoring/SKILL.md
│   ├── api-design/SKILL.md
│   ├── observability/SKILL.md
│   ├── consistency-check/SKILL.md
│   ├── performance-profiling/SKILL.md
│   ├── e2e-testing/SKILL.md
│   ├── feature-branch/SKILL.md
│   ├── research/SKILL.md
│   └── brainstorming/SKILL.md
│
├── workflows/                ← 5 Workflows (v3.0, Memory-Lifecycle)
│   ├── SPEC.md
│   ├── project-start.md
│   ├── feature-development.md
│   ├── bug-fix.md
│   ├── research-spike.md
│   └── release.md
│
└── packs/                    ← 4 Packs (v3.0)
    ├── fullstack-tdd.md
    ├── quick-fix.md
    ├── project-kickstart.md
    └── research.md
```

---

## 7. Offene Fragen

| # | Frage | Auswirkung |
|---|-------|-----------|
| Q1 | Soll `memory-distiller` automatisch am Workflow-Ende laufen oder manuell getriggert werden? | Beeinflusst Workflow-Phasen |
| Q2 | Graph-Nodes versionieren (v1, v2) oder nur temporal invalidieren? | Beeinflusst graph-memory Skill |
| Q3 | Sollen Packs auch ohne Continue (ohne Graph) out-of-the-box funktionieren? | Beeinflusst graceful degradation Tiefe |
| Q4 | Brauchen wir einen `devops` Agent (CI/CD, Deployment) oder bleibt das out-of-scope? | Beeinflusst Agent-Count |
| Q5 | Soll der `router` Agent als MCP-Tool oder als Prompt-Instruction modelliert werden? | Beeinflusst Architektur |

---

## 8. Abgrenzung

**In Scope:**
- Alle Markdown-Artifacts (agents, skills, workflows, packs)
- graph-memory Integration in jedes Artifact
- Frontmatter v2.1 (kein Schema-Breaking-Change)
- index.json Rebuild

**Out of Scope:**
- linxmd CLI Änderungen (separate Aufgabe)
- Continue Backend-Änderungen (API existiert bereits)
- i18n/Design-Token/Market-Analysis Skills (on-demand, bei Bedarf nachrüstbar)
- MCP-Server Integration (separate Concerns)
