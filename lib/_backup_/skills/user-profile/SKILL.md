---
name: user-profile
type: skill
level: core
version: 2.0.0
description: >
  Schema, creation, and maintenance of ~/.linxmd/user-profile.md — the personal
  orientation file every agent reads to adapt to the human's preferences.
tags: [core, profile, preferences, onboarding, global]
---

# User Profile Skill

> Every agent reads `~/.linxmd/user-profile.md` at startup. This skill defines what goes in it, how to create it, and when to update it.

## Global Environment: `~/.linxmd/`

Before any agent can read the user profile, the global directory must exist:

```
~/.linxmd/
├── user-profile.md        ← Identity, preferences, communication style
├── global/
│   ├── patterns.md        ← Cross-project patterns that work
│   └── antipatterns.md    ← Cross-project mistakes to avoid
└── config.md              ← Tool defaults (optional)
```

### Bootstrap Check

Every agent that reads `~/.linxmd/user-profile.md` must handle two cases:

1. **File exists** → Read and apply preferences.
2. **File missing** → Continue without it. Do NOT fail. Optionally suggest creating one.

Only `onboarder` actively creates the profile via the interview process below.

## User Profile Schema

```markdown
---
name: "<display name>"
created: <date>
updated: <date>
---

# User Profile

## Identity
- **Name**: <name>
- **Role**: <developer | designer | manager | mixed>
- **Experience Level**: <junior | mid | senior | lead>
- **Primary Languages**: <comma-separated>
- **Domain**: <web | backend | mobile | data | devops | mixed>

## Communication Preferences
- **Language**: <de | en | ...> — preferred language for agent responses
- **Verbosity**: <brief | normal | detailed>
- **Tone**: <casual | professional | technical>
- **Explanations**: <always | when-complex | never>

## Coding Preferences
- **Style**: <functional | oop | mixed>
- **Naming**: <camelCase | snake_case | PascalCase per language convention>
- **Max Function Length**: <lines>
- **Max File Length**: <lines>
- **Test Style**: <tdd-strict | test-after | coverage-target>
- **Coverage Target**: <percentage>

## Review Preferences
- **Review Depth**: <quick | standard | thorough>
- **Security Focus**: <minimal | standard | paranoid>
- **Performance Focus**: <minimal | standard | aggressive>
- **Auto-Fix Trivial**: <yes | no> — let consistency-guardian fix naming/imports automatically?

## Tool Preferences
- **Editor**: <vscode | neovim | ...>
- **Terminal**: <powershell | bash | zsh | ...>
- **Git Strategy**: <merge | rebase | squash>
- **Branch Prefix**: <feat/ | feature/ | custom>

## Timezone & Availability
- **Timezone**: <IANA timezone>
- **Work Hours**: <HH:MM–HH:MM> (optional)
```

## Creation Process

The `onboarder` agent creates the profile through a structured interview:

### Interview Questions (in order)

1. "What's your name and primary role?"
2. "Which programming languages do you use most?"
3. "Do you prefer brief or detailed agent responses?"
4. "In which language should agents communicate? (en/de/...)"
5. "Do you follow strict TDD, or write tests after implementation?"
6. "What's your preferred code review depth — quick, standard, or thorough?"
7. "Should agents auto-fix trivial issues (naming, imports) or always ask first?"
8. "Any specific coding conventions? (max function length, naming style, etc.)"

### Rules for the Interview

- Ask **max 3 questions at a time** — don't overwhelm.
- Provide **sensible defaults** for every field — user can just confirm.
- Accept **partial answers** — not every field is required.
- Mark optional fields clearly.

### Defaults (if user skips)

| Field | Default |
|-------|---------|
| Verbosity | normal |
| Tone | professional |
| Explanations | when-complex |
| Review Depth | standard |
| Security Focus | standard |
| Auto-Fix Trivial | yes |
| Git Strategy | merge |
| Coverage Target | 80% |

## Update Triggers

Re-run the interview (or prompt for updates) when:

- User explicitly requests changes ("I want shorter responses")
- A new project uses a tech stack not listed in the profile
- 3+ months since last update (agent may suggest a refresh)

## How Agents Use the Profile

| Profile Field | Agent Behavior Change |
|---|---|
| Language | Respond in user's preferred language |
| Verbosity | Adjust response length |
| Review Depth | reviewer-quality adjusts checklist granularity |
| Auto-Fix Trivial | consistency-guardian fixes without asking (or asks first) |
| Test Style | test-writer adapts workflow (RED-first vs. test-after) |
| Coverage Target | test-writer uses as threshold |
| Git Strategy | implementer uses merge/rebase/squash accordingly |

## What This Skill Does NOT Cover

- Project-specific memory (see `project-memory` skill)
- Session traces (see `trace-writing` skill)
- Task schemas (see `task-management` skill)
