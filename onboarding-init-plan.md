# Onboarding Init Plan

Stand: 2026-03-22

## Ziel

Einfaches 2-Step-Modell:

1. `linxmd init` legt Struktur an, erstellt einen Basis-Skill fuer Linxmd-Bedienung und fuehrt `sync` aus.
2. Der User fuehrt danach bewusst einen Prompt aus, der das Onboarding abschliesst.

Keine automatische Analyse, keine implizite Installation, keine versteckten Aenderungen.

## Produktentscheidung

Init macht nur Setup-Grundlagen.

Der eigentliche Onboarding-Intellekt liegt im Prompt-Run des Users.

Vorteile:

1. Klarheit: Init ist deterministisch und schnell.
2. Kontrolle: User entscheidet explizit ueber den inhaltlichen Einstieg.
3. Flexibilitaet: Der Prompt kann auf Software-, Content- oder Mischprojekte eingehen.

## LLM-Betriebsmodell

Das LLM muss primaer die Linxmd CLI kennen.

Mit CLI-Wissen + Shell-Zugriff kann es den gesamten Onboarding-Ablauf selbst ausfuehren:

1. Zustand pruefen (CLI help + Dateistruktur)
2. Onboarding-Analyse per Prompt
3. Vorschlaege machen und nach Bestaetigung ausfuehren

Das LLM muss nicht alle Kommandos vorab kennen. Es kann bei Bedarf selbst `linxmd --help` nutzen.

## Scope von linxmd init

`linxmd init` darf nur:

1. `.linxmd/` Struktur anlegen
2. `sources.json` und `installed.json` initialisieren
3. Basis-Skill fuer Linxmd-Selbstbedienung anlegen (lokal unter `.linxmd/skills/`)
4. `sync` einmal automatisch ausfuehren
5. Success-Ausgabe + Next Step + Basis-Prompt anzeigen

`linxmd init` darf nicht:

1. Workflows automatisch installieren
2. Projektklassifikation erzwingen
3. Migrationen (tests/tracing/logging) auto-anlegen
4. Dateien ausserhalb `.linxmd/` veraendern

## Init Output (Soll)

Nach erfolgreichem Init:

1. "Created .linxmd/ structure."
2. "Created base skill: linxmd-self-bootstrap."
3. "Synced wrappers and skills."
4. "Run onboarding prompt to complete setup."
5. Ausgabe einer kopierbaren Basis-Prompt.

Optional:
6. Prompt in Clipboard kopieren (`--copy-prompt`)

## Onboarding Completion via Prompt

Der User startet danach einen Prompt in seinem Agent-Tool.

Der Prompt uebernimmt:

1. Ist-Analyse des Projekts (greenfield/bestand)
2. Empfehlung fuer passenden Workflow
3. Optional: Backlog fuer Tests/Tracing/Logging-Migration
4. Optional: Doku-Startdatei in `.linxmd/tasks/backlog/`
5. Optional: Installation/Verwaltung von Workflows, Agents und Skills nach Bestaetigung

Wichtig:

Der Prompt fuehrt kein `linxmd init` aus. Init ist bereits abgeschlossen.

## Prompt-Vorlage (MVP)

Nutze diese Vorlage fuer den User:

"Onboard this repository for linxmd. Do not run linxmd init. First analyze what kind of project this is (software/content/mixed), whether it is greenfield or existing, and what quality gaps exist (tests, logging, tracing). Then propose one primary workflow and up to three optional improvements. Do not change files yet. Wait for my confirmation. After confirmation, you may use linxmd add/remove/sync/update to apply the agreed setup."

## Command-Policy

Bleibt strikt:

1. `add`, `remove`, `list`, `update`, `sync` brauchen init
2. `status` darf ohne init nur informieren
3. `init` bleibt der einzige Einstieg ohne Vorbedingungen

## Implementierungsplan (kompakt)

## Batch 1

1. Init auf Struktur + Basis-Skill + Auto-Sync umstellen
2. Ausgabe auf 2-Step-Modell mit Basis-Prompt umstellen
3. Tests fuer Init-Output, Basis-Skill-Erzeugung und Auto-Sync

## Batch 2

1. Prompt-Hinweistext in README und CLI-Hilfe aufnehmen
2. `linxmd init-prompt` Command, der die Vorlage ausgibt
3. Optionaler Clipboard-Copy fuer `init` und `init-prompt`

## Abnahme

1. `linxmd init` ist schnell und deterministisch
2. Nach Init wurde nichts ausser `.linxmd/` und Sync-Zielordnern veraendert
3. Basis-Skill ist vorhanden und beschreibt Linxmd-Bedienung
4. Kein Workflow wird ohne expliziten User-Befehl installiert
5. User bekommt eine kopierbare Basis-Prompt fuer das LLM-Onboarding
6. User kann die Prompt spaeter via `linxmd init-prompt` erneut ausgeben
