# Linxmd - Strategic Plan (2026-03)

## Decision Lock-In (vom User bestaetigt)

1. Keine neue Ordnerstruktur
2. Option A
3. Option A
4. Option A
5. Option A
6. Option A
7. Option B (`--type` kann entfallen)
8. Option B
9. Coverage-Overlay in VS Code muss funktionieren
10. Option B
11. Option B
12. Neuer Name: sehr kurz, 2 Silben, helper-branding

---

## Tool-Standard fuer Referenzen (GitHub + Claude + OpenCode)

Ergebnis aus Doku-Check:
- Es gibt keinen gemeinsamen, universellen `@skill:<name>` Standard in Artefakt-Inhalten.
- Claude Code unterstuetzt `@path/to/file` Importe in `CLAUDE.md`.
- OpenCode unterstuetzt externe Instruktionen ueber `opencode.json` (`instructions`) und manuelle `@file`-Konventionen in `AGENTS.md`.
- Copilot Skills folgen Agent-Skills-Standard (SKILL.md mit Frontmatter), aber kein universelles Inline-`@skill`-Protokoll.

Daher verbindliche Empfehlung fuer dieses Repo:
- Maschinenlesbar in Index/Deps: `type:name` (z.B. `skill:task-management`, `workflow:sdd-tdd`).
- Menschlich in Markdown: relative Markdown-Links auf Artefaktdateien.
- Optionaler Alias in Text: `@skill/task-management`, `@agent/reviewer-spec` nur als lesbarer Hint, nicht als Parser-Quelle.
- Parser/CLI sollen nur `type:name` (und CLI-Syntax `type:name`) als kanonisch behandeln.

## Zielbild

Die Library soll aus autonomen, wiederverwendbaren Bausteinen bestehen (Agent, Skill, Workflow),
mit klarer Separation of Concerns, stabiler CLI, sauberer Dependency-Sicherheit und guter DX.

---

## Batch-Board zum Abarbeiten

Statusregel:
- Ein Batch gilt erst als erledigt, wenn alle Punkte und alle Abnahmepunkte abgehakt sind.

### Batch 1 - CLI-Kern finalisieren
- [x] Deprecated Commands entfernen (`agent|skill|workflow` Trees raus)
- [x] Typed IDs als Standard einfuehren (`add typ:name`, `remove typ:name`)
- [x] `--type` Option entfernen
- [x] Uninstall mit Reverse-Dependency-Blockierung (safe default)
- [x] Tests fuer obige Punkte aktualisieren

Abnahme Batch 1
- [x] Help zeigt nur neue Kommandostruktur
- [x] Alte Commands sind entfernt
- [x] Uninstall blockiert korrekt bei Abhaengigkeiten
- [x] Tests gruen

### Batch 2 - Multi-Source + Update-Logik
- [x] `.linxmd/sources.json` einfuehren
- [x] Install speichert source metadata in `installed.json`
- [x] Update prueft primaer origin source
- [x] Fallback-Regeln bei nicht erreichbarer source
- [x] Unit- und E2E-Tests fuer source/update erweitern

Abnahme Batch 2
- [x] Install aus alternativer source funktioniert
- [x] Update bleibt source-treu
- [x] Fehlertexte bei offline source sind klar
- [x] Tests gruen

### Batch 3 - Library-Inhalte
- [x] Alle Agent/Skill/Workflow Versionen auf `0.0.1`
- [x] Namen vereinheitlichen gemaess Konvention
- [x] Generator-Workflow (Option A) implementieren
- [x] Skill `preview-delivery` implementieren
- [x] Referenzen in Markdown auf kanonische IDs umstellen (`type:name`)

Abnahme Batch 3
- [x] `lib/index.json` konsistent
- [x] Artefakte installierbar
- [x] Keine Namenskollisionen
- [x] Tests gruen

### Batch 4 - Produktisierung
- [x] README auf realen CLI-Stand + modernes Layout
- [x] Update-Check beim Start (async, cached, opt-out)
- [x] Coverage-UX in VS Code stabil (Gutter/Tree sichtbar)
- [x] Rebranding-Spuren auf Linxmd finalisieren

Abnahme Batch 4
- [x] README und CLI sind konsistent
- [x] Update-Hinweis funktioniert ohne spuerbaren Startup-Delay
- [x] Coverage sichtbar in VS Code
- [x] Release-ready Check bestanden

---

## Entscheidungen pro Anforderung (Optionen + Empfehlung)

### 1) Lib ueberarbeiten (Namen, Referenzen, Baustein-Prinzip)

Option A: Flat by type (heute) mit besseren Namen
- Pros: Wenig Migration
- Cons: Architektur bleibt implizit

Option B: Capability-first Struktur
- Beispiel: `lib/capabilities/testing/{agents,skills,workflows}`
- Pros: Bessere Domainenavigation
- Cons: Hoeherer Migrationsaufwand

Option C: Hybrid (empfohlen)
- Behalte `agents/`, `skills/`, `workflows/`
- Fuege verpflichtende Metadaten ein: `purpose`, `inputs`, `outputs`, `constraints`, `compatible_with`
- Erzwinge Referenzen nur ueber `type:name` (z.B. `skill:task-management`)
- Definiere Namenskonvention: `<domain>-<intent>` (z.B. `code-reviewer-quality`, `workflow-content-review`)

Entscheidung: Option A (keine neue Ordnerstruktur)

Konkrete Umsetzung innerhalb der bestehenden Struktur:
- Namenskonvention vereinheitlichen:
  - Agents: `<verb>-<focus>` (z.B. `review-spec`, `write-tests`)
  - Skills: `<domain>-<capability>` (z.B. `task-planning`, `preview-delivery`)
  - Workflows: `<outcome>-<flow>` (z.B. `ship-feature`, `content-review`)
- Referenzen in Markdown auf kanonische IDs und Links umstellen (siehe Tool-Standard oben).

### 2) Alle Lib-Versionen auf 0.0.1

Option A: Alles auf `0.0.1`
- Pros: Einheitlich fuer fruehe Experimentierphase
- Cons: Keine Reife-Signale, kaum differenzierte Upgrade-Kommunikation

Option B: SemVer nach Reifegrad (empfohlen)
- Core/produktive Artefakte bleiben >= `1.0.0`
- Experimentelle Artefakte: `0.x`

Option C: Doppelte Versionierung
- `artifact_version` + `stability` (`experimental|stable`)

Entscheidung: Option A
- Alle Agents/Skills/Workflows auf `0.0.1` setzen.
- Danach eine Policy in der README dokumentieren, wann auf `0.0.2+` erhoeht wird.

### 3) Workflow/Skill/Agent fuer die Generierung von Workflows/Skills/Agents

Option A: Nur ein Workflow `builder-suite`
- Pros: Einfach
- Cons: Monolithisch

Option B: Drei getrennte Artefakte (empfohlen)
- Agent: `lib-builder`
- Skill: `artifact-authoring`
- Workflow: `artifact-factory`

Entscheidung: Option A
- Ein einzelner Workflow als Startpunkt (z.B. `artifact-factory`) mit eingebetteten Schritten.
- Spaeter optional in dedizierte Bausteine aufteilen, falls Wiederverwendungsmuster stabil sind.

### 4) Skill fuer Previews (Tailscale, binaries senden, Feedback loop)

Option A: Ein Skill `preview-delivery` (empfohlen)
- Schritte: Build -> Publish preview -> Share link/binary -> Collect feedback -> Decide iterate/release
- Adapter per Tool: Tailscale Funnel / GitHub Release asset / SCP

Option B: Zwei Skills
- `preview-web` und `preview-binary`

Entscheidung: Option A
- Ein Skill `preview-delivery` (Web + Binary + Feedback-Loop).

### 5) Deprecated Commands entfernen

Option A: Sofort entfernen (breaking)

Option B: Soft window + removal (empfohlen)
- v0.3: nur Warnung
- v0.4: harte Entfernung

Entscheidung: Option A
- Alte `agent|skill|workflow` Command-Trees sofort entfernen.
- Migration-Hinweise nur in Release Notes/README.

### 6) Uninstall bei Abhaengigkeiten + Testlage

Ist-Stand
- Uninstall entfernt aktuell direkt das Zielartefakt; Reverse-Dependency-Schutz fehlt.

Option A: Blockieren bei Referenzen (empfohlen)
- Wenn etwas noch von anderem installiertem Artefakt benoetigt wird: Fehler + Liste der Blocker

Option B: `--force` und `--cascade`
- `--force`: ignoriert Schutz
- `--cascade`: entfernt abh. Kette topologisch

Entscheidung: Option A
- Default: blockieren, wenn Reverse-Dependencies existieren.
- In diesem Schritt noch kein `--force`/`--cascade`.

### 7) Type-qualified Install/Uninstall (z.B. `workflow:sdd-tdd`)

Option A: Nur `--type`

Option B: Prefix-Notation + `--type` (empfohlen)
- Unterstuetze:
  - `linxmd add workflow:sdd-tdd --yes`
  - `linxmd remove skill:feature --yes`
- Behalte `--type` als Alternative

Entscheidung: Option B
- Prefix-Notation wird primaer.
- `--type` wird entfernt.

### 8) Source-Feld + externe Repos/Sources fuer Update

Option A: Nur globales Standard-Repo

Option B: Multi-source registry (empfohlen)
- Neue Datei: `.linxmd/sources.json`
- Beispielquellen: GitHub repo index, raw URL, lokaler Ordner, private mirror
- In `installed.json` speichere pro Artefakt: `sourceId`, `sourcePath`, `checksum`
- `update` prueft zuerst die origin source, dann fallback rules

Entscheidung: Option B

### 9) VS Code Coverage Anzeige ("Batterie")

Option A: Nur CLI report im Terminal

Option B: Coverage in VS Code (empfohlen)
- Extension: Coverage Gutters
- Tooling: `coverlet` + `reportgenerator`
- Task: `test:coverage` erzeugt Cobertura + HTML

Entscheidung: Option B

### 10) README modernisieren (2026 style)

Option A: Minimal text update

Option B: Volles Redesign (empfohlen)
- Neue Story: "Package manager for AI workflow building blocks"
- Moderne Sektionen: Why, 60s Quickstart, Architecture, Typed commands, Sources, Safety, Examples, Roadmap
- GIF/screenshot der interaktiven CLI

Entscheidung: Option B

### 11) CLI Update Check beim Start

Option A: Immer synchron checken
- Cons: Langsamer Start

Option B: Asynchron + gecached (empfohlen)
- Max alle 24h
- Timeout 500ms
- Opt-out env/config: `LINXMD_NO_UPDATE_CHECK=1`
- Hinweis: "New version available: x.y.z - run ..."

Entscheidung: Option B

### 12) Neuer Name statt "linxmd"

Optionen
- `agentblocks`
- `flowblocks`
- `agentforge`
- `orchestragent`
- `workflowkit`

Neue Namensrichtung (kurz, 2 Silben, helper-branding):
- `Navi`
- `Kumo`
- `Lynx`
- `Ibis`
- `Mika`
- `Taro`
- `Odin`
- `Sage`

Entscheidung: Linxmd
- kurz, eindeutig, helper-charakter.
- passt semantisch zu "links" zwischen Agents/Skills/Workflows.

---

## Umsetzungsplan (Phasen)

## Phase 0 - Design Freeze (1-2 Tage)
- Definiere Metadata-Schema fuer Artefakte
- Definiere Source Registry Schema
- Definiere Dependency Safety Verhalten (`block|force|cascade`)

Exit-Kriterien
- JSON-Schema fuer `index.json`, `installed.json`, `sources.json` steht
- CLI UX-Entscheidung fuer Type-prefix steht

## Phase 1 - Safety + Typed IDs (2-3 Tage)
- Parser fuer `type:name`
- Reverse dependency graph im State
- `remove` mit Blocker-Hinweis
- `remove --cascade`, `remove --force`

Tests
- Unit: graph build, cycle detection, blocker detection
- E2E: uninstall blocked, uninstall cascade, uninstall force

Exit-Kriterien
- Kein versehentliches Brechen durch Standard-Uninstall

## Phase 2 - Multi-source Registry (3-4 Tage)
- `.linxmd/sources.json` einfuehren
- install speichert source metadata
- update nutzt origin source
- source fallback + clear errors

Tests
- Unit: source resolution
- E2E: install from alt source, update from same source, source offline fallback

Exit-Kriterien
- Artefakte koennen aus unterschiedlichen Quellen installiert + aktualisiert werden

## Phase 3 - Deprecated Removal + CLI Polish (1-2 Tage)
- `agent|skill|workflow` trees entfernen
- help/usage anpassen
- migration notes in changelog

Tests
- E2E: alte Commands failen mit klarer Migration-Message

Exit-Kriterien
- Nur neue 7-Kommandostruktur aktiv

## Phase 4 - Library Productization (2-3 Tage)
- Versionspolicy umsetzen
- Neue Meta-Artefakte:
  - agent `lib-builder`
  - skill `artifact-authoring`
  - workflow `artifact-factory`
- Neuer Skill `preview-delivery`

Tests
- Content lint tests fuer frontmatter completeness
- E2E install tests fuer neue Artefakte

Exit-Kriterien
- Library ist modular und selbst-erweiterbar

## Phase 5 - README + Coverage UX + Update notifier (2 Tage)
- README komplett neu
- Coverage task + VS Code setup docs
- update check (cached async)

Tests
- Unit: update check cache + timeout
- E2E smoke: notifier message bei neuer Version

Exit-Kriterien
- Onboarding + Wartung + QA sichtbar modernisiert

---

## Empfohlene Reihenfolge (kurz)
1. Safety/Dependencies zuerst
2. Typed IDs
3. Multi-source
4. Deprecated removal
5. Library artifacts + preview skill
6. README + coverage UX + update notifier

---

## Risiko-Liste
- Breaking changes durch command removal
- Inkonsistente source metadata bei legacy installs
- Update-Check darf Startup nicht verlangsamen
- Cascade uninstall braucht klare UX, sonst Datenverlustrisiko

---

## Definition of Done fuer "perfekte CLI" (realistisch)
- Deterministische Install/Uninstall Semantik
- Dependency-safe default Verhalten
- Type-disambiguation ohne Mehrdeutigkeit
- Multi-source update robust
- >95% Coverage fuer In-Process Kernlogik
- Stabile E2E smoke matrix fuer reale CLI Flows

