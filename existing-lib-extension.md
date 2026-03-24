# Existing Lib Extension Plan

## Ziel
Bestehende Artifacts in `lib/` um praxisnahe `quickActions` erweitern, mit starkem Fokus auf Markdown-Kontexte in:

- `.linxmd/specs/*.md`
- `.linxmd/tasks/backlog/*.md`
- `.linxmd/tasks/in-progress/**/(SPEC|TASKS|NOTES).md`
- `PROJECT.md`

Die Actions sollen kurze, wiederkehrende Arbeitsschritte beschleunigen (spec review, task breakdown, test plan, quality check, release prep).

Neu hinzugefuegt:

- Inbox-Ansatz fuer Brain-Dump (`.linxmd/tasks/inbox.md`)
- Assistant-first QuickActions (ausarbeiten, schoener schreiben, vorschlaege, recherchieren, backlog)
- Trigger-Strategie nach Ort + Marker (nicht nur Dateipfad)

## Leitregeln

1. `fileMatch` ist immer gesetzt (non-empty list).
2. Alle anderen Trigger sind optional.
3. QuickActions sind eng auf Artifact-Rolle zugeschnitten.
4. Prompts sind handlungsorientiert und liefern ein klares Output-Format.
5. Start mit einem kleinen, hochwertigen Kernset; dann iterativ erweitern.
6. Standard-Regel fuer Trigger: Ort + 1 Marker (max. 2-3 Bedingungen insgesamt).
7. Keine Action darf Roh-Notizen ueberschreiben; strukturierte Ergebnisse werden angehaengt.

## Gemeinsame Trigger-Bausteine

```yaml
fileMatch:
  - "^\\.linxmd/specs/.*\\.md$"
  - "^\\.linxmd/tasks/backlog/.*\\.md$"
  - "^\\.linxmd/tasks/in-progress/.*/(SPEC|TASKS|NOTES)\\.md$"
  - "^PROJECT\\.md$"
```

Optionale Verfeinerungen:

```yaml
fileExclude:
  - "^CHANGELOG\\.md$"
workspaceHas:
  - ".linxmd"
languageId:
  - "markdown"
contentMatch:
  - "Acceptance Criteria|Akzeptanzkriterien|TASK-"
```

## Trigger-Strategie fuer Markdown-Kontexte

QuickActions sind primär fuer Markdown sinnvoll. Deshalb triggern wir in zwei Stufen:

1. Ort (Dateipfad) bestimmt den Arbeitsraum.
2. Marker (contentMatch) bestimmt den Dokumenttyp innerhalb des Arbeitsraums.

Beispiele fuer Marker:

- Brain-Dump: `## Brain Dump|## Inbox|^[-*] `
- Task-Liste: `- \[ \]|- \[x\]|## Tasks`
- Offene Fragen: `## Open Questions|\?\s*$`
- Risiken: `## Risks|Risk|Annahme|Assumption`
- Entscheidung: `## Decision|ADR|Trade-off`

Faustregel gegen Overengineering:

- Pro QuickAction maximal: `fileMatch` + genau 1 zusaetzlicher Marker.
- Erst wenn echte Fehltrigger auftreten, weitere Bedingungen ergaenzen.

## Inbox-Konzept fuer Task-Workflow

Neue Datei als dauerhaftes Brain-Dump-Ziel:

- `.linxmd/tasks/inbox.md` (initial leer)

Ziel:

- Ideen schnell erfassen
- dann per QuickAction in strukturierte Artefakte umwandeln

### Inbox QuickActions (Assistant-first)

1. `inbox-work-out`
- Label: "Look here: Arbeite aus"
- Prompt: Rohnotizen in eine klare, strukturierte Fassung ueberfuehren.

2. `inbox-write-better`
- Label: "Look here: Schreibe schoener"
- Prompt: Stil, Klarheit, Lesbarkeit verbessern, Bedeutung beibehalten.

3. `inbox-suggest`
- Label: "Look here: Mache Vorschlaege"
- Prompt: 3-7 konkrete Optionen mit Vor-/Nachteilen.

4. `inbox-research-web`
- Label: "Look here: Recherchiere im Internet"
- Prompt: offene Punkte recherchieren und mit Quellenhinweisen zusammenfassen.

5. `inbox-to-backlog`
- Label: "Look here: Erstelle Backlog Eintraege"
- Prompt: Brain-Dump in priorisierte Backlog-Kandidaten mit Akzeptanzkriterien umwandeln.

6. `inbox-open-questions`
- Label: "Look here: Klaere offene Fragen"
- Prompt: fehlende Informationen und Rueckfragenliste erstellen.

7. `inbox-next-three-steps`
- Label: "Look here: Naechste 3 Schritte"
- Prompt: sofort umsetzbare naechste Schritte fuer heute.

8. `inbox-risks-assumptions`
- Label: "Look here: Risiken und Annahmen"
- Prompt: Risiken/Annahmen extrahieren und Gegenmassnahmen vorschlagen.

Empfohlener Trigger fuer Inbox Actions:

```yaml
trigger:
  fileMatch:
    - "^\\.linxmd/tasks/inbox\\.md$"
  languageId:
    - "markdown"
  workspaceHas:
    - ".linxmd/tasks"
```

## Vorschlaege: Workflows

### workflow:feature-development

1. `qa-spec-gap-check`
- Label: "Spec Gap Check"
- fileMatch: specs + in-progress/SPEC.md
- Prompt: Pruefe Vollstaendigkeit der Akzeptanzkriterien, identifiziere Luecken, erstelle konkrete Nachfragen.

2. `qa-task-split`
- Label: "Split Into Task Units"
- fileMatch: in-progress/TASKS.md
- Prompt: Zerlege in 1-4h Tasks mit Dependencies, Risiko und Test-Hinweis je Task.

3. `qa-ready-for-impl`
- Label: "Ready for Implementer"
- fileMatch: in-progress/(SPEC|TASKS).md
- Prompt: Gib Go/No-Go mit Begruendung und fehlenden Punkten fuer Implementierung.

### workflow:bug-fix

1. `qa-repro-template`
- Label: "Create Repro Steps"
- fileMatch: in-progress/NOTES.md
- Prompt: Erstelle reproduzierbare Schritte, erwartetes vs. aktuelles Verhalten, betroffene Komponenten.

2. `qa-regression-tests`
- Label: "Regression Test Scope"
- fileMatch: in-progress/SPEC.md
- Prompt: Liste Regressionstestfaelle und priorisiere sie nach Risiko.

### workflow:project-start

1. `qa-project-md-audit`
- Label: "Audit PROJECT.md"
- fileMatch: PROJECT.md
- Prompt: Pruefe fehlende Architektur-, Stack-, Konventions- und Team-Informationen.

2. `qa-backlog-seed`
- Label: "Seed Backlog Items"
- fileMatch: tasks/backlog/*.md
- Prompt: Erzeuge konsistente initiale Task-Eintraege inkl. Prioritaet und Akzeptanz.

### workflow:research-spike

1. `qa-research-question`
- Label: "Sharpen Research Question"
- fileMatch: specs/*.md
- Prompt: Formuliere Scope, Hypothesen, Abbruchkriterien und Entscheidungsvorlage.

2. `qa-decision-brief`
- Label: "Decision Brief"
- fileMatch: in-progress/NOTES.md
- Prompt: Verdichte Erkenntnisse in Optionen, Trade-offs und klare Empfehlung.

### workflow:quality-baseline

1. `qa-quality-checklist`
- Label: "Quality Checklist"
- fileMatch: specs/*.md + tasks/in-progress/**/NOTES.md
- Prompt: Erzeuge Checkliste fuer Tests, Security, Performance, Docs.

2. `qa-risk-register`
- Label: "Risk Register"
- fileMatch: in-progress/NOTES.md
- Prompt: Erstelle Risikoregister mit Wahrscheinlichkeit, Impact, Gegenmassnahmen.

### workflow:release

1. `qa-release-readiness`
- Label: "Release Readiness"
- fileMatch: PROJECT.md + tasks/in-progress/**/NOTES.md
- Prompt: Bewerte Release Readiness inkl. offenen Gates und Blockern.

2. `qa-release-notes-draft`
- Label: "Draft Release Notes"
- fileMatch: CHANGELOG.md
- Prompt: Erzeuge strukturierten Release-Notes-Entwurf aus aenderungsrelevanten Tasks.

### workflow:consistency-sprint

1. `qa-consistency-pass`
- Label: "Consistency Pass"
- fileMatch: specs/*.md + tasks/**/*.md
- Prompt: Finde Terminologie-, Status- und Strukturinkonsistenzen und schlage Fixes vor.

### workflow:content-review

1. `qa-content-fact-check-queue`
- Label: "Fact Check Queue"
- fileMatch: specs/*.md + NOTES.md
- Prompt: Markiere verifizierungsbeduerftige Aussagen und priorisiere nach Risiko.

## Vorschlaege: Agents

### agent:spec-writer

1. `qa-spec-acceptance-criteria`
- Label: "Improve Acceptance Criteria"
- fileMatch: specs/*.md
- Prompt: Mache Kriterien testbar, messbar, widerspruchsfrei.

2. `qa-spec-edge-cases`
- Label: "Add Edge Cases"
- fileMatch: specs/*.md
- Prompt: Ergaenze Randfaelle, Fehlerfaelle, Abbruchpfade.

### agent:planner

1. `qa-plan-estimates`
- Label: "Validate Estimates"
- fileMatch: in-progress/TASKS.md
- Prompt: Pruefe Aufwandsschaetzungen, splitte oversized tasks.

2. `qa-plan-dependencies`
- Label: "Dependency Sanity"
- fileMatch: in-progress/TASKS.md
- Prompt: Pruefe Reihenfolge und dependency graph auf Konflikte.

### agent:test-writer

1. `qa-test-plan-from-spec`
- Label: "Generate Test Plan"
- fileMatch: specs/*.md
- Prompt: Erzeuge Testmatrix (happy path, edge, error).

2. `qa-missing-tests`
- Label: "Find Missing Tests"
- fileMatch: in-progress/SPEC.md
- Prompt: Liste ungetestete Akzeptanzkriterien.

### agent:implementer

1. `qa-impl-checklist`
- Label: "Implementation Checklist"
- fileMatch: in-progress/(SPEC|TASKS).md
- Prompt: Liefere konkrete Umsetzungs-Checkliste inkl. Reihenfolge.

### agent:reviewer-spec

1. `qa-spec-verdict`
- Label: "Spec Review Verdict"
- fileMatch: specs/*.md
- Prompt: Vergib PASS/CHANGES_REQUIRED mit konkreten Diff-Vorschlaegen.

### agent:reviewer-quality

1. `qa-quality-verdict`
- Label: "Quality Review Verdict"
- fileMatch: in-progress/NOTES.md
- Prompt: Bewerte Testabdeckung, Sicherheit, Wartbarkeit; klare Restaufgaben.

### agent:consistency-guardian

1. `qa-frontmatter-consistency`
- Label: "Frontmatter Consistency"
- fileMatch: tasks/**/*.md
- Prompt: Pruefe Schema-Konformitaet und Status-Konsistenz.

### agent:docs-writer

1. `qa-doc-update-plan`
- Label: "Documentation Update Plan"
- fileMatch: PROJECT.md + specs/*.md
- Prompt: Liste noetige Doku-Updates mit Prioritaet.

### agent:changelog-writer

1. `qa-changelog-entry`
- Label: "Generate Changelog Entry"
- fileMatch: CHANGELOG.md + tasks/in-progress/**/NOTES.md
- Prompt: Erzeuge changelog-konforme Eintraege aus umgesetzten Aenderungen.

### agent:architect

1. `qa-adr-candidate`
- Label: "ADR Candidate Check"
- fileMatch: specs/*.md + NOTES.md
- Prompt: Erkenne ADR-pflichtige Entscheidungen inkl. Entscheidungsoptionen.

## Vorschlaege: Skills ("actions")

### skill:task-management

1. `qa-task-frontmatter-fix`
- Label: "Fix Task Frontmatter"
- fileMatch: tasks/**/*.md
- Prompt: Korrigiere Frontmatter nach Schema und nenne Unsicherheiten.

2. `qa-task-state-transition`
- Label: "Validate State Transition"
- fileMatch: tasks/**/*.md
- Prompt: Pruefe erlaubte Statusuebergaenge und Konflikte.

### skill:debugging

1. `qa-debug-hypothesis-table`
- Label: "Hypothesis Table"
- fileMatch: in-progress/NOTES.md
- Prompt: Erstelle Hypothesenliste mit Experiment, Ergebnis, naechstem Schritt.

### skill:refactoring

1. `qa-refactor-scope`
- Label: "Refactor Scope Check"
- fileMatch: specs/*.md + TASKS.md
- Prompt: Grenze Refactor-Scope ab, vermeide unbeabsichtigte Verhaltensaenderung.

### skill:api-design

1. `qa-api-contract-review`
- Label: "API Contract Review"
- fileMatch: specs/*.md
- Prompt: Pruefe Konsistenz von Endpunkten, Payloads, Fehlercodes.

### skill:project-memory

1. `qa-memory-entry`
- Label: "Record Decision"
- fileMatch: NOTES.md + PROJECT.md
- Prompt: Extrahiere dauerhafte Learnings und Entscheidungsprotokoll.

### skill:consistency-check

1. `qa-consistency-batch`
- Label: "Batch Consistency Check"
- fileMatch: specs/*.md + tasks/**/*.md
- Prompt: Finde inkonsistente Begriffe, IDs, Status, Referenzen.

### skill:conventional-commits

1. `qa-commit-message-from-task`
- Label: "Commit Message from Task"
- fileMatch: tasks/in-progress/**/NOTES.md
- Prompt: Formuliere Conventional Commit aus der dokumentierten Aenderung.

## Rollout-Plan

### Phase 1 (MVP, 2-3 Tage)

- Inbox pilot: 5 Assistant-first Inbox QuickActions
- 2 QuickActions je Kern-Workflow: `feature-development`, `bug-fix`, `project-start`
- 2 QuickActions je Kern-Agent: `spec-writer`, `planner`, `test-writer`, `reviewer-spec`
- 2 QuickActions fuer `task-management`

Ziel: Hoher Nutzen bei minimalem Risiko.

### Phase 2 (Erweiterung, 3-5 Tage)

- restliche Inbox QuickActions (falls im Pilot nicht enthalten)
- Restliche Workflows
- Restliche Core/Control Agents
- Skills: `debugging`, `refactoring`, `api-design`, `project-memory`, `consistency-check`

### Phase 3 (Feinschliff)

- Trigger-Feintuning pro Repo-Konvention
- Marker-Feintuning (Ort + Marker pro Action validieren)
- Prompt-Qualitaet per Nutzerfeedback optimieren
- Duplikate zusammenfuehren

## Akzeptanzkriterien fuer diese Extension

1. Jede QuickAction hat non-empty `fileMatch`.
2. Keine QuickAction triggert auf irrelevante Nicht-Markdown-Dateien.
3. Kern-Workflows liefern fuer Plan/Spec/Task-Dateien sinnvolle Vorschlaege.
4. Parser/Unit-Tests decken Positiv- und Negativfaelle ab (`fileMatch` fehlt => invalid).
5. E2E-Suite bleibt gruen.
6. Inbox-QuickActions bewahren Rohnotizen und haengen strukturierte Ergebnisse an.
7. Trigger-Komplexitaet bleibt schlank (Default: Ort + 1 Marker).

## Teststrategie

1. Unit: FrontmatterParser
- valid quickAction mit `fileMatch` list
- invalid quickAction ohne `fileMatch`
- optionale Trigger fehlen -> weiterhin valid
- `contentMatch` Marker-Varianten fuer Markdown werden korrekt eingelesen

2. E2E: CLI
- bestehende Flows (`add`, `remove`, `list`, `sync`, `update`) unveraendert gruen
- mindestens ein lokaler Source-Test fuer dependency-blocking behalten

3. Regression
- keine Aenderung am Verhalten ohne quickActions/lifecycle
- bestehende Artifacts ohne neue Felder bleiben installierbar
- Inbox-Datei kann leer bleiben; QuickActions funktionieren trotzdem robust
