# Final Changes Before Public — linxmd v0.3.0

Last honest review before users start depending on this. Grouped by severity.

---

## Q1: Project Memory — Markdown vs Brain/RAG

**Short answer: markdown files are fine. No RAG needed.**

The collision concern (workflows stepping on each other) is real but the solution is coordination protocols, not infrastructure.

**Why RAG would be overkill here:**
- Requires a running embedding model or external API (cost, latency, infra dependency)
- Every AI tool — Copilot, Claude Code, OpenCode — can already read markdown files natively; they function as the RAG themselves
- The context window of modern models easily handles dozens of ADRs and a full CHANGELOG
- Files are version-controlled; git provides conflict resolution for free

**What IS missing (and where collisions actually happen):**
The real gap is not storage format — it's that `sdd-tdd` and `bug-fix` are disconnected from `changelog-writer`. You can complete an entire feature cycle and produce zero CHANGELOG entry unless you explicitly know to call the changelog agent.

**Fix:** Wire changelog-writer as an optional final step in `sdd-tdd` (see item 8 below). The `task-management` skill's NOTES.md already provides per-session durable state. `project-memory` is the right place for cross-session ADRs and changelogs. The current architecture is correct.

---

## 🔴 CRITICAL — Blockers (fix before any public release)

### 1. `generate-index.py` will destroy all pack entries on next CI push

**Problem:** `generate-index.yml` runs on every push to `lib/**`. The script scans `agents/*.md`, `skills/*/SKILL.md`, and `workflows/*.md` only. It does NOT scan packs. On next push, it will regenerate `lib/index.json` and wipe all 4 pack entries. The `"artifacts"` field also won't be preserved.

**Fix options (choose one):**
- **A (recommended):** Create `lib/packs/` directory. Add one `.md` file per pack with YAML frontmatter that includes an `artifacts:` list field. Update `generate-index.py` to also scan `packs/*.md`.
- **B:** Seed a `lib/packs.json` file and merge it into the generated index.
- **C (stop-gap):** Change the `generate-index.yml` trigger from `lib/**` to only trigger for specific non-pack paths OR make the script merge instead of replace.

Option A is cleanest — pack definitions get YAML frontmatter just like everything else, they become version-controllable, and the script handles them uniformly.

**Additional: update `generate-index.py` to write the `"artifacts"` array for packs.**

### 2. CLI version is `0.2.0` — bump to `0.3.0`

`src/Linxmd/Linxmd.csproj` line 12: `<Version>0.2.0</Version>` — update to `0.3.0`.

The auto-updater compares semver; if the CLI is `0.2.0` and the release tag is `v0.3.0`, it will immediately prompt every user to update.

### 3. CHANGELOG not updated for v0.3.0

`CHANGELOG.md` documents `0.1.0` and `0.2.0` only. The public-facing changelog should capture all the v0.3.0 work (10 new artifacts, pack support, 4 packs, updated descriptions).

### 4. Router agent doesn't know about v0.3.0 workflows

`lib/agents/router.md` routing table was written for v0.2.0. Missing:
- `workflow:quality-baseline` — how do users route "audit my project" or "raise quality"?
- `workflow:release` — how does a user route "release this"?
- `workflow:bug-fix` is already there ✓

### 5. Temp test output files are committed

`full-test.txt`, `full-err.txt`, `v3-out.txt`, `v3-err.txt`, `quick-test.txt`, `quick-err.txt`,  `test-results.txt` are literally in the root of the repo right now. Add `*.txt` or the specific files to `.gitignore` and delete them.

### 6. `lib-pimp-plan.md` is an internal planning doc in the repo root

Either delete it or move it to `docs/` before going public. Users don't need to see it.

---

## 🟡 HIGH — Strong recommendations

### 7. Echo-test artifacts pollute the browse view

When a user runs `linxmd add`, they see:
```
echo-test   agent    0.1.0   Test-only agent for smoke tests — safe to delete
echo-test   skill    0.1.0   Test-only skill for smoke tests — safe to delete
echo-test   workflow 0.1.0   Test-only workflow for smoke tests — safe to delete
```

This looks amateur. Options:
- **A (recommended):** Add `"internal": true` to `ArtifactEntry`, filter from `IndexParser.Search()` by default. Tests override by querying directly. Cleanest.
- **B:** Add an `echo-test` source tag and filter `tag:smoke` from browse. Worse — "smoke" is a legitimate search term.
- **C:** Delete echo-test from the public lib entirely, move to a local test fixture. Tests set up their own local source pointing to a test-fixtures lib. Most work but cleanest user experience.

Option A is 1 model property + 1 filter line — fast to implement.

### 8. `sdd-tdd` produces no CHANGELOG entry by default

After completing an entire feature cycle via `sdd-tdd`, there is zero update to `CHANGELOG.md` unless the user independently knows to run `changelog-writer`. Add a CHANGELOG step as the last step before COMMIT:

```markdown
7. **CHANGELOG** → `changelog-writer` (if installed) → Append `Added` entry to `CHANGELOG.md`
   - Skipped silently if `agent:changelog-writer` is not installed
   - Format: `- Add [feature name] ([spec reference])`
```

Also add `agent:changelog-writer@>=0.3.0` as a soft/optional dep (or just note it as "recommended").

### 9. No `dotnet tool` install path — biggest adoption barrier

Right now, the only install path is "download binary from GitHub Releases." `dotnet tool install -g linxmd` doesn't work. For .NET developers (the primary audience), this is the expected install pattern.

What's needed in `Linxmd.csproj`:
```xml
<PackAsTool>true</PackAsTool>
<ToolCommandName>linxmd</ToolCommandName>
<PackageId>linxmd</PackageId>
```
Plus a NuGet publish step in the release pipeline.

Note: `PublishSingleFile=true` and `PackAsTool=true` are mutually exclusive in the same build — you need a separate `pack` target or a separate project configuration. The existing binary releases remain for users who don't have .NET installed.

### 10. `linxmd source` command is missing

Users can't manage sources via the CLI — they must manually edit `.linxmd/sources.json`. For the multi-source scenario (private artifact repos), this is a real gap:

```bash
linxmd source list
linxmd source add --id company --kind github --owner myorg --repo my-agents --branch main --path lib
linxmd source remove company
```

### 11. `UpdateNotifier` timeout is 500ms — regularly fails on corporate networks

`UpdateNotifier.cs` line 31: `new CancellationTokenSource(TimeSpan.FromMilliseconds(500))`. On a loaded machine or corporate proxy, this routinely times out silently. Increase to 2000ms or 3000ms. The update check runs in parallel with command execution so it's not blocking.

---

## 🟢 MEDIUM — Worth doing but not blockers

### 12. `refactoring` skill declares hard deps on agents

`lib/skills/refactoring/SKILL.md` declares `agent:implementer` and `agent:reviewer-quality` as deps. A skill should not declare agent deps — this creates circular dependency potential (agents can depend on skills, but skills depending on agents is an inverted coupling). Skills provide knowledge and protocols, not call stacks.

Fix: remove deps from `skill:refactoring`'s frontmatter and document them as "works best with" in the body text instead.

Corresponding `lib/index.json` entry needs the `deps` array cleared.

### 13. `GetArtifactDir` throws `ArgumentException` for unknown type strings

`InstalledStateManager.cs`: if any future code accidentally passes `"pack"` to `GetArtifactDir`, it throws. Currently safe because `InstallArtifactAsync` returns early for packs. But it's a latent bug. Add a `"pack" => string.Empty` case or a safe fallback.

### 14. `artifact-factory` workflow lists `skill:preview-delivery` as hard dep

The `preview-delivery` skill is thin (v0.1.0) and mostly irrelevant for developers building CLI tools or backend code. The `artifact-factory` works fine without it. Make `preview-delivery` optional — document it in the workflow body as "useful if you're delivering web previews" rather than a hard dep.

Remove `skill:preview-delivery@>=0.1.0` from `artifact-factory`'s frontmatter deps.

### 15. Workflows are not synced — this is undocumented

`SyncEngine` only syncs agents (and skills for Claude Code). Workflows are installed to `.linxmd/workflows/` but never synced to `.github/agents/`, `.claude/agents/`, etc. This is intentional (they're orchestration documents, not callable tools), but users discovering this are confused. Add a note to the `linxmd sync --help` output or the README.

### 16. `lib/README.md` doesn't include v0.3.0 artifacts

The lib catalog README was built for pre-v0.3.0. It needs the new skills, agents, workflows, and packs documented.

### 17. No `osx-x64` release binary — Intel Mac users are excluded

`release.yml` only builds `osx-arm64` (Apple Silicon). Intel Macs still exist. Add `osx-x64` to the release matrix.

---

## 📝 CONTENT — Lib artifact quality

### 18. `sdd-tdd` has no SESSION GUARD / collision prevention

If two agent sessions run `sdd-tdd` concurrently against the same project, they can both grab the same backlog item. Add at START CONDITIONS:

```markdown
## Collision Guard
Before creating a new in-progress item, check `.linxmd/tasks/in-progress/` for existing active tasks. If an in-progress folder already exists for the same backlog item, either find a different task or pause and notify the user.
```

### 19. `router.md` routing table needs pagination and release entries

Add to the routing table:
```
"release", "cut a release", "prepare v[x]" → workflow:release
"audit quality", "run quality check" → workflow:quality-baseline
```

### 20. `fact-checker.md`, `editor.md`, `drafter.md` — bodies are very thin

These content agents do the job but their prompts are sparse. A user installing them with no context gets very little guidance. Worth fleshing out before public (longer process descriptions, output formats, edge cases).

### 21. `preview-delivery` skill body is sparse

`lib/skills/preview-delivery/SKILL.md` is v0.1.0 and minimal. Either flesh it out or version-gate it as an experimental feature. Currently referenced by `sdd-tdd` (dep) and `artifact-factory`, which means users installing those get this thin artifact.

---

## 🧪 TESTS

### 22. `ComprehensiveCliE2ETests` tests assume echo-test is in browse results

`Add_Browse_WorksWithoutInit` searches for `echo-test`. If echo-test is hidden from browse (item 7 above), add a `--include-internal` flag to the add command for test use.

### 23. No test for UpdateNotifier `NormalizeVersion`

Quick unit test for `v0.3.0` → `0.3.0`, `0.3.0+build.1` → `0.3.0`, `invalid` → null. Currently zero coverage on this logic.

### 24. No test for `linxmd add` with multiple items sharing the same name across types

`linxmd add echo-test` currently returns multiple matches (agent + skill + workflow all named `echo-test`). The "ambiguous match" code path (non-interactive `--yes` mode) shows an error message — this is tested indirectly but not explicitly as a unit test.

---

## 📐 ARCHITECTURE — Project Memory at Scale

The user asked: should `skill:project-memory` use RAG, vector search, or SQLite instead of pure markdown? And can it scale to huge projects without filling the LLM context window?

### The Real Problem Statement

The fear is: "500 ADRs in a mature project = AI loads all of them = context explosion."  
This fear is misplaced. The problem is not storage format — it is **access protocol**.

What actually happens with good markdown + protocol:
1. AI loads `docs/decisions/README.md` (the index table) — ~3KB, trivial
2. AI greps the index titles for relevant keywords — finds ADR-0042, ADR-0107
3. AI loads only those 2 files — maybe 4KB total
4. AI never loads the other 498 ADRs

At 500 ADRs the index is perhaps 25KB (still loadable). At 2,000 ADRs the index is ~100KB — impractical.

**Markdown at scale breaks when:**
- ADR count exceeds ~400 (index becomes too large to skim)
- CHANGELOG has thousands of entries (full read is impossible)
- KNOWN_ISSUES table grows beyond ~150 rows
- You want aggregate insight: "how many auth-related decisions have we made?"

This is a realistic concern for a 3+ year active project with heavy ADR discipline.

### Option A — Improved Markdown Protocols (v0.3.0, free, do now)

Update `skill:project-memory/SKILL.md` with explicit selective-load rules:

```markdown
## Selective Load Protocol
1. Load `docs/decisions/README.md` index first — never load all ADRs at once
2. Grep the index titles for keywords relevant to your task before opening any ADR file
3. For CHANGELOG: load only the `[Unreleased]` block — never read full history unless explicitly asked
4. For KNOWN_ISSUES: filter to rows where Status = Open — ignore Resolved rows
5. Hard cap: load at most 3 full ADR files per decision — if you need more, the decision needs more research than AI can provide in one context
```

Also add an index size warning: "If `decisions/README.md` exceeds 200 rows, create `decisions/README-archive.md` for older ADRs."

**Verdict:** Works today. Costs zero. Handles 90% of real projects. Implement now.

### Option B — SQLite + FTS5 as Derived Index (v0.4.0, recommended plan)

SQLite FTS5 is full-text search built into SQLite (zero external dependencies). The architecture:

- `docs/decisions/*.md`, `CHANGELOG.md`, `KNOWN_ISSUES.md` **remain the source of truth** — committed, human-readable, git-friendly
- `.linxmd/memory.db` — a derived index, in `.gitignore`, rebuilt on demand from the markdown files
- New CLI commands:
  - `linxmd memory index` — (re)builds the SQLite index from all markdown memory files
  - `linxmd memory search <query>` — FTS5 keyword search, returns top-5 file paths + titles + snippets  
  - `linxmd memory stats` — "347 decisions, 52 open issues, 1,200 changelog entries" — no file loading
  - `linxmd memory recent [--type decision|issue|changelog] [--limit N]` — most recent N entries

The project-memory SKILL protocol changes: instead of "load index, grep, load file", it becomes:
```
1. Run: linxmd memory search "<keywords>"
2. Load only the 1-3 files returned
3. Make decision
```

**Why this is better at real scale:**
- AI gets 5 search results in ~200 tokens without loading a single file
- Aggregate queries work: `linxmd memory stats --type decision --tag auth`
- Index is fast (FTS5 is sub-millisecond on millions of rows)
- One NuGet package: `Microsoft.Data.Sqlite` (~500KB)

**Implementation cost:** ~400 lines of C# + 3 CLI subcommands. Medium effort.

**Verdict:** Correct v0.4.0 feature. Not für jetzt — but plan it explicitly so the SKILL.md protocol points toward it.

> **Update:** `linxmd memory index/search/stats/recent` was shipped in v0.3.0. SQLite + FTS5 via `Microsoft.Data.Sqlite` 9.0.3, cross-platform (win/linux/mac), single `.linxmd/memory.db` file that is git-ignorable. The SKILL.md selective-load protocol was also updated to describe both the CLI and pure-markdown paths.

### Option C — Vector Embeddings / Semantic Search (v0.5.0+, optional plugin)

Semantic search ("find decisions conceptually similar to this authentication problem") is compelling but:
- Requires: Ollama (~500MB + local GPU) or OpenAI API (cost, privacy, internet dependency)
- Fundamentally changes linxmd's scope from "package manager" to "AI infrastructure tool"
- Most keyword-based FTS5 searches cover 95% of real use cases anyway

**Verdict:** Explicitly out of scope for the core CLI. Could be a plugin or integration with existing tools (Cursor, Copilot Workspace) that already do this. Add to "won't build" list.

### Recommendation for v0.3.0

1. **Update `project-memory/SKILL.md` now** with the selective-load protocol from Option A — 30 minutes of work, immediate benefit.
2. **Update the anti-bloat list** — change the existing "❌ RAG rejected" entry to reflect the nuanced answer: FTS5 in v0.4.0 is reasonable; vector embeddings are not.
3. **Note in SKILL.md frontmatter**: "v0.4.0 will add `linxmd memory search` for projects with >400 ADRs."

---

## ✅ ARTIFACT QUALITY STANDARD — Attention to Detail

### The Quality Bar (every production artifact must pass)

An artifact is production-quality when it passes all five of these:

| # | Check | What it means |
|---|-------|---------------|
| 1 | **Trigger phrases** | Explicit list of phrases that invoke this artifact — users and routing agents know exactly when to use it |
| 2 | **Concrete process** | Numbered steps with actionable verbs (not "consider X" — do "run X", "write X", "check X") |
| 3 | **Output contract** | Exact format of what the artifact produces — VERDICT format, template with sections, specific file names |
| 4 | **When NOT to use** | Explicit anti-cases pointing to better alternatives — prevents misuse and scope creep |
| 5 | **Rules / constraints** | Named constraints that prevent the most common failure modes |

### Current Audit

| Artifact | Type | Trigger | Process | Output | When NOT | Rules | Status |
|----------|------|---------|---------|--------|----------|-------|--------|
| `sdd-tdd` | workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `bug-fix` | workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `quality-baseline` | workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `release` | workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `content-review` | workflow | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `artifact-factory` | workflow | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `echo-test` | workflow | internal | ✅ | ✅ | n/a | ✅ | ✅ |
| `implementer` | agent | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `reviewer-quality` | agent | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `reviewer-spec` | agent | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `docs-writer` | agent | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `changelog-writer` | agent | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `architect` | agent | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `drafter` | agent | ❌ | partial | partial | ❌ | ❌ | ❌ |
| `editor` | agent | ❌ | partial | partial | ❌ | ❌ | ❌ |
| `fact-checker` | agent | ❌ | ✅ | ✅ | ❌ | ❌ | ⚠️ |
| `router` | agent | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `task-management` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `project-memory` | skill | ✅ | ✅ | ✅ | ✅ | partial | ⚠️ |
| `debugging` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `feature` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `refactoring` | skill | ✅ | ✅ | ✅ | partial | ✅ | ⚠️ |
| `api-design` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `code-translator` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `i18n` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `text-translator` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `design-tokens` | skill | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `preview-delivery` | skill | ❌ | partial | partial | ❌ | ❌ | ❌ |
| `echo-test` | skill | internal | ✅ | ✅ | n/a | ✅ | ✅ |

**Summary:** 16 ✅ passing, 9 ⚠️ minor gaps, 3 ❌ failing (`drafter`, `editor`, `preview-delivery`)

### What Specifically Is Wrong and How to Fix It

**`drafter.md` (❌)**  
Missing: trigger phrases ("write a first draft", "draft this", "create initial content"), `When NOT to Use` section (don't use for technical specs → use `implementer`; don't use for content that already exists → use `editor`), explicit rules ("produce complete prose, never bullet fragments unless asked", "flag every assumption"). Body is 8 lines; needs ~25.

**`editor.md` (❌)**  
Missing: trigger phrases ("edit this", "clean this up", "improve readability", "refine this draft"), `When NOT to Use` section (not for fact-checking → `fact-checker`; not for creating content from scratch → `drafter`), rules ("never change factual claims without flagging them", "never restructure technical content near deadline without author confirmation"). Body is 8 lines; needs ~25.

**`preview-delivery/SKILL.md` (❌)**  
This skill is v0.1.0 and the body is a rough outline, not a production skill. Missing: trigger phrases, explicit infrastructure requirements (tunneling tool needed — ngrok/localtunnel/vite preview), When NOT to Use, rules about what constitutes a valid "feedback received" signal. Either flesh it out or mark as `experimental: true` in frontmatter and strip it from the `sdd-tdd` default deps.

**⚠️ Minor gaps (add `When NOT to Use` section to each):**  
`content-review`, `implementer`, `reviewer-quality`, `reviewer-spec`, `docs-writer`, `fact-checker`, `refactoring`  
These are solid everywhere else — each just needs 3-4 bullet points in a `## When NOT to Use` block. Low effort, high signal for users who install them blind.

**`project-memory/SKILL.md` (⚠️)**  
Missing: the selective-load protocol described above. Existing rules are good but the scale protocol is absent. Also missing: "When NOT to Use" (don't record decisions that apply to ALL projects — those belong in a standard/convention, not a project ADR).

### Fix Priority

1. **`drafter.md`** — flesh out fully (15 min) — it's the entry point of `content-review` workflow
2. **`editor.md`** — flesh out fully (15 min) — same workflow
3. **`preview-delivery/SKILL.md`** — either flesh out or mark experimental + remove from sdd-tdd hard deps (see item 14)
4. **`When NOT to Use` on 6 agents** — 5 min each, 30 min total
5. **`project-memory/SKILL.md`** — add selective-load protocol (10 min)

---

## 🔒 STABLE FOUNDATIONS — Kernel API Stability

### The Question

Are `skill:task-management` and `skill:project-memory` stable enough that every other workflow and agent can safely depend on them? Can users build on top of them without fear of breaking changes?

### The Answer: Yes — and this should be explicit, not assumed

These two skills are the kernel of the entire agent system:
- `sdd-tdd`, `bug-fix`, `quality-baseline`, `release`, `content-review`, `artifact-factory` — all depend on task-management
- `sdd-tdd`, `bug-fix`, `architect`, `changelog-writer` — all use project-memory patterns

If the file structure changes, every workflow that says "write to `NOTES.md`" or "check `docs/decisions/`" breaks silently. This is the same risk as changing a kernel syscall ABI.

### What the Stability Guarantee Covers

**task-management — the public API surface:**
```
.linxmd/tasks/backlog/           ← directory name = API
.linxmd/tasks/in-progress/       ← directory name = API
.linxmd/tasks/in-progress/<n>/SPEC.md    ← file name = API
.linxmd/tasks/in-progress/<n>/TASKS.md   ← file name = API
.linxmd/tasks/in-progress/<n>/NOTES.md   ← file name = API
```
The format of the frontmatter keys in each file (What we're building, Acceptance Criteria, etc.) is also API surface — removing or renaming a key silently breaks every workflow that reads it.

**project-memory — the public API surface:**
```
docs/decisions/              ← directory name = API
docs/decisions/README.md     ← file name = API (index)
docs/decisions/[NNNN]-*.md   ← naming pattern = API
CHANGELOG.md                 ← file name = API (at root)
KNOWN_ISSUES.md              ← file name = API (at root)
```

### Versioning Contract (add to both SKILL.md files)

Propose adding to both skills:

```markdown
## Stability Contract

This skill's file structure is a **public API**. Every workflow that uses task-management 
or project-memory depends on these exact paths and formats.

Compatibility rules:
- File/directory names → never renamed without a major version bump
- Frontmatter key names → never removed or renamed without a major version bump  
- New optional fields may be added freely (additive changes are non-breaking)
- Protocol steps may be improved freely (behavior clarification is non-breaking)
- Structural changes require: (1) semver major bump, (2) migration note in CHANGELOG, 
  (3) update to all dependent workflows and agents
```

### How This Affects Development

In practice: **treat these skills like `package.json` format or `pyproject.toml`**. Nobody just renames fields because it's cleaner — they maintain compatibility. The same discipline applies here.

For v0.4.0 SQLite memory: this is purely **additive** — the markdown files remain exactly where they are, and SQLite is an optional layer on top. No stability violation.

---

## 🏗️ ARCHITECTURE / FUTURE (post-v0.3.0, not for now)

These are observations for the roadmap, not immediate tasks:

- **`linxmd search` as a separate command** — `linxmd add` doing double duty as browse/install is clever but unusual. A dedicated `linxmd search` that opens a read-only browse + copy-to-clipboard would be V0.4 material.
- **Artifact integrity verification** — installed artifacts have checksums in `installed.json` but `linxmd sync` never verifies them. A `linxmd doctor` command could detect tampered files.
- **`skill:refactoring` sub-skills** — refactoring is a huge domain. Extract method, rename, extract to service, etc. could each be a focused skill vs one omnibus SKILL.md.
- **Platform-specific sync for workflows** — some tools (Cursor, Aider) have their own workflow file format. Expanding SyncEngine is a natural next step.
- **Publish date / deprecation fields in `ArtifactEntry`** — useful for lifecycle management.

---

## Priority Order for Execution

```
-- CRITICAL (must be done before any public release) --
1. Create lib/packs/ directory + update generate-index.py          [CRITICAL]
2. Bump CLI version to 0.3.0                                        [CRITICAL]
3. Write CHANGELOG v0.3.0 entry                                     [CRITICAL]
4. Update router agent routing table                                [CRITICAL]
5. Fix .gitignore + delete temp txt files                           [CRITICAL]
6. Delete/move lib-pimp-plan.md                                     [CRITICAL]

-- HIGH (do before announcing to any audience) --
7. Hide echo-test from browse (internal flag)                       [HIGH]
8. Add CHANGELOG step to sdd-tdd                                    [HIGH]
9. Add dotnet tool packaging                                        [HIGH]
10. Add `linxmd source` command                                     [HIGH]
11. Increase UpdateNotifier timeout                                 [HIGH]

-- CONTENT (artifact quality — do in this order) --
12. Flesh out drafter.md (trigger phrases, rules, When NOT to Use) [CONTENT]
13. Flesh out editor.md (trigger phrases, rules, When NOT to Use)  [CONTENT]
14. Fix/mark-experimental preview-delivery, remove from sdd-tdd    [CONTENT]
15. Add When NOT to Use to: content-review, implementer,           [CONTENT]
    reviewer-quality, reviewer-spec, docs-writer, fact-checker
16. Update project-memory SKILL.md: selective-load protocol +      [CONTENT]
    stability contract
17. Update task-management SKILL.md: stability contract            [CONTENT]
18. Add SESSION GUARD to sdd-tdd                                    [CONTENT]

-- MEDIUM (polish) --
19. Fix refactoring skill deps                                      [MEDIUM]
20. Update lib/README.md for v0.3.0                                 [MEDIUM]
21. Add osx-x64 release binary                                      [MEDIUM]

-- TESTS --
22. Add NormalizeVersion unit tests                                 [TESTS]

-- FUTURE (v0.4.0 roadmap) --
23. `linxmd memory export` (markdown export of the SQLite index)    [v0.4.0]
```

---

## What to NOT add (anti-bloat checklist)

Things considered and rejected for v0.3.0:

- ❌ Vector search / semantic embeddings for project memory — requires Ollama or API, changes linxmd's scope, keyword FTS5 covers 95% of use cases (plan for v0.4.0 as optional)
- ❌ `linxmd memory` SQLite commands in v0.3.0 — correct direction, wrong time; improved markdown protocols handle v0.3.0 scale
- ❌ `linxmd search` as separate command — `linxmd add <query>` works; deduplicate later
- ❌ GUI / web UI — CLI is the right primitive for this tool
- ❌ Artifact dependency graph visualization — interesting but zero user requests
- ❌ Multi-agent orchestration / DAG runner — scope creep; linxmd is a package manager, not a runtime
- ❌ Cloud sync for installed state — local-first is the feature; don't undo it
- ❌ `linxmd memory export` in v0.3.0 — no SQLite to export from yet
