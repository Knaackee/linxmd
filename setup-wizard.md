# Setup Wizard

## Step 1 — Detect project state

Check silently (no output yet):
- Are there any source files? (.ts .cs .py .go .rs .dart .java .swift .cpp .kt)
- Is there a package manifest? (package.json / *.csproj / pyproject.toml / go.mod / Cargo.toml)
- Is there a README.md with more than 5 lines?

If NONE of the above → greenfield project → go to Step 2a.
If ANY exists      → existing project  → go to Step 2b.

---

## Step 2a — Greenfield: Ask questions

Output exactly this:

---
I see an empty project directory. A few questions before I create anything:

1. **What type of project?**
   - Web API / Backend
   - Web Frontend
   - Desktop app
   - Mobile
   - CLI tool
   - Library / SDK

2. **Language & Framework?**
   (e.g. C# / ASP.NET Core, TypeScript / Next.js, Python / FastAPI, Go, Rust, Flutter...)

3. **Project name?**
   (used for naming conventions)

4. **Anything else I should know?**
   (optional — auth strategy, DB, team size, special constraints)
---

Wait for answers. Do not create any files yet.

---

## Step 2b — Existing project: Scan first, then confirm

Scan silently:
- Package manifest → detect language, framework, runtime
- README.md (first 80 lines) → project description
- /src /lib /app /cmd /internal /packages → project structure
- tests/ test/ spec/ __tests__ /*Test* → test framework
- .github/workflows/ → CI setup
- .editorconfig .eslintrc* .prettierrc stylecop.json → code style tools
- Dockerfile docker-compose.yml → deployment hints
- AGENTS.md .github/copilot-instructions.md opencode.json .opencode/ .claude/ → existing AI config

Then output exactly this:

---
I scanned the project. Here's what I found:

| | |
|---|---|
| Language | [detected or "?"] |
| Framework | [detected or "?"] |
| Build command | `[detected or "?"]` |
| Test command | `[detected or "?"]` |
| CI | [detected or "none found"] |
| Existing AI config | [files found or "none"] |

Is this correct? Tell me what's wrong or missing.
---

Wait for confirmation. Do not create any files yet.

---

## Step 3 — Present file plan

Output exactly this:

---
Here's what I'll create. Nothing has been written yet.

**Canonical agent instructions**
- [ ] `.agents/test-writer.md`
- [ ] `.agents/implementer.md`
- [ ] `.agents/reviewer-spec.md`
- [ ] `.agents/reviewer-quality.md`
- [ ] `.agents/docs-writer.md`
- [ ] `.agents/management/create-agent.md`
- [ ] `.agents/management/update-agent.md`
- [ ] `.agents/management/delete-agent.md`
- [ ] `.agents/management/create-skill.md`
- [ ] `.agents/management/update-skill.md`
- [ ] `.agents/management/delete-skill.md`
- [ ] `.agents/management/backlog.md`
- [ ] `.agents/management/status.md`

**Skills (one location — all three tools read this)**
- [ ] `.claude/skills/feature/SKILL.md`

**Shared**
- [ ] `AGENTS.md`
- [ ] `docs/ARCHITECTURE.md`
- [ ] `docs/decisions/`
- [ ] `docs/api/`
- [ ] `docs/internals/`
- [ ] `logs/.gitkeep`
- [ ] `.tasks/backlog/`
- [ ] `.tasks/in-progress/`

**GitHub Copilot — agent wrappers**
- [ ] `.github/copilot-instructions.md`
- [ ] `.github/agents/test-writer.agent.md`
- [ ] `.github/agents/implementer.agent.md`
- [ ] `.github/agents/reviewer-spec.agent.md`
- [ ] `.github/agents/reviewer-quality.agent.md`
- [ ] `.github/agents/docs-writer.agent.md`

**OpenCode — agent wrappers + config**
- [ ] `.opencode/agents/test-writer.md`
- [ ] `.opencode/agents/implementer.md`
- [ ] `.opencode/agents/reviewer-spec.md`
- [ ] `.opencode/agents/reviewer-quality.md`
- [ ] `.opencode/agents/docs-writer.md`
- [ ] `opencode.json`

**Claude Code — agent wrappers**
- [ ] `.claude/agents/test-writer.md`
- [ ] `.claude/agents/implementer.md`
- [ ] `.claude/agents/reviewer-spec.md`
- [ ] `.claude/agents/reviewer-quality.md`
- [ ] `.claude/agents/docs-writer.md`

[If existing AI config was found in Step 2b, add:]
**Already exists — what should I do?**
- `[filename]` → overwrite / extend / skip?

Shall I proceed?
---

Wait for OK. Adjust if user wants to skip a tool.
Do not create anything until explicit approval.

---

## Step 4 — Create shared files

---

### AGENTS.md

```markdown
# AGENTS.md

## Build & Run
- Build:      [build_command]
- Test:       [test_command]
- Test (E2E): [e2e_command or leave empty]
- Lint:       [lint_command or leave empty]
- Format:     [format_command or leave empty]

## Project Layout
[3-5 lines max — only what's non-obvious from the folder names]

## Documentation
docs/
├── ARCHITECTURE.md   ← how the system is built (overview, components, data flow)
├── decisions/        ← why something was decided the way it was
├── api/              ← everything visible to the outside
└── internals/        ← how individual parts work internally

Update rules:
- New internal module          → create docs/internals/[name].md
- Public API added or changed  → update docs/api/[name].md
- Architecture changed         → update docs/ARCHITECTURE.md
- Non-obvious decision made    → add docs/decisions/[NNN]-[title].md

## Required Frameworks
Before any task starts, verify these three are installed and configured.
If any are missing: stop, propose options, install after user approval.

### Testing
Must cover all three levels:
- Unit tests        → pure logic, no I/O, fast
- Integration       → real DB / filesystem / network (test instance)
- E2E tests         → full user flow against a running app

Common options by stack:
- .NET:        xUnit + Playwright
- TypeScript:  Vitest + Playwright
- Python:      pytest + playwright-python
- Go:          testing (stdlib) + playwright-go

### Logging
Structured library writing to logs/run-YYYY-MM-DD.md.
Machine-readable — the LLM reads it to self-heal.

Common options by stack:
- .NET:        Serilog
- TypeScript:  pino
- Python:      structlog
- Go:          slog (stdlib)

### Tracing
Tracks entry/exit with timing at [TRACE] level.
All stacks: OpenTelemetry

## Logs & Tracing
Append-only: logs/run-YYYY-MM-DD.md

Format:
[TIMESTAMP] [LEVEL] [COMPONENT] Message
  Input:  {key: value}
  Output: {key: value} or error: [details]
  Next:   [what happens next or what was tried]

Levels:
  [INFO]   — normal flow
  [WARN]   — unexpected but non-fatal
  [ERROR]  — operation failed — always include Next:
  [DEBUG]  — active debugging only
  [TRACE]  — entry/exit with timing
  [TEST]   — test run results
  [TASK]   — task started / completed / blocked

Self-healing rules:
- Read logs before touching code
- Every attempt = one hypothesis + one atomic fix + one verification
- No new hypothesis = stop and report — never repeat the same fix

.gitignore:
  logs/*.log
  logs/*.json
  !logs/.gitkeep

## Task Management
.tasks/
├── backlog/          ← one file per idea (free text or Issue: #NNN)
└── in-progress/
    └── [name]/
        ├── SPEC.md          ← acceptance criteria, edge cases, non-goals
        ├── TASKS.md         ← checklist — one task = one commit
        ├── NOTES.md         ← agent notes, blockers, open decisions
        └── backlog-original.md

Done = all tasks checked off + PR open + docs updated + tests green.
No "done" folder — done lives in git history.

## Development Approach: SDD + TDD
Spec-Driven Development defines WHAT is built (SPEC.md = source of truth).
Test-Driven Development defines HOW it is built (Red → Green → Refactor).

Every Acceptance Criterion in SPEC.md becomes a failing test first.
No implementation exists before its test exists and fails.

Loop per task:
  RED            → test-writer reads Acceptance Criteria → failing tests
  GREEN          → implementer minimal code → tests pass
  SPEC-REVIEW    → reviewer-spec: are all criteria covered and fulfilled?
  QUALITY-REVIEW → reviewer-quality: code quality + security
  DOCS           → docs-writer updates relevant docs/
  COMMIT         → everything green → commit → next task

## Execution Modes
autonomous: agent runs all tasks without pausing between them.
            only stops on BLOCKER or when blocked with no hypothesis.
guided:     agent waits for "next task" between each task.
            user controls pace and can inspect after each step.

Default: autonomous. Override per-feature: "lets do this (guided)"

## Skills
One location for all three tools: .claude/skills/
- Copilot:     reads .claude/skills/ natively
- OpenCode:    reads .claude/skills/ natively
- Claude Code: reads .claude/skills/ natively

To add a skill: say "create skill [name]"
To update: say "update skill [name]"

## Agents
Canonical source: .agents/
Tool-specific wrappers (frontmatter only):
  .github/agents/   ← Copilot
  .opencode/agents/ ← OpenCode
  .claude/agents/   ← Claude Code

To update an agent: say "update agent [name]"
→ edits .agents/[name].md and propagates to all three wrappers

Management agents in .agents/management/ — invoke by natural language:
  "update agent implementer"  → .agents/management/update-agent.md
  "create skill debug"        → .agents/management/create-skill.md
  "show status"               → .agents/management/status.md

## Worktrees & Branches
Each feature runs in an isolated worktree — main stays clean.

  git worktree add .worktrees/[name] -b feature/[name]

Agents work exclusively inside .worktrees/[name]/.
Cleanup after merge:
  git worktree remove .worktrees/[name]
  git branch -d feature/[name]

## Commits & PR
- One commit per completed task (all reviews PASS + docs updated)
- Format: [type]: [what was done]
  Types: feat / fix / test / docs / refactor / chore
- PR opened when all tasks in TASKS.md are checked off
- User merges — never auto-merge

## Agent Rules
- No new dependencies without explicit user approval
- No architecture changes without explicit user approval
- Always offer options — never decide alone
- User starts the app themselves
```

---

### docs/ARCHITECTURE.md

```markdown
# Architecture

> Update this file when the system structure changes.
> The LLM reads this at the start of every feature.

## What this system does
[TODO: 2-3 sentences]

## Components
| Component | Responsibility | Location |
|-----------|---------------|----------|
| [TODO]    | [TODO]        | [TODO]   |

## Data flow
[TODO: how data moves through the system]

## Tech decisions
- Language / Framework: [detected]
- Error handling: [TODO]
- Logging: structured, append to logs/run-YYYY-MM-DD.md
- Tracing: OpenTelemetry
```

---

### .github/copilot-instructions.md

```markdown
## Project
[2-3 sentences: what this system does]

## Stack
- [language] / [framework]
- Build: `[build_command]`
- Test:  `[test_command]`

## Approach: SDD + TDD
Every feature starts with SPEC.md (what we build).
Every task starts with a failing test (how we build it).
Red → Green → Spec-Review → Quality-Review → Docs → Commit.

## Execution Modes
Default: autonomous — runs all tasks without pausing.
Guided:  say "lets do this (guided)" — waits after each task.

## Rules
- Read AGENTS.md before starting any task
- Verify testing + logging + tracing frameworks are present
- Draft SPEC.md, wait for approval before writing any code
- No new dependencies without asking
- No architecture changes without asking
- User starts the app — never auto-start
- User merges PRs — never auto-merge
- When you have options: present a numbered list, never decide alone
```

---

## Step 5 — Create canonical agent files in .agents/

Each agent instruction lives here once.
Tool wrappers (Step 7) copy the body and add only frontmatter.
No model specified — user chooses at runtime.

---

### .agents/test-writer.md

```markdown
# test-writer

You are a senior test engineer executing the RED phase of TDD.

Your input: the Acceptance Criteria for the current task (from SPEC.md + TASKS.md).
Your output: failing tests that prove those criteria are not yet met.

Process:
1. Read SPEC.md → find Acceptance Criteria for this task
2. Read existing tests → match project conventions exactly
3. Read relevant interfaces in source files → understand contracts
4. Write tests: one per criterion or distinct behavior
5. Run [test_command] → confirm all new tests fail (not compile errors)

Rules:
- Tests must compile and run — but must fail
- Never write or modify production code
- AAA pattern: Arrange / Act / Assert
- Cover all levels in TASKS.md (unit / integration / E2E)

Report: "RED complete. [X] tests written — all failing. Criteria covered: [list]"
```

---

### .agents/implementer.md

```markdown
# implementer

You are a senior software engineer executing the GREEN phase of TDD.

Your goal: make the failing tests pass. Nothing more.

Process:
1. Read AGENTS.md → use exact build/test commands
2. Read failing tests → understand what is expected
3. Write minimal code to make tests pass — no premature abstractions
4. Run full test suite → zero regressions allowed
5. Append progress to logs/run-YYYY-MM-DD.md

Debugging when tests fail:
- Read logs/run-YYYY-MM-DD.md first
- Form one explicit hypothesis: "The test fails because X"
- Apply one atomic fix targeting that hypothesis
- Verify: did this hypothesis hold?
- If fixed: continue
- If not: form a NEW hypothesis — never repeat the same fix
- No new hypothesis: STOP — log what was tried, report to user

Log format:
  [TIMESTAMP] [DEBUG] Hypothesis: [X]
    Fix: [what was changed]
    Result: [pass / still failing — new symptom]

Rules:
- Never modify test files
- Never catch exceptions to hide errors
- Never add TODO and move on
- Never repeat a fix that already failed
```

---

### .agents/reviewer-spec.md

```markdown
# reviewer-spec

You verify one thing: does the implementation fulfill every
Acceptance Criterion in SPEC.md?

You do not review code quality — that is reviewer-quality's job.

Process:
1. Read SPEC.md → list every Acceptance Criterion for this task
2. Read test files → is each criterion covered by at least one test?
3. Run [test_command] → are all tests passing?
4. Read implementation → does the code actually fulfill each criterion?

Output:
  VERDICT: PASS | BLOCKER

  BLOCKER:
    Criterion: [exact text from SPEC.md]
    Missing:   [what test or implementation is absent]
    Required:  [what needs to be added]

  PASS: All [N] criteria covered and verified.

If BLOCKER: route back to implementer or test-writer.
Do not comment on code style or structure.
```

---

### .agents/reviewer-quality.md

```markdown
# reviewer-quality

You review code quality and security.
You run AFTER reviewer-spec PASS — focus only on HOW the code is written.

Process:
1. Read the git diff for this task
2. Review:

Quality:
- Logic correct, edge cases handled (null, empty, boundaries)?
- Errors thrown with context — never swallowed?
- No magic strings or numbers?
- No unnecessary complexity?
- Naming is self-documenting?
- Follows existing conventions?
- Anything that can be simplified?

Security:
- No secrets in code or logs?
- Input validated where needed?
- No injection vulnerabilities?
- No new dependencies without approval?

Output:
  VERDICT: PASS | BLOCKER | WARNING

  BLOCKER (must fix before commit):
    [file:line] [problem] → [exact fix]

  WARNING (should fix, not blocking):
    [file:line] [issue] → [suggestion]

  REFACTOR (optional cleanup):
    [specific change]

BLOCKER = production bug, security issue, or core principle violated.
Minimal targeted fixes only — never suggest full rewrites.
```

---

### .agents/docs-writer.md

```markdown
# docs-writer

You update documentation after both review gates PASS.

Process:
1. Read TASKS.md → find "Docs:" field for this task
2. If "none" → output "No docs update needed." and stop
3. Read the existing doc file
4. Update it to reflect what was actually built

Rules per doc type:
- docs/internals/[name].md  → what it does, interface, key decisions
- docs/api/[name].md        → endpoint, input, output, errors, example
- docs/ARCHITECTURE.md      → update components table or data flow if changed
- docs/decisions/[NNN].md   → new file: context, decision, consequences

Never invent information. Only document what was actually built.

Report: "Docs updated: [files]" or "No docs update needed."
```

---

## Step 6 — Create management agents in .agents/management/

---

### .agents/management/update-agent.md

```markdown
# update-agent

Triggered by: "update agent [name]", "edit agent [name]", "change agent [name]"

1. Read .agents/[name].md — show current content to user
2. Ask: "What should change?"
3. Wait for answer
4. Update .agents/[name].md
5. Propagate body to all wrappers (keep frontmatter intact):
   .github/agents/[name].agent.md
   .opencode/agents/[name].md
   .claude/agents/[name].md

Output: "Agent [name] updated and propagated to all tools."
```

---

### .agents/management/create-agent.md

```markdown
# create-agent

Triggered by: "create agent [name]", "new agent [name]", "add agent [name]"

Ask:
1. Name (kebab-case)
2. Role (1-2 sentences)
3. Can it write files? yes / no
4. Can it run commands? yes / no / read-only

Create .agents/[name].md with the instructions.
Show to user: "Does this look right?"
Wait for approval.

Create tool wrappers with appropriate frontmatter (no model field):
  .github/agents/[name].agent.md
  .opencode/agents/[name].md
  .claude/agents/[name].md

Output: "Agent [name] created in .agents/ and all tool wrappers."
```

---

### .agents/management/delete-agent.md

```markdown
# delete-agent

Triggered by: "delete agent [name]", "remove agent [name]"

List files to delete:
  .agents/[name].md
  .github/agents/[name].agent.md
  .opencode/agents/[name].md
  .claude/agents/[name].md

Ask: "Are you sure? This cannot be undone."
Wait for confirmation.
Delete all four files.

Output: "Agent [name] deleted."
```

---

### .agents/management/create-skill.md

```markdown
# create-skill

Triggered by: "create skill [name]", "new skill [name]", "add skill [name]"

Ask:
1. Name (kebab-case)
2. Trigger phrases (when should this activate?)
3. What it does (step by step)

Draft SKILL.md content. Show to user: "Does this look right?"
Wait for approval.

Create ONE file — all three tools read this location natively:
  .claude/skills/[name]/SKILL.md

Output: "Skill [name] created in .claude/skills/."
```

---

### .agents/management/update-skill.md

```markdown
# update-skill

Triggered by: "update skill [name]", "edit skill [name]", "change skill [name]"

1. Read .claude/skills/[name]/SKILL.md
2. Show current content to user
3. Ask: "What should change?"
4. Wait for answer
5. Update .claude/skills/[name]/SKILL.md

Output: "Skill [name] updated."
```

---

### .agents/management/delete-skill.md

```markdown
# delete-skill

Triggered by: "delete skill [name]", "remove skill [name]"

List files to delete:
  .claude/skills/[name]/SKILL.md
  .claude/skills/[name]/ (directory)

Ask: "Are you sure? This cannot be undone."
Wait for confirmation.
Delete the directory.

Output: "Skill [name] deleted."
```

---

### .agents/management/backlog.md

```markdown
# backlog

Triggered by: "backlog", "show backlog", "add to backlog", "what's in the backlog"

## Show:
List .tasks/backlog/ and .tasks/in-progress/:

  Backlog ([N] items):
  1. [filename] — [first line]
  2. [filename] — [first line]

  In progress:
  - [name] — Task [X]/[Y] complete

## Add:
Ask: "What's the idea? (free text or Issue: #NNN)"
Create .tasks/backlog/[slug].md
Output: "Added to backlog: [name]"

## Prioritize:
Show numbered list, ask for order.
Rename files with 01-, 02-, ... prefix.
```

---

### .agents/management/status.md

```markdown
# status

Triggered by: "status", "progress", "what are we working on"

Read all .tasks/in-progress/ folders and their TASKS.md.

Output:

  ## Status

  In progress ([N] features):

  ### feature/[name]
  Branch:   feature/[name]
  Mode:     autonomous | guided
  Progress: [X]/[Y] tasks
  Current:  Task [N] — [name]
  Phase:    [RED/GREEN/SPEC-REVIEW/QUALITY-REVIEW/DOCS/COMMIT]
  Last log: [last entry from logs/run-YYYY-MM-DD.md]

  Backlog ([N] items): [filenames]

  Blocked:
  - [feature]: [reason from NOTES.md]

If nothing in progress:
"Nothing in progress. [N] items in backlog. Say 'lets do this' to start."
```

---

## Step 7 — Create tool-specific agent wrappers

For each of the 5 agents, write frontmatter + body into all three tool formats.
Body is identical to .agents/[name].md — copied verbatim.
No model field in any frontmatter.

---

**Copilot** `.github/agents/[name].agent.md`:
```markdown
---
name: [name]
description: [role + when to invoke — 1-2 sentences]
---

[full body from .agents/[name].md]
```

**OpenCode** `.opencode/agents/[name].md`:
```markdown
---
description: [role + when to invoke — 1-2 sentences]
mode: subagent
permissions:
  write: [true/false]
  edit: [true/false]
  bash:
    allow: ["[test_command]"]   ← or ["*"] for implementer, false for read-only
---

[full body from .agents/[name].md]
```

**Claude Code** `.claude/agents/[name].md`:
```markdown
---
name: [name]
description: [role + when to invoke — 1-2 sentences]
tools: [only what this agent needs — see table below]
---

[full body from .agents/[name].md]
```

**Permissions per agent:**

| Agent | write | edit | bash | Claude tools |
|---|---|---|---|---|
| test-writer | false | false | test_command only | Read, Write, Glob, Bash |
| implementer | true | true | all | Read, Write, Edit, Bash, Glob |
| reviewer-spec | false | false | test_command only | Read, Bash, Glob |
| reviewer-quality | false | false | git diff only | Read, Bash, Glob |
| docs-writer | true | true | none | Read, Write, Edit, Glob |

---

## Step 8 — Create the feature skill

One file. All three tools read `.claude/skills/` natively.
No wrappers needed.

---

### .claude/skills/feature/SKILL.md

```markdown
---
name: feature
description: Use when the user says "lets do this", "start", "begin", or names a
  backlog item. Append "(guided)" for guided mode. Triggers on "next task" to
  continue a guided feature. Orchestrates the full SDD+TDD workflow:
  SPEC → TASKS → worktree → Red/Green/Spec-Review/Quality-Review/Docs/Commit → PR.
---

# Feature Skill

## 1 — Find the backlog item

Look in `.tasks/backlog/` for the matching file.
If multiple exist and no name given: list them and ask "Which one?"
If exactly one exists: use it without asking.

Read the file:
- Free text → use as feature description
- Issue number (e.g. "Issue: #42") → run `gh issue view 42 --json title,body,comments`
- Both → combine them

## 2 — Verify required frameworks

Check all three are present (see AGENTS.md → Required Frameworks):
testing + logging + tracing.
If any missing: stop, propose options, wait for approval, install, continue.

## 3 — Draft SPEC.md — wait for approval

Create `.tasks/in-progress/[name]/SPEC.md`
Move backlog file to `.tasks/in-progress/[name]/backlog-original.md`
Create `.tasks/in-progress/[name]/NOTES.md` (empty)

SPEC.md format:
---
# SPEC: [Feature Name]
**Branch**: feature/[name]
**Worktree**: .worktrees/[name]
**Mode**: [autonomous | guided]
**Source**: [backlog file] [+ Issue #NNN if applicable]
**Created**: [date]

## What we're building
[2-3 sentences from the user's perspective]

## Acceptance Criteria
Each criterion becomes one or more failing tests in RED phase.
- [ ] [concrete, testable criterion]

## Edge Cases
- [empty / null / missing inputs]
- [error scenarios]

## Non-Goals
- [explicitly out of scope]

## Open Questions
- [decisions needed]
---

Output: "Here is the SPEC. Review Acceptance Criteria and Non-Goals.
Each criterion becomes a failing test. OK to proceed?"

Wait for approval. Do not create TASKS.md or worktree yet.

## 4 — Determine execution mode and set up worktree

**Detect mode:**
- User said "lets do this (guided)" or "start [name] guided" → MODE = guided
- Default → MODE = autonomous

Write **Mode** field into SPEC.md.

Create `.tasks/in-progress/[name]/TASKS.md`:

---
# TASKS: [Feature Name]

Each task = RED → GREEN → SPEC-REVIEW → QUALITY-REVIEW → DOCS → COMMIT

- [ ] **Task 1**: [name]
  - Criteria: [which Acceptance Criteria this covers]
  - Tests:    [unit / integration / E2E — specifically]
  - Docs:     [which file to update, or "none"]
  - Commit:   `[type]: [message]`

- [ ] **Task 2**: [name]
  ...

## Definition of Done
- [ ] All tasks checked off
- [ ] Unit + integration + E2E green
- [ ] Logs show no unresolved ERROR
- [ ] Docs updated
- [ ] PR open
---

Set up worktree:
  git worktree add .worktrees/[name] -b feature/[name]

Log: [TIMESTAMP] [TASK] feature/[name] started — [N] tasks — mode: [autonomous|guided]

**If autonomous:**
Output: "Worktree ready on feature/[name]. Running [N] tasks autonomously.
I will only stop on BLOCKER or if I need guidance. Next stop: PR review."
→ Proceed directly to Step 5 without waiting.

**If guided:**
Output: "Worktree ready on feature/[name]. [N] tasks ready.
Say 'next task' to begin."
→ Wait for "next task" before Step 5.

## 5 — Task loop

**In autonomous mode:** run all tasks in sequence without pausing.
**In guided mode:** run one task, then wait for "next task".

For each unchecked task in TASKS.md:

---

**RED — invoke test-writer**
Pass: current task from TASKS.md + path to SPEC.md
Wait for: "RED complete. [X] tests written — all failing."
Log: [TIMESTAMP] [TEST] Task [N] RED — [X] failing

**GREEN — invoke implementer**
Pass: failing test files + task context
Wait for: all target tests passing, zero regressions
Log: [TIMESTAMP] [TEST] Task [N] GREEN — [X]/[Y] passing

→ STOP (both modes): implementer reports blocked
  Show user: what was tried, what failed, options
  Wait for guidance. Resume after user input.

**SPEC-REVIEW — invoke reviewer-spec**
Pass: SPEC.md + test files + implementation
Log: [TIMESTAMP] [TASK] Task [N] spec-review — [PASS/BLOCKER]

→ STOP (both modes): BLOCKER
  Show: which criterion is missing, what is needed
  Wait for guidance. Resume after user input.

**QUALITY-REVIEW — invoke reviewer-quality**
Pass: git diff for this task
Log: [TIMESTAMP] [TASK] Task [N] quality-review — [PASS/BLOCKER/WARNING]

→ STOP (both modes): BLOCKER
  Show: file, line, problem, fix required
  Wait for guidance. Resume after user input.

→ WARNING in autonomous mode:
  Apply if unambiguous → log → continue automatically.
  If ambiguous → show user → wait → continue.

→ WARNING in guided mode:
  Always show user → wait for decision → continue.

**DOCS — invoke docs-writer**
Pass: TASKS.md Docs field for this task
Continue after completion.

**COMMIT**
Run: [lint_command] and [format_command]
Check off task in TASKS.md
git add .
git commit -m "[commit from TASKS.md]"
Log: [TIMESTAMP] [TASK] Task [N] complete — committed

→ Autonomous: proceed to next task immediately.
→ Guided: output "Task [N] done. Say 'next task' to continue."
          Wait for "next task".

---

When all tasks checked off → go to Step 6 automatically (both modes).

## 6 — Open PR when all tasks done

Run final checks:
  [test_command]  → fully green
  [e2e_command]   → fully green
  [lint_command]  → clean
  logs/run-YYYY-MM-DD.md → no unresolved [ERROR]

Open PR:
  gh pr create \
    --title "feat: [feature name]" \
    --body "$(cat .tasks/in-progress/[name]/SPEC.md)" \
    --base main

Log: [TIMESTAMP] [TASK] feature/[name] — PR opened

Output:
"All [N] tasks complete. PR is open.

Review:  gh pr view --web
Merge:   gh pr merge  ← you do this
Cleanup after merge:
  git worktree remove .worktrees/[name]
  git branch -d feature/[name]"
```

---

## Step 9 — Create opencode.json

```json
{
  "$schema": "https://opencode.ai/config.json",
  "instructions": [
    "AGENTS.md"
  ],
  "agent": {
    "build": {
      "mode": "primary"
    },
    "plan": {
      "mode": "primary",
      "permissions": {
        "write": "deny",
        "edit": "deny",
        "bash": "ask"
      }
    }
  }
}
```

---

## Step 10 — Done

Output exactly this:

---
Setup complete.

Created:
[list every file created or skipped]

**Key principle — one source, no duplication:**

  .agents/[name].md            ← edit agent instructions here
  .claude/skills/[name]/       ← edit skills here (all tools read this)
  .agents/management/          ← invoke by natural language

**Your workflow:**

  Drop idea in `.tasks/backlog/`         →  say "lets do this"
  SPEC drafted                           →  you approve
  Autonomous: runs until PR              →  you review + merge
  Guided:     say "next task" each step  →  you control pace

**Execution modes:**

  "lets do this"          →  autonomous (default)
  "lets do this (guided)" →  guided, you say "next task" between tasks

**Stop conditions (both modes):**

  Implementer blocked     →  stops, reports, waits for you
  Spec review BLOCKER     →  stops, reports, waits for you
  Quality review BLOCKER  →  stops, reports, waits for you

**Managing agents and skills:**

  "update agent [name]"   →  edit .agents/ + propagate to all tools
  "create agent [name]"   →  new agent everywhere
  "delete agent [name]"   →  remove from everywhere
  "create skill [name]"   →  new skill in .claude/skills/
  "update skill [name]"   →  edit .claude/skills/[name]/SKILL.md
  "show backlog"          →  list .tasks/backlog/
  "status"                →  all in-progress features

**Commit the setup:**
  git add . && git commit -m "chore: add AI workflow config"
---
