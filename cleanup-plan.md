# Cleanup Plan

Stand: 2026-03-22

## Ziel

- Alle verbleibenden Legacy-Spuren entfernen.
- Rebranding auf "linxmd" konsequent in Code, CI, Doku und Artefakten durchziehen.
- Tote/obsolet gewordene Inhalte und lokale Build-Artefakte bereinigen.
- Breaking Changes sind explizit erlaubt.

## Gefundene Hotspots (repo scan)

1. Runtime-Fallback im Code
- `src/Linxmd/Services/UpdateNotifier.cs`: Nur `LINXMD_NO_UPDATE_CHECK` ist erlaubt.

2. Release-Artefaktnamen/CI noch auf Legacy-Prefix
- `.github/workflows/release.yml`: Matrix-Artefakte und Rename/Upload-Pfade waren noch nicht konsistent.

3. Legacy-CLI in Doku/Lib-Artefakten
- `lib/workflows/content-review.md`: Beispiele mit altem CLI-Schema.
- `lib/workflows/sdd-tdd.md`: Beispiele mit altem CLI-Schema.
- `CHANGELOG.md`: Historische Eintraege waren gemischt benannt.
- `NEXT-GEN.md`: Umfangreiche Altplanung mit deprecated command tree.

4. Konsistenz-/Naming-Reste
- Test-Tempfolderpraefixe wie `linxmd-test-*`, `linxmd-sync-*`.
- Namespace/Projektordnername `Linxmd` in `src/Linxmd/...` und `tests/Linxmd.Tests/...`.

5. Aufraeum-Kandidaten (dead folders / artefacts)
- Lokale generierte Ordner vorhanden: `src/Linxmd/bin`, `src/Linxmd/obj`, `tests/Linxmd.Tests/bin`, `tests/Linxmd.Tests/obj`, `tests/Linxmd.Tests/TestResults`.
- Lokaler `.linxmd/`-Ordner muss explizit ignoriert werden.

## Plan (priorisiert)

## Phase 1 - Hard Rebrand in Runtime/CI (P0)

1. Runtime-Fallback entfernen (breaking by design)
- Datei: `src/Linxmd/Services/UpdateNotifier.cs`
- Aktion: Nur noch `LINXMD_NO_UPDATE_CHECK` akzeptieren.

2. Release Pipeline auf linxmd umstellen
- Datei: `.github/workflows/release.yml`
- Aktionen:
- Matrix `artifact` auf `linxmd`/`linxmd.exe` setzen.
- Rename-Targets auf `publish/linxmd-${rid}` setzen.
- Upload-Name/-Path auf `linxmd-*` setzen.

Abnahme Phase 1
- `rg -n "legacy-update-check|legacy-artifact-prefix" src .github` liefert 0 Treffer fuer Runtime/Release-Pfade.
- Release-Workflow produziert Artefakte mit Prefix `linxmd-`.

## Phase 2 - CLI/Doku-Konsolidierung (P0)

1. Workflow-Dokumente auf neue CLI bringen
- Dateien:
- `lib/workflows/content-review.md`
- `lib/workflows/sdd-tdd.md`
- Aktion: Alte Beispiele auf `linxmd ...` umstellen.

2. Historische Doku sauber trennen
- Datei: `CHANGELOG.md`
- Aktion: Durchgaengig `linxmd` verwenden.

3. Alt-Planungsdokument behandeln
- Datei: `NEXT-GEN.md`
- Aktion: Loeschen.

Abnahme Phase 2
- `rg -n "\bagentsmd\b" lib README.md CHANGELOG.md` liefert 0 Treffer.

## Phase 3 - Repo Hygiene / Dead Artefacts (P1)

1. Runtime-Ordner gegen versehentliches Commit schuetzen
- Datei: `.gitignore`
- Aktion: `.linxmd/` aufnehmen.

2. Build/Test-Artefakte bereinigen
- Aktion lokal:
- `dotnet clean Linxmd.sln`
- Loeschen von `tests/Linxmd.Tests/TestResults` falls noetig.

3. Optionales Cleanup-Skript
- Neue Datei optional: `scripts/clean.ps1`
- Inhalt: `dotnet clean`, remove `**/bin`, `**/obj`, `**/TestResults` (repo-lokal).

Abnahme Phase 3
- `git status --short` zeigt keine Build-Artefakt-Dateien.
- Neu erzeugte lokale `.linxmd/` Inhalte bleiben untracked.

## Phase 4 - Naming Deep-Clean (P2, breaking)

1. Assembly ist bereits `linxmd`, aber Typ-/Ordnernamen sind noch `Linxmd`.
- Kandidaten:
- `src/Linxmd` -> `src/Linxmd`
- `tests/Linxmd.Tests` -> `tests/Linxmd.Tests`
- Namespace `Linxmd.*` -> `Linxmd.*`

2. Testdaten-/Tempnamen angleichen
- Dateien:
- `tests/Linxmd.Tests/Services/InstalledStateManagerTests.cs`
- `tests/Linxmd.Tests/Services/SyncEngineTests.cs`
- Aktion: Tempfolderpraefixe `linxmd-*` konsistent halten.

Abnahme Phase 4
- `rg -n "\bLinxmd\b|legacy-prefix" src tests` zeigt keine Legacy-Praefixe.

## Dead Code / Dead Content Einschätzung

- Wahrscheinlich dead content:
- `NEXT-GEN.md` (beschreibt entfernte/deprecated Struktur, nicht mehr Soll-Zustand).

- Potenziell stale content:
- Teile von `PLAN.md` und `CHANGELOG.md` sollten kontinuierlich auf Konsistenz geprueft werden.

- Runtime dead code
- Kein klarer toter Command-Codepfad gefunden (deprecated trees sind bereits entfernt).
- Legacy-Codepfad entfernt.

## Reihenfolge fuer Umsetzung

1. Phase 1 komplett
2. Phase 2 komplett
3. Phase 3 komplett
4. Phase 4 optional als separater Breaking-PR

## Validierung nach Umsetzung

1. `dotnet build Linxmd.sln`
2. `dotnet test tests/Linxmd.Tests/Linxmd.Tests.csproj --verbosity minimal`
3. `rg -n --hidden --glob '!**/bin/**' --glob '!**/obj/**' "legacy-check-token|legacy-cli-name"`
4. Dry-run Release-Workflow (Artefaktnamen checken: `linxmd-*`)

