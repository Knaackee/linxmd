# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-03-21

### Added
- Initial release of agentsmd CLI
- `agentsmd init` — Initialize `.agentsmd/` directory structure with tasks/backlog and tasks/in-progress
- `agentsmd agent install/uninstall/list/search/info` — Manage agents
- `agentsmd skill install/uninstall/list/search/info` — Manage skills (folder-based)
- `agentsmd workflow install/uninstall/list/search/info` — Manage workflows with dependency resolution
- `agentsmd search` — Global search across all artifact types
- `agentsmd list` — List all installed artifacts
- `agentsmd sync` — Generate tool wrappers for Copilot, Claude Code, and OpenCode + copy skills to `.claude/skills/`
- `agentsmd status` — Project overview with artifact counts and task status
- Lib content: 5 agents (test-writer, implementer, reviewer-spec, reviewer-quality, docs-writer)
- Lib content: 4 skills (task-management, feature, debugging, refactoring)
- Lib content: 2 workflows (sdd-tdd, content-review)
- Front matter parser for YAML metadata in markdown files
- Lib index parser with search and filter support
- GitHub Actions CI pipeline (Windows, Linux, macOS)
- GitHub Actions release pipeline (self-contained exe for 3 platforms)
- GitHub Actions lib index generation
- 51 tests (unit + E2E)
