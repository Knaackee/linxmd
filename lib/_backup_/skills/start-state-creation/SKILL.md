---
name: start-state-creation
type: skill
level: growth
version: 2.0.0
description: >
  Analyze an existing codebase and generate PROJECT.md, initial task backlog,
  and memory initialization. The "day zero" toolkit for existing projects.
tags: [growth, onboarding, project-analysis, initialization]
---

# Start State Creation Skill

> Take any existing codebase from unknown to fully mapped. Detect tech stack, generate orientation docs, seed the task backlog.

## Analysis Process

### Step 1: Directory Structure Scan
Map the top-level structure:
```
project/
├── src/           → Application source
├── tests/         → Test files
├── docs/          → Documentation
├── .github/       → CI/CD workflows
├── package.json   → Node.js project
└── ...
```

### Step 2: Tech Stack Detection

Read config files to identify:

| File | Indicates |
|------|-----------|
| `package.json` | Node.js, npm/yarn/pnpm |
| `tsconfig.json` | TypeScript |
| `*.csproj` / `*.sln` | .NET / C# |
| `Cargo.toml` | Rust |
| `go.mod` | Go |
| `pyproject.toml` / `requirements.txt` | Python |
| `Gemfile` | Ruby |
| `pom.xml` / `build.gradle` | Java |
| `Dockerfile` | Containerized |
| `.github/workflows/` | GitHub Actions CI |
| `Jenkinsfile` | Jenkins CI |
| `.gitlab-ci.yml` | GitLab CI |

### Step 3: Test Strategy Discovery

Identify test setup:
- Test framework (from config/dependencies)
- Test file location and naming convention
- Coverage tool (if configured)
- Test scripts (from package.json, Makefile, etc.)

### Step 4: Generate PROJECT.md

Using the onboarder agent's template, fill in:
- Verified tech stack (from actual config files)
- Real directory descriptions (from scanning)
- Known issues (from TODO/FIXME comments)
- Architecture observations (from directory structure and patterns)

### Step 5: Initialize .linxmd/

```
.linxmd/
├── memory/
│   ├── decisions/      ← Empty or imported from existing ADRs
│   └── learnings/      ← Empty
├── tasks/              ← Seeded from analysis
├── traces/             ← Empty
├── specs/              ← Empty
└── inbox/              ← Empty
```

### Step 6: Seed Tasks

Create tasks from:
- `TODO` comments in source code (search for TODO, FIXME, HACK, XXX)
- Missing test coverage (files without corresponding test files)
- Missing documentation (public APIs without docs)
- Security concerns (outdated dependencies, hardcoded values)
- Build/CI gaps (no lint, no format check, no coverage threshold)

## Output Checklist

- [ ] PROJECT.md generated with verified data
- [ ] `.linxmd/` directory structure initialized
- [ ] Initial tasks seeded from code analysis
- [ ] All findings presented to human for approval
- [ ] Nothing committed without human sign-off

## Rules

- **Verify, don't guess** — read actual config files, not assumptions
- **Conservative** — mark uncertain findings as "needs verification"
- **Read-only** — this skill analyzes, it doesn't modify source code
- **Human approval required** — present everything before committing
