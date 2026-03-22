# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.4.0] - 2026-03-22

### Added

- `linxmd memory index` — (re)build `.linxmd/memory.db` SQLite + FTS5 index from all project memory files
- `linxmd memory search <query>` — full-text search across ADRs, changelog blocks, and known issues, with highlighted snippets
- `linxmd memory stats` — entry counts per type (`decision` / `changelog` / `issue`)
- `linxmd memory recent [--type] [--limit]` — most recently indexed entries, newest first
- `skill:project-memory` — selective-load protocol, stability contract, and CLI search path

### Details

- SQLite index lives at `.linxmd/memory.db` (git-ignorable; safe to delete and rebuild at any time)
- Markdown files remain the canonical source of truth — the db is a derived search index
- `Microsoft.Data.Sqlite` 9.0.3 bundled via `SQLitePCLRaw`; native sqlite3 ships with the binary for win-x64, linux-x64, and osx-arm64 — no system SQLite required
- WAL journal mode and single-transaction bulk indexing for performance at scale (500+ ADRs)
- FTS5 `content=` external content table — text is not stored twice

---

## [0.3.0] - 2026-03-22

### Added

- Rebrand: renamed from `agentsmd` to `linxmd` — new binary name, namespace, and `.linxmd/` directory
- **Pack support** — install a curated bundle of multiple artifacts with a single `linxmd add pack:<name>`
  - `pack:fullstack-tdd` — full TDD pipeline with routing and context management
  - `pack:content-pipeline` — complete draft → fact-check → edit stack
  - `pack:quality-sprint` — baseline audit, project memory, and automated routing
  - `pack:i18n-ready` — i18n extraction, translation, and task management
- **8 new skills**: `api-design`, `code-translator`, `i18n`, `text-translator`, `design-tokens`, `project-memory`, `context-management`, `observability`
- **2 new agents**: `changelog-writer`, `architect`
- **3 new workflows**: `quality-baseline`, `release`, `bug-fix`
- `linxmd init-prompt [--copy]` — generates a ready-to-paste LLM onboarding prompt; `--copy` puts it on the clipboard
- `linxmd platform` — view and toggle which AI tools receive synced artifacts (Copilot, Claude Code, OpenCode)
- Library index rewritten with app-store-quality descriptions for all 37 artifacts
- Richer `linxmd add` browse view — description and version shown in selection prompt

### Changed

- `.agentsmd/` directory renamed to `.linxmd/` across all commands and sync targets
- `linxmd add` now handles packs with recursive artifact installation
- All artifact descriptions updated for clarity and discoverability

---

## [0.2.0] - 2026-03-22

### Added
- Rich terminal UI powered by Spectre.Console v0.50.0
- FigletText ASCII logo on `linxmd init`
- Rounded-border tables for search, list, and info commands
- Rounded-border panels for status overview and artifact info
- Emoji icons per artifact type (🤖 Agent, 📦 Skill, 🔄 Workflow)
- Animated spinner during network requests (fetch index, install)
- Tree visualization for `linxmd sync` output (wrappers + skills)
- "Did you mean?" fuzzy matching (Levenshtein) on not-found errors
- `Cli.cs` — centralized rendering helper with consistent theming
- Color system: DodgerBlue1 (primary), Green3 (success), Orange1 (warning), Red (error)

### Changed
- All command output now uses Spectre.Console markup (auto-strips in piped/redirected mode)
- Status command renders in a styled panel with emoji-prefixed counts
- Info command renders in a styled panel with bold labels and colored values
- Install/uninstall messages use ✓ checkmark prefix
- Search results displayed in formatted tables instead of plain text

## [0.1.0] - 2026-03-21

### Added
- Initial release of linxmd CLI
- `linxmd init` — Initialize `.linxmd/` directory structure with tasks/backlog and tasks/in-progress
- `linxmd agent install/uninstall/list/search/info` — Manage agents
- `linxmd skill install/uninstall/list/search/info` — Manage skills (folder-based)
- `linxmd workflow install/uninstall/list/search/info` — Manage workflows with dependency resolution
- `linxmd search` — Global search across all artifact types
- `linxmd list` — List all installed artifacts
- `linxmd sync` — Generate tool wrappers for Copilot, Claude Code, and OpenCode + copy skills to `.claude/skills/`
- `linxmd status` — Project overview with artifact counts and task status
- Lib content: 5 agents (test-writer, implementer, reviewer-spec, reviewer-quality, docs-writer)
- Lib content: 4 skills (task-management, feature, debugging, refactoring)
- Lib content: 2 workflows (sdd-tdd, content-review)
- Front matter parser for YAML metadata in markdown files
- Lib index parser with search and filter support
- GitHub Actions CI pipeline (Windows, Linux, macOS)
- GitHub Actions release pipeline (self-contained exe for 3 platforms)
- GitHub Actions lib index generation
- 51 tests (unit + E2E)
