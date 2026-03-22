# 📋 Lib Pimp Plan — v0.3.0 Roadmap

Research-backed planning for the next wave of Linxmd library artifacts.
Sources: GitHub awesome repos (anthropic/claude-cookbooks, openai/openai-agents-python, dair-ai/Prompt-Engineering-Guide, f/prompts.chat, microsoft/promptbase, reddit/r/ClaudeAI, r/ChatGPTCoding), community patterns, and existing lib gaps.

---

## 🗂️ Planning Questions — Resolved

### Q1 — `skill:code-translator` (translating code between languages)

**Decision: ✅ Add in v0.3.0**

There is massive demand for this. Python↔TypeScript, JS↔Rust, Python↔Go — teams constantly need to port codebases or rewrite libraries. The key challenge is *correctness verification*: a translation is only trustworthy if the same tests pass before and after.

**Proposed artifact:** `skill:code-translator`

Responsibilities:
1. Analyze source language idioms, patterns, and stdlib usage
2. Map to idiomatic equivalents in the target language (not literal syntax conversion)
3. Translate in logical chunks (module by module, not file by file)
4. Re-run or rewrite the existing test suite in the target language
5. Flag untranslatable constructs (language-specific features with no target equivalent) for manual review
6. Produce a translation report with diff statistics and open items

Depends on: `agent:test-writer`, `agent:implementer`, `agent:reviewer-quality`

---

### Q2 — `skill:translator` / `skill:i18n` (translation and i18n-ification)

**Decision: ✅ Add both in v0.3.0, as separate artifacts**

These are two distinct problems:

**`skill:text-translator`** — translates existing copy (docs, UI strings, marketing content) from one human language to another. Uses tone-preservation rules, glossary management, and a review pass.

**`skill:i18n`** — makes a codebase *internationalisation-ready*:
1. Audits all hardcoded strings in source code
2. Extracts them to locale resource files (`.json`, `.resx`, `.po`, etc.)
3. Replaces inline strings with lookup calls (`t('key')`, `Resources.Key`)
4. Generates base locale file (e.g., `en.json`)
5. Flags date/number/currency formatting assumptions
6. Verifies all keys are present in all locale files (no missing translations)

High demand validated by: GitHub job boards, i18n being the #1 "we always defer it" technical debt on Reddit discussions.

---

### Q3 — Design / visual asset skill

**Decision: ⚠️ Scoped add in v0.3.0 — `skill:design-tokens` only; image prompting deferred**

LLMs cannot generate images directly, but they can produce extremely valuable design artefacts:

**`skill:design-tokens`** — creates and maintains a design token system:
1. Audits existing CSS/SCSS for magic values (colors, spacing, typography)
2. Extracts tokens into a structured token file (`tokens.json` / CSS custom properties)
3. Generates a Tailwind config or CSS variable sheet from tokens
4. Flags inconsistencies (17 slightly different shades of grey)
5. Produces a visual token inventory document

AI image prompting (Midjourney, DALL-E, Flux prompt engineering) is interesting but niche and hard to verify automatically. Defer to v0.4.0 with a dedicated `skill:ai-image-prompts` if community requests it.

---

### Q4 — `skill:remotion` / video automation

**Decision: ❌ Not yet — defer to v0.4.0**

Remotion requires a Node.js runtime, `remotion` npm package, and a preview server. The skill would need to assume specific toolchain availability, making it unreliable as a general-purpose library artifact.

Revisit when:
- A "runtime requirements" system is added to the artifact frontmatter
- Community signals demand (GitHub Issues / Discussions)

Candidate future artifact: `workflow:product-video` — renders a Remotion project from a feature spec.

---

### Q5 — `workflow:quality-baseline` (project quality leveling)

**Decision: ✅ Strong add in v0.3.0 — flagship new workflow**

This is the most universally valuable addition. Every project needs a quality floor, and "raise quality to a baseline" is a task teams always intend to do but never structure properly.

**Proposed: `workflow:quality-baseline`**

Pipeline stages:

| Stage | What it checks / does |
|---|---|
| Test coverage | Runs coverage report, identifies uncovered paths, asks `agent:test-writer` to fill gaps above a threshold (e.g., 80%) |
| Static analysis | Runs linter/analyzer for the detected language, categorizes warnings by severity |
| Security scan | Checks deps for known CVEs (npm audit, dotnet outdated, pip-audit), flags OWASP Top 10 patterns |
| Documentation | Audits for missing READMEs, undocumented public APIs, stale changelogs |
| Observability | Verifies structured logging and error boundaries are in place (`skill:observability`) |
| CI/CD | Checks for presence and health of a CI pipeline (GitHub Actions, GitLab CI, etc.) |
| Summary report | Produces a `QUALITY_REPORT.md` with pass/fail per category and a prioritised action list |

Depends on: `agent:test-writer`, `agent:implementer`, `agent:reviewer-quality`, `skill:observability`, `skill:debugging`, `skill:task-management`

---

### Q6 — `skill:project-memory` / changelog management / RAG

**Decision: ✅ Add `skill:project-memory` + `agent:changelog-writer` in v0.3.0; runtime RAG deferred**

Large projects suffer from context amnesia — decisions made months ago get revisited, bugs are fixed twice, and new team members have no way to understand *why* things are the way they are.

**`skill:project-memory`** — a structured, file-based knowledge system:
1. Maintains `docs/decisions/` as an ADR (Architecture Decision Record) directory
2. Each significant decision gets a numbered ADR: `docs/decisions/0001-use-postgres.md`
3. Maintains `CHANGELOG.md` in [Keep a Changelog](https://keepachangelog.com/) format
4. Maintains a `docs/KNOWN_ISSUES.md` for open, deferred, and resolved issues
5. Provides search conventions so agents can find relevant history before making decisions

**`agent:changelog-writer`** — dedicated agent for CHANGELOG maintenance:
1. Reads closed tasks, merged PRs, or a diff summary
2. Categorizes changes (Added / Changed / Fixed / Removed / Security)
3. Appends a new version entry to `CHANGELOG.md`
4. Follows Keep a Changelog + SemVer conventions strictly

**Runtime RAG (sqlite-vec, embeddings, etc.):** Deferred. The complexity of running a vector database as a local dependency is too high for a general-purpose skill. File-based ADR + CHANGELOG covers 90% of the use case without any runtime infrastructure.

---

### Q7 — Specific domain agents (e.g., `agent:frontend-developer`)

**Decision: ❌ No — user instinct is correct**

Domain-specific agents like `agent:frontend-developer`, `agent:backend-developer`, or `agent:data-scientist` are too project-specific to be useful as general library artifacts.

The existing agent set (`implementer`, `reviewer-quality`, `reviewer-spec`) is deliberately language- and framework-agnostic. The project context (README, codebase, `SPEC.md`) supplies the domain knowledge; the agents supply the process structure.

Domain awareness should come from **skills**: `skill:design-tokens` for frontend, `skill:database-migrations` for backend, etc. — not from agents.

---

### Q8 — Artifact packs (bundle installs)

**Decision: ✅ Add in v0.3.0 — index-only bundles, no new files**

Packs are a pure index-level concept. Instead of:

```bash
linxmd add workflow:sdd-tdd --yes
linxmd add agent:router --yes
linxmd add skill:context-management --yes
```

A user does:

```bash
linxmd add pack:fullstack-tdd --yes
```

**Design — deliberately minimal:**

A pack is just a new `type` entry in `lib/index.json` with an `artifacts` array. No `packs/` directory, no YAML manifests, no new file schema.

```json
{
  "name": "fullstack-tdd",
  "type": "pack",
  "version": "0.3.0",
  "description": "Everything you need for spec-driven TDD",
  "artifacts": [
    "workflow:sdd-tdd",
    "agent:router",
    "skill:context-management",
    "skill:observability"
  ]
}
```

**How deps work:** Packs carry no `deps` array of their own. Each listed artifact already has its own `deps`. `linxmd add pack:X` expands the `artifacts` list and runs the normal install path for each entry — transitive dep resolution happens automatically. The pack itself is never recorded in `installed.json`; only the individual artifacts are.

This means: zero new artifact files, zero new dep resolution logic. The only CLI change is recognizing `type: "pack"` in the index and expanding the `artifacts` list before the normal install loop.

**Candidate packs:**
| Pack | Contents |
|---|---|
| `pack:fullstack-tdd` | `workflow:sdd-tdd` + `agent:router` + `skill:context-management` + `skill:observability` |
| `pack:content-pipeline` | `workflow:content-review` + `agent:router` + `skill:task-management` |
| `pack:quality-sprint` | `workflow:quality-baseline` + `skill:project-memory` + `agent:changelog-writer` |
| `pack:i18n-ready` | `skill:i18n` + `skill:text-translator` + `skill:task-management` |

---

### Q9 — Friendlier, app-store-style descriptions

**Decision: ✅ Do now — part of v0.3.0**

Current descriptions are functional but dry. App-store-style descriptions lead with the *benefit*, not the mechanism. Compare:

| Current | Improved |
|---|---|
| "Updates documentation after reviews pass" | "Keeps your docs in sync — updates READMEs, changelogs, and API docs the moment code review passes" |
| "Systematic debugging with hypothesis tracking" | "Never chase the same bug twice — structured fault isolation with a hypothesis log that survives context resets" |
| "Verifies factual claims, links, numbers, and references in content drafts" | "Your content's last line of defence — catches broken links, wrong numbers, and unverifiable claims before they reach readers" |

All 20 production artifact descriptions in `lib/index.json` should be rewritten. This has a real user impact: descriptions appear in `linxmd list` output and in the onboarding prompt.

**Action:** Rewrite all descriptions as part of the v0.3.0 batch.

---

### Q10 — `workflow:bug-fix` pipeline improvements

**Decision: ✅ Improve in v0.3.0**

The current `workflow:bug-fix` is missing two critical steps:

1. **Regression test** — after the fix is implemented, `agent:test-writer` must write a test that *specifically tests the bug scenario*. Without this, the bug can silently regress.

2. **Changelog entry** — every bug fix should produce a `Fixed` entry in `CHANGELOG.md`. Currently the workflow ends after docs-writer, with no changelog step.

**Proposed updated pipeline:**

```text
reproduce → implement fix → write regression test → reviewer-spec → reviewer-quality → docs-writer → changelog-writer
```

Updated deps for `workflow:bug-fix`:
- Add: `agent:test-writer@>=0.2.0`
- Add: `agent:changelog-writer@>=0.3.0` (once that agent exists)

---

## 🔭 Additional Artifacts (Research-Derived)

Beyond the 10 planning questions, research across `anthropic/claude-cookbooks`, `openai/openai-agents-python`, and community repos surfaced these high-value additions:

### Skills

| Artifact | Priority | Description |
|---|---|---|
| `skill:api-design` | 🔥 High | REST API design conventions: OpenAPI spec writing, versioning strategies, error response schemas, pagination patterns |
| `skill:security-review` | 🔥 High | OWASP Top 10 checklist, secret scanning patterns, dependency CVE audit, auth flow review |
| `skill:database-migrations` | 🟡 Medium | Schema change management, rollback plans, forward-only migration conventions, seed data hygiene |
| `skill:accessibility` | 🟡 Medium | WCAG 2.2 compliance audit for frontend components, aria attributes, keyboard navigation, colour contrast |
| `skill:code-review-conventions` | 🟡 Medium | Team code review standards: what to check, how to comment, what to approve vs. request changes on |
| `skill:performance-profiling` | 🟢 Low | Profile-first workflow, flamegraph interpretation, identifying hot paths before optimising |

### Agents

| Artifact | Priority | Description |
|---|---|---|
| `agent:changelog-writer` | 🔥 High | Maintains `CHANGELOG.md` — reads task/diff summary, categorises by Added/Changed/Fixed/Removed/Security, appends version entry |
| `agent:architect` | 🔥 High | Makes and records significant architecture decisions as ADRs in `docs/decisions/` |
| `agent:security-auditor` | 🟡 Medium | Dedicated OWASP / threat-model reviewer — separate from `reviewer-quality` which is more about code style |
| `agent:scribe` | 🟡 Medium | General-purpose note-taker and meeting/session summariser — produces structured Markdown summaries |

### Workflows

| Artifact | Priority | Description |
|---|---|---|
| `workflow:quality-baseline` | 🔥 High | (See Q5 above) — comprehensive project quality audit and improvement pipeline |
| `workflow:release` | 🔥 High | Full release pipeline: SemVer bump → CHANGELOG entry → docs update → git tag → release notes |
| `workflow:onboarding-new-project` | 🟡 Medium | Assess a new codebase: detect tech stack, document architecture, identify quick wins, propose workflow installs |
| `workflow:dependency-update` | 🟡 Medium | Automated dependency update cycle: detect outdated deps → update → run tests → review → PR |

---

## 📅 Proposed Release Plan

### v0.3.0 — "Quality & Knowledge"

**New skills (6):**
- `skill:code-translator`
- `skill:text-translator`
- `skill:i18n`
- `skill:design-tokens`
- `skill:project-memory`
- `skill:api-design`

**New agents (2):**
- `agent:changelog-writer`
- `agent:architect`

**New workflows (2):**
- `workflow:quality-baseline`
- `workflow:release`

**New packs (4):**
- `pack:fullstack-tdd`
- `pack:content-pipeline`
- `pack:quality-sprint`
- `pack:i18n-ready`

**Improvements:**
- `workflow:bug-fix` — add regression test step + changelog step
- All 20 production descriptions rewritten in app-store tone

**Total new artifacts: 14** (10 installable + 4 index-only packs) · Target: stable lib with 33 production artifacts + 4 packs

---

### v0.4.0 — "Security & Automation"

**New skills (3):**
- `skill:security-review`
- `skill:accessibility`
- `skill:database-migrations`

**New agents (2):**
- `agent:security-auditor`
- `agent:scribe`

**New workflows (2):**
- `workflow:onboarding-new-project`
- `workflow:dependency-update`

**New packs (2+):**
- `pack:security-hardening`
- `pack:new-project`

**Total new artifacts: 7** · Target: 40 production artifacts + 6+ packs

---

## 🔑 Key Principles for New Artifacts

1. **Benefit-first descriptions** — lead with the outcome, not the mechanism
2. **Dependency discipline** — don't add deps that aren't strictly needed; prefer zero-dep skills
3. **Zero runtime assumptions** — skills must work with only an LLM; no CLI tools assumed unless stated in frontmatter
4. **Test-first development** — every new artifact gets an E2E test in `LibV2E2ETests.cs` before merge
5. **Minimal surface area** — one skill does one thing; resist the urge to make skills multi-purpose
6. **Verification by default** — every workflow that mutates code must include a test or review gate
