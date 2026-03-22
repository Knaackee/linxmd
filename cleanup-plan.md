# Cleanup Plan

Stand: 2026-03-22

## Ziel

- Alle verbleibenden "agentsmd"-Kompatibilitaets- und Legacy-Spuren entfernen.
- Rebranding auf "linxmd" konsequent in Code, CI, Doku und Artefakten durchziehen.
- Tote/obsolet gewordene Inhalte und lokale Build-Artefakte bereinigen.
- Breaking Changes sind explizit erlaubt.

## Gefundene Hotspots (repo scan)

1. Runtime-Fallback im Code
- `src/Agentsmd/Services/UpdateNotifier.cs`: `AGENTSMD_NO_UPDATE_CHECK` wird noch als Legacy-Env akzeptiert.

2. Release-Artefaktnamen/CI noch auf agentsmd
- `.github/workflows/release.yml`: Matrix-Artefakte und Rename/Upload-Pfade nutzen noch `agentsmd*`.

3. Legacy-CLI in Doku/Lib-Artefakten
- `lib/workflows/content-review.md`: Beispiele mit `agentsmd ...`
- `lib/workflows/sdd-tdd.md`: Beispiele mit `agentsmd ...`
- `CHANGELOG.md`: Historische Eintraege nennen `agentsmd`.
- `NEXT-GEN.md`: Umfangreiche Altplanung mit deprecated command tree.

4. Konsistenz-/Naming-Reste
- Test-Tempfolderpraefixe wie `agentsmd-test-*`, `agentsmd-sync-*`.
- Namespace/Projektordnername `Agentsmd` in `src/Agentsmd/...` und `tests/Agentsmd.Tests/...`.

5. Aufraeum-Kandidaten (dead folders / artefacts)
- Lokale generierte Ordner vorhanden: `src/Agentsmd/bin`, `src/Agentsmd/obj`, `tests/Agentsmd.Tests/bin`, `tests/Agentsmd.Tests/obj`, `tests/Agentsmd.Tests/TestResults`.
- Lokaler `.agentsmd/`-Ordner ist untracked und aktuell nicht in `.gitignore` gelistet.

## Plan (priorisiert)

## Phase 1 - Hard Rebrand in Runtime/CI (P0)

1. Legacy Runtime-Fallback entfernen (breaking by design)
- Datei: `src/Agentsmd/Services/UpdateNotifier.cs`
- Aktion: Nur noch `LINXMD_NO_UPDATE_CHECK` akzeptieren, `AGENTSMD_NO_UPDATE_CHECK` entfernen.

2. Release Pipeline auf linxmd umstellen
- Datei: `.github/workflows/release.yml`
- Aktionen:
- Matrix `artifact` auf `linxmd`/`linxmd.exe` setzen.
- Rename-Targets auf `publish/linxmd-${rid}` aendern.
- Upload-Name/-Path auf `linxmd-*` setzen.

Abnahme Phase 1
- `rg -n "AGENTSMD_NO_UPDATE_CHECK|agentsmd-" src .github` liefert 0 Treffer fuer Runtime/Release-Pfade.
- Release-Workflow produziert Artefakte mit Prefix `linxmd-`.

## Phase 2 - CLI/Doku-Konsolidierung (P0)

1. Workflow-Dokumente auf neue CLI bringen
- Dateien:
- `lib/workflows/content-review.md`
- `lib/workflows/sdd-tdd.md`
- Aktion: `agentsmd ...` Beispiele auf `linxmd ...` umstellen.

2. Historische Doku sauber trennen
- Datei: `CHANGELOG.md`
- Aktion: Historische Versionen als Historie belassen, aber ab aktuellem Abschnitt nur `linxmd` verwenden.
- Optional strict mode: historisches Wording ebenfalls auf `linxmd` normieren (wenn komplette Brand-Puristik gewuenscht).

3. Alt-Planungsdokument behandeln
- Datei: `NEXT-GEN.md`
- Aktion: Entweder loeschen oder klar als `docs/archive/NEXT-GEN-agentsmd.md` archivieren.
- Empfehlung: Archivieren statt loeschen (Nachvollziehbarkeit).

Abnahme Phase 2
- `rg -n "\bagentsmd\b" lib README.md CHANGELOG.md NEXT-GEN.md` zeigt nur erlaubte Historien-/Archivstellen.

## Phase 3 - Repo Hygiene / Dead Artefacts (P1)

1. Runtime-Ordner gegen versehentliches Commit schuetzen
- Datei: `.gitignore`
- Aktion: `.agentsmd/` aufnehmen.

2. Build/Test-Artefakte bereinigen
- Aktion lokal:
- `dotnet clean Agentsmd.sln`
- Loeschen von `tests/Agentsmd.Tests/TestResults` falls noetig.

3. Optionales Cleanup-Skript
- Neue Datei optional: `scripts/clean.ps1`
- Inhalt: `dotnet clean`, remove `**/bin`, `**/obj`, `**/TestResults` (repo-lokal).

Abnahme Phase 3
- `git status --short` zeigt keine Build-Artefakt-Dateien.
- Neu erzeugte lokale `.agentsmd/` Inhalte bleiben untracked.

## Phase 4 - Naming Deep-Clean (P2, breaking)

1. Assembly ist bereits `linxmd`, aber Typ-/Ordnernamen sind noch `Agentsmd`.
- Kandidaten:
- `src/Agentsmd` -> `src/Linxmd`
- `tests/Agentsmd.Tests` -> `tests/Linxmd.Tests`
- Namespace `Agentsmd.*` -> `Linxmd.*`

2. Testdaten-/Tempnamen angleichen
- Dateien:
- `tests/Agentsmd.Tests/Services/InstalledStateManagerTests.cs`
- `tests/Agentsmd.Tests/Services/SyncEngineTests.cs`
- Aktion: Tempfolderpraefixe `agentsmd-*` -> `linxmd-*`.

Abnahme Phase 4
- `rg -n "\bAgentsmd\b|agentsmd-" src tests` zeigt nur explizit erlaubte historische Fundstellen.

## Dead Code / Dead Content Einschätzung

- Wahrscheinlich dead content:
- `NEXT-GEN.md` (beschreibt entfernte/deprecated Struktur, nicht mehr Soll-Zustand).

- Potenziell stale content:
- Teile von `PLAN.md` und `CHANGELOG.md` mit agentsmd-Begriffen. Nicht unbedingt dead, aber inkonsistent.

- Runtime dead code
- Kein klarer toter Command-Codepfad gefunden (deprecated trees sind bereits entfernt).
- Einziger klarer Legacy-Codepfad: Env-Fallback in `UpdateNotifier`.

## Reihenfolge fuer Umsetzung

1. Phase 1 komplett
2. Phase 2 komplett
3. Phase 3 komplett
4. Phase 4 optional als separater Breaking-PR

## Validierung nach Umsetzung

1. `dotnet build Agentsmd.sln`
2. `dotnet test tests/Agentsmd.Tests/Agentsmd.Tests.csproj --verbosity minimal`
3. `rg -n --hidden --glob '!**/bin/**' --glob '!**/obj/**' "\bagentsmd\b|AGENTSMD_NO_UPDATE_CHECK"`
4. Dry-run Release-Workflow (Artefaktnamen checken: `linxmd-*`)

