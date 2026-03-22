# Lib Improvement Plan

## Bestandsaufnahme

### Agents (6)

| Agent | Aufgabe | Qualität | Probleme |
|-------|---------|----------|----------|
| **implementer** | GREEN-Phase: minimaler Code bis Tests grün | ✅ Gut | Debugging-Sektion dupliziert `debugging` Skill |
| **test-writer** | RED-Phase: schreibt Failing Tests aus SPEC.md | ✅ Gut | — |
| **reviewer-spec** | Prüft ob Acceptance Criteria erfüllt sind | ✅ Gut | — |
| **reviewer-quality** | Prüft Code-Qualität und Security | ✅ Gut | — |
| **docs-writer** | Aktualisiert Docs nach Reviews | ⚠️ OK | Referenziert `TASKS.md` aber hat keine deps auf `task-management` |
| **echo-test** | No-Op für Smoke Tests | ✅ OK | Bleibt als Test-Fixture |

### Skills (6)

| Skill | Aufgabe | Qualität | Probleme |
|-------|---------|----------|----------|
| **task-management** | Backlog, SPEC, TASKS, Tracking | ✅ Gut | Kernstück, gut strukturiert |
| **feature** | Orchestriert SDD+TDD (SPEC → TASKS → Red/Green/Review/Docs/Commit) | ⚠️ Problematisch | Viel zu groß, dupliziert Workflow-Logik, Trigger-Konflikte mit task-management |
| **debugging** | Hypothesen-basiertes Debugging | ✅ Gut | Wird vom `implementer` dupliziert statt referenziert |
| **refactoring** | Sicheres Refactoring mit Tests | ⚠️ OK | Dünn, keine Agent-Referenzen |
| **preview-delivery** | Preview bauen, Feedback sammeln | ⚠️ OK | Tailscale-spezifisch, kein Agent zugeordnet |
| **echo-test** | No-Op für Smoke Tests | ✅ OK | Bleibt als Test-Fixture |

### Workflows (3)

| Workflow | Aufgabe | Qualität | Probleme |
|----------|---------|----------|----------|
| **sdd-tdd** | Kompletter SDD+TDD-Pipeline (5 Agents + 2 Skills) | ⚠️ OK | Dupliziert `feature` Skill fast 1:1, Getting Started zu generisch |
| **content-review** | Content-Erstellung mit Review-Pipeline | ❌ Schwach | Extrem dünn, keine Agents definiert, Pipeline nur angedeutet |
| **echo-test** | Test-Workflow für Dependency-Resolution | ✅ OK | Bleibt als Test-Fixture |

---

## Analyse: Hauptprobleme

### P1 — Dopplung: `feature` Skill ↔ `sdd-tdd` Workflow

`feature` Skill enthält die komplette Orchestrierungslogik (Pipeline, Modi, Task Loop, PR).
`sdd-tdd` Workflow beschreibt die gleiche Pipeline nochmal.
Wer orchestriert? Unklar. Beide sagen "für jeden Task: RED → GREEN → REVIEW → DOCS → COMMIT".

**Lösung:** Die Orchestrierung gehört in den Workflow. `feature` Skill wird aufgelöst oder stark reduziert.

### P2 — Dopplung: `implementer` Debugging ↔ `debugging` Skill

Der `implementer` Agent hat eine vollständige Debugging-Sektion (Hypothese → Fix → Verify).
Der `debugging` Skill beschreibt exakt das gleiche Pattern.

**Lösung:** `implementer` sollte auf debugging Skill verweisen, nicht copy-pasten.

### P3 — `content-review` Workflow ist zu dünn

4 Phasen (DRAFT → FACT-CHECK → EDIT → PUBLISH) aber keine Substanz:
- Keine Agents zugeordnet (wer drafted? wer fact-checked? wer editiert?)
- Keine konkreten Anweisungen pro Phase
- Kein Task-Format, kein Output-Format

### P4 — `feature` Skill ist zu groß und falsch kategorisiert

Ein Skill sollte Wissen/Techniken sein, kein Orchestrator.
`feature` enthält aber den gesamten Ablauf: Backlog finden → SPEC → TASKS → Loop → PR.
Das ist Workflow-Logik, kein Skill.

### P5 — Trigger-Konflikte

`feature` Skill: "Triggered by: lets do this, start, begin"
`task-management` Skill: "Start Feature — Triggered by: lets do this, start, begin"
Beide claimen exakt die gleichen Trigger.

### P6 — Fehlende Cross-Referenzen

- `docs-writer` nutzt TASKS.md-Struktur, hat aber keine dep auf `task-management`
- `refactoring` referenziert keine Agents
- `implementer` referenziert nicht den `debugging` Skill
- `preview-delivery` ist isoliert, kein Workflow nutzt ihn

---

## Verbesserungsplan

### Phase 1: Architektur bereinigen (Dopplungen eliminieren)

#### 1.1 — `feature` Skill auflösen

Der `feature` Skill wird gelöscht. Seine Inhalte gehen an die richtigen Stellen:

- **Orchestrierung** (Pipeline, Modi, Task Loop, PR) → wird Teil von `sdd-tdd` Workflow
- **"Find backlog item"** Logik → bereits in `task-management` Skill
- **Trigger-Definitionen** → gehören in die Workflows

Der `sdd-tdd` Workflow wird zum einzigen Ort für die Pipeline-Definition.

#### 1.2 — `sdd-tdd` Workflow anreichern

Der Workflow bekommt die volle Substanz aus `feature`:

- Detaillierte Pipeline-Beschreibung pro Phase (RED/GREEN/REVIEW/DOCS/COMMIT)
- Modi (autonomous vs. guided) mit konkreten Beschreibungen
- Stop-Conditions und Eskalationsregeln
- Task-Loop-Logik
- PR-Erstellung am Ende

#### 1.3 — `implementer` ↔ `debugging` Skill verbinden

- `implementer`: Debugging-Sektion entfernen, stattdessen: "When tests fail → apply debugging skill"
- `debugging` Skill als Referenz, nicht als Kopie

### Phase 2: Content-Review Workflow ausbauen

#### 2.1 — Neue Agents für Content-Pipeline

| Agent | Aufgabe |
|-------|---------|
| **drafter** | Erstellt den initialen Content-Entwurf basierend auf SPEC |
| **fact-checker** | Prüft alle Fakten, Claims, Zahlen, Links |
| **editor** | Verbessert Sprache, Stil, Struktur, Konsistenz |

#### 2.2 — `content-review` Workflow anreichern

- Jede Phase bekommt einen zugeordneten Agent
- Konkrete Input/Output-Formate pro Phase
- Modi (autonomous/guided) wie bei sdd-tdd
- Task-Format für Content-Items

### Phase 3: Bestehende Artifacts verbessern

#### 3.1 — `docs-writer` Agent

- dep auf `task-management` hinzufügen (nutzt ja TASKS.md)
- Konkretere Anweisungen: welche Sektionen einer Doku aktualisiert werden
- Prüfung ob Links/Referenzen noch stimmen

#### 3.2 — `refactoring` Skill

- Referenz auf `implementer` (nutzt gleiche Run-Tests-Logik)
- Refactoring-Patterns besser beschreiben (Extract, Rename etc. sind zu kurz)
- Commit-Message-Format definieren
- Verbindung zum `reviewer-quality` Agent (REFACTOR-Ergebnisse)

#### 3.3 — `debugging` Skill

- Mehr konkrete Strategien: Stack-Trace-Analyse, Bisect, Isolation
- Tool-Referenzen: Debugger, Logging, Breakpoints
- Integration in Workflow: wann wird Debugging ausgelöst?

#### 3.4 — `preview-delivery` Skill

- Tailscale-Referenz generalisieren (beliebiger Tunnel / Hosting)
- Integration in einen Workflow (z.B. als optionale Phase in sdd-tdd nach QUALITY-REVIEW)
- Feedback-Format konkretisieren

#### 3.5 — `reviewer-quality` und `reviewer-spec` Agents

- Checklisten erweitern (Performance, Accessibility, i18n)
- Severity-Levels klarer definieren (BLOCKER vs WARNING Kriterien)

#### 3.6 — `task-management` Skill

- Trigger-Sektion bereinigen (keine Orchestrierung, nur Struktur)
- "Start Feature" Sektion entfernen (gehört in Workflow)
- Fokus: Strukturdefinition (Ordner, Dateiformate, Status-Tracking)

### Phase 4: Index und Deps aktualisieren

- `index.json` an neue/gelöschte Artifacts anpassen
- Alle deps prüfen und korrigieren
- Tags vereinheitlichen
- Versions auf 0.1.0 bumpen (erster "richtiger" Release der Lib)

---

## Ergebnis-Architektur (Ziel)

```
agents/
  docs-writer.md          ← verbessert (+ dep task-management)
  drafter.md              ← NEU (Content-Pipeline)
  echo-test.md            ← unverändert (Test-Fixture)
  editor.md               ← NEU (Content-Pipeline)
  fact-checker.md         ← NEU (Content-Pipeline)
  implementer.md          ← verbessert (Debugging → Skill-Referenz)
  reviewer-quality.md     ← verbessert (erweiterte Checkliste)
  reviewer-spec.md        ← verbessert (erweiterte Checkliste)
  test-writer.md          ← unverändert (bereits gut)

skills/
  debugging/SKILL.md      ← verbessert (+ Strategien)
  echo-test/              ← unverändert (Test-Fixture)
  preview-delivery/       ← verbessert (generalisiert)
  refactoring/SKILL.md    ← verbessert (+ Agent-Referenzen)
  task-management/SKILL.md ← bereinigt (nur Struktur, keine Orchestrierung)

workflows/
  content-review.md       ← komplett überarbeitet (3 Agents, volle Pipeline)
  echo-test.md            ← unverändert (Test-Fixture)
  sdd-tdd.md              ← angereichert (volle Pipeline aus feature Skill)
```

**Gelöscht:** `skills/feature/` (aufgelöst in sdd-tdd Workflow + task-management)

---

## Reihenfolge

| # | Aufgabe | Abhängigkeit |
|---|---------|-------------|
| 1 | `task-management` Skill bereinigen | — |
| 2 | `sdd-tdd` Workflow anreichern (aus feature übernehmen) | 1 |
| 3 | `feature` Skill löschen + index.json anpassen | 2 |
| 4 | `implementer` → debugging Referenz | — |
| 5 | `docs-writer` deps + Inhalt verbessern | — |
| 6 | `debugging` Skill erweitern | — |
| 7 | `refactoring` Skill erweitern | — |
| 8 | `preview-delivery` generalisieren | — |
| 9 | Content-Agents erstellen (drafter, fact-checker, editor) | — |
| 10 | `content-review` Workflow komplett überarbeiten | 9 |
| 11 | `reviewer-quality` + `reviewer-spec` erweitern | — |
| 12 | `index.json` final aktualisieren, Versions bump | Alle |
