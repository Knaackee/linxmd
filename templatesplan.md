# Templates Plan

## Zielbild

Linxmd sollte neben `agent`, `skill`, `workflow` und `pack` einen vierten inhaltlichen Bibliothekstyp `template` unterstützen.

Meine Empfehlung ist, Templates **nicht** als weiteren Runtime-Layer zwischen Workflows und Packs einzuführen, sondern als **separaten Dateityp fuer Vorlagen**:

```text
Authoring:
templates -> liefern Dateien als Vorlage

Runtime:
packs -> workflows -> agents -> skills -> memory
```

Das passt besser zu `lib/ARCHITECTURE.md` und verhindert, dass die bestehende Schichtung verwässert wird. Agents, Skills und Workflows bleiben die ausführbaren bzw. installierbaren Wissens- und Orchestrierungsbausteine. Templates sind dagegen nur Vorlagen-Dateien, die man in ein Projekt holen kann.

## Annahme

Ich interpretiere „Templates“ als wiederverwendbare Dateivorlagen für:

- neue Agent-Dateien
- neue Skill-Dateien
- neue Workflow-Dateien
- optional kleine Datei-Bundles

Im Repository habe ich keinen Verweis auf „Leap“ oder „LEAP“ gefunden. Der Vorschlag unten leitet sich daher ausschließlich aus den vorhandenen Specs und der aktuellen CLI-Implementierung ab.

## Warum Templates sinnvoll sind

Die vorhandenen Specs definieren bereits sehr klare Strukturregeln:

- `lib/agents/SPEC.md` beschreibt Pflichtfelder und Body-Struktur für Agents.
- `lib/skills/SPEC.md` beschreibt die Skill-Struktur.
- `lib/workflows/SPEC.md` beschreibt Phasen, Gates und Exit-Kriterien.
- `lib/FRONTMATTER-SPEC.md` liefert ein gemeinsames Frontmatter-Modell.

Genau daraus lassen sich hochwertige Templates ableiten. Der Mehrwert ist nicht ein weiterer Wissensbaustein im Laufzeitsystem, sondern ein einfacher Vorrat an standardisierten Dateien.

## Empfehlung für das fachliche Modell

### 1. Neuer Typ `template`

Das Shared Frontmatter sollte nur um `type: template` erweitert werden.

Minimalbeispiel:

```yaml
---
name: agent-core-template
type: template
version: 0.1.0
description: Vorlage fuer eine neue Agent-Datei mit Linxmd-konformer Struktur.
tags:
  - template
  - authoring
---
```

### 2. Template-spezifische Felder

Ich wuerde in `lib/FRONTMATTER-SPEC.md` fuer Templates keine weiteren Pflichtfelder einfuehren.

Fuer eine erste Version wuerde ich **keine** Platzhalter, **keine** Variablen, **keine** `quickActions` und **keine** `lifecycle`-Hooks fuer Templates aktiv nutzen. Das haelt V1 so einfach wie moeglich.

### 3. Verzeichnisstruktur

Empfehlung:

```text
lib/
  templates/
    SPEC.md
    agent-core/
      template.md
      files/
        agent.md
    skill-core/
      template.md
      files/
        SKILL.md
    workflow-core/
      template.md
      files/
        workflow.md
```

Ein Directory-basiertes Modell ist sinnvoller als einzelne Markdown-Dateien, weil ein Template dann einfach aus einem Manifest plus einem oder mehreren echten Dateien besteht.

## Empfehlung für die Spezifikation

### Neues Dokument

Es sollte ein neues Dokument `lib/templates/SPEC.md` geben.

Vorgeschlagene Struktur:

1. Zweck des Template-Typs
2. Required Frontmatter
3. Verzeichnisstruktur mit `template.md` und `files/`
4. einfaches Kopierverhalten
5. Body-Struktur des Template-Manifests
6. Beispiele fuer Agent-, Skill- und Workflow-Templates

### Einfaches Verhalten in `templates/SPEC.md`

Ich wuerde nur dieses Verhalten beschreiben:

- Ein Template besteht aus einem Manifest plus Dateien unter `files/`.
- Diese Dateien werden unveraendert kopiert.
- Templates werden nicht in Tool-Wrapper synchronisiert.

## Empfehlung fuer die CLI

Die aktuelle CLI ist auf Installation vorhandener Artefakte ausgerichtet. Fuer Templates reicht deshalb ein sehr einfaches Modell.

### Phase 1: Templates installierbar machen

Kurzfristig:

- `linxmd add template:agent-core --yes`
- Speicherung unter `.linxmd/templates/<name>/`
- `linxmd list template`
- `linxmd remove template:agent-core --yes`

Damit wird der Typ zuerst sauber in Index, State und CLI verankert.

### Phase 2: Templates kopieren

Danach ein neuer Befehl, z. B.:

```text
linxmd new template:agent-core
```

Das Verhalten ist dann bewusst simpel: Die Dateien aus `files/` werden unveraendert kopiert.

Alternative Namen waeren `create` oder `apply`. Ich wuerde `new` bevorzugen, weil das Verhalten klar scaffold-orientiert ist.

### Phase 3: Optionaler Sicherheitskomfort

Optional spaeter:

- Dry-Run-Ansicht der zu erzeugenden Dateien
- Abbruch bei Konflikten mit vorhandenen Dateien

## Konkrete Code-Auswirkungen

Die Umsetzung betrifft voraussichtlich mindestens diese Stellen:

- `src/Linxmd/Models/ArtifactType.cs`
  - `Template` ergänzen
- `src/Linxmd/Models/FrontMatter.cs`
  - minimales Template-Manifestmodell ergaenzen
- `src/Linxmd/Services/InstalledStateManager.cs`
  - `TemplatesDir` ergänzen
  - `EnsureDirectories()` erweitern
  - `IsInitialized` prüfen, ob Templates-Verzeichnis Pflicht sein soll
  - `GetArtifactDir("template")` ergänzen
- `src/Linxmd/Cli.cs`
  - neues Icon und Status-/List-Darstellung für `template`
- `src/Linxmd/Commands/CommandFactory.cs`
  - `list`-Filter und Status-Zählung um Templates erweitern
  - Install- und Uninstall-Logik für Directory-basierte Templates ergänzen
  - einfachen `new`-Befehl zum Kopieren der Template-Dateien einfuehren
- `.github/scripts/generate-index.py`
  - Templates in die Pattern-Liste aufnehmen
  - Pfadauflösung fuer Directory-basierte Templates ergänzen

## Wichtiger Architekturpunkt: Kein Sync in Copilot/Claude/OpenCode

`SyncEngine` sollte Templates bewusst **ignorieren**.

Begründung:

- Agents werden in Wrapper uebersetzt.
- Skills werden nach `.claude/skills/` kopiert.
- Workflows bleiben Linxmd-interne Artefakte.
- Templates sind Authoring-Rohmaterial und kein Laufzeitartefakt fuer den Agent-Host.

Das sollte auch in der Spezifikation explizit stehen, damit der Typ keine unklare Doppelrolle bekommt.

## Empfohlene Start-Templates

Fuer eine erste Version reichen drei Templates mit hohem Nutzen:

1. `template:agent-core`
  - liefert eine Agent-Datei mit den empfohlenen Abschnitten
2. `template:skill-core`
  - liefert eine `SKILL.md` mit den empfohlenen Abschnitten
3. `template:workflow-core`
  - liefert eine Workflow-Datei mit den empfohlenen Abschnitten

Optional als vierter Kandidat spaeter:

4. `template:starter-bundle`
  - liefert ein kleines Dateibundle aus Agent + Skill + Workflow

## Teststrategie

Die Tests sollten in Stufen aufgebaut werden.

### Parser und Modelle

- Frontmatter mit `type: template` wird korrekt erkannt
- Template-Verzeichnisse werden korrekt gelesen

### Index und Quellen

- `generate-index.py` nimmt Templates auf
- lokales Test-Lib unter `tests/Linxmd.Tests/TestLib` bekommt ein `templates/`-Fixture
- `LocalLibClient` kann Template-Verzeichnisse vollständig lesen

### CLI

- `add template:x --yes` installiert korrekt
- `list template` zeigt Templates korrekt an
- `remove template:x --yes` entfernt korrekt
- `status` zählt Templates separat oder bewusst nicht, aber konsistent

### Instanziierung

- `new template:x ...` erzeugt die erwarteten Dateien
- bestehende Dateien werden ohne explizite Zustimmung nicht überschrieben
- Dateiinhalte bleiben unveraendert

## Offene Entscheidungen

Vor einer Umsetzung sollten diese Punkte fest entschieden werden:

1. Sollen Templates in `.linxmd/templates/` installiert oder direkt aus der Quelle angewendet werden?
2. Soll `template` ein vollwertiger installierbarer Artefakttyp sein oder nur ein Source-seitiger Dateityp?
3. Braucht V1 nur Artefakt-Templates fuer `agent`, `skill`, `workflow`, oder auch kleine Bundles?
4. Sollen Templates immer nur kopiert werden, oder spaeter optional umbenannt werden duerfen?

## Meine Empfehlung fuer die Einfuehrungsreihenfolge

### Schritt 1

Spec-first:

- `lib/FRONTMATTER-SPEC.md` erweitern
- `lib/templates/SPEC.md` neu anlegen
- 3 Beispiel-Templates in `lib/templates/` anlegen

### Schritt 2

Core-CLI faehig machen:

- Typ `template` in Index, State, List, Add, Remove, Status integrieren
- noch ohne weitere Komfortfunktionen

### Schritt 3

Kopierbefehl bauen:

- `linxmd new template:<name>`
- Dateien aus `files/` kopieren
- Konflikterkennung

### Schritt 4

Dokumentation und E2E:

- README erweitern
- TestLib um Templates ergänzen
- umfassende E2E-Faelle fuer Install + Copy ergänzen

## Kurzfazit

Templates passen fachlich gut zu Linxmd, wenn sie als **einfache Vorlagen-Dateien** und nicht als weiterer Laufzeit-Layer modelliert werden. Die sauberste Linie ist:

- neuer Bibliothekstyp `template`
- neues `lib/templates/SPEC.md`
- Installation zunaechst lokal nach `.linxmd/templates/`
- spaeter einfacher CLI-Befehl zum Kopieren der Vorlagen
- kein Sync in externe Agent-Hosts

Wenn du willst, ist der naechste sinnvolle Schritt die Ausarbeitung von `lib/templates/SPEC.md` fuer genau dieses einfache Dateimodell.