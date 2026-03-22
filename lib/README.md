# 📦 Linxmd Library

> The curated collection of reusable Agents, Skills, and Workflows that power every Linxmd-enabled project.

Install anything with a single command:

```bash
linxmd add workflow:sdd-tdd --yes
linxmd add agent:router --yes
linxmd add skill:context-management --yes
```

---

## ✨ What's Inside

- 🔄 **4 Workflows** — End-to-end pipelines for software development, content creation, and bug fixing
- 🤖 **10 Agents** — Focused specialists that each handle exactly one job
- 🛠️ **6 Skills** — Reusable capability modules shared across agents and workflows
- 🔗 **Dependency-aware** — Every install automatically resolves what else it needs
- 🏷️ **Versioned** — Semantic versions ensure compatibility and safe updates

---

## 🔄 Workflows

Workflows orchestrate multiple agents and skills into a complete, opinionated pipeline. Install a workflow and everything it needs comes with it.

| Artifact | Version | Description | Auto-installs |
|---|---|---|---|
| `workflow:sdd-tdd` | 0.2.0 | Full Spec-Driven Development with TDD — from spec to green tests to reviewed, documented code | 8 artifacts |
| `workflow:content-review` | 0.2.0 | Draft → fact-check → edit pipeline for high-quality content of any kind | 4 artifacts |
| `workflow:bug-fix` | 0.2.0 | Reproduce, fix, verify, and document a confirmed bug end-to-end | 5 artifacts |
| `workflow:artifact-factory` | 0.2.0 | Author new agents, skills, and workflows as reusable lib building blocks | 2 artifacts |

> `workflow:echo-test` is a smoke-test fixture — safe to ignore.

---

## 🤖 Agents

Agents are focused AI specialists you assign to one task. Mix and match them to build custom pipelines, or let workflows wire them together automatically.

| Artifact | Version | Description | Depends on |
|---|---|---|---|
| `agent:router` | 0.2.0 | Triages any request and routes it to the right workflow or agent | — |
| `agent:planner` | 0.2.0 | Decomposes a SPEC.md into a structured, sequenced TASKS.md | `skill:task-management` |
| `agent:test-writer` | 0.2.0 | Writes failing tests from acceptance criteria (RED phase) | — |
| `agent:implementer` | 0.2.0 | Writes minimal code until tests pass (GREEN phase) | `skill:debugging` |
| `agent:reviewer-spec` | 0.2.0 | Verifies every acceptance criterion is met before merge | — |
| `agent:reviewer-quality` | 0.2.0 | Audits code quality, design patterns, and security posture | — |
| `agent:docs-writer` | 0.2.0 | Updates READMEs, changelogs, and API docs after reviews pass | `skill:task-management` |
| `agent:drafter` | 0.2.0 | Produces a structured first draft from task context and target audience | — |
| `agent:editor` | 0.2.0 | Polishes content for clarity, flow, and consistency | — |
| `agent:fact-checker` | 0.2.0 | Verifies claims, links, numbers, and references in any document | — |

> `agent:echo-test` is a smoke-test fixture — safe to ignore.

---

## 🛠️ Skills

Skills are reusable capability modules that agents and workflows import. They encode best practices that would otherwise need to be re-explained in every prompt.

| Artifact | Version | Description | Depends on |
|---|---|---|---|
| `skill:task-management` | 0.2.0 | Backlog, spec files, and task-tracking conventions for any project | — |
| `skill:context-management` | 0.2.0 | Strategies for maintaining coherent context across long sessions and large codebases | — |
| `skill:debugging` | 0.2.0 | Systematic fault isolation with structured hypothesis tracking | — |
| `skill:refactoring` | 0.2.0 | Safe, test-backed refactoring with rollback checkpoints | `agent:implementer`, `agent:reviewer-quality` |
| `skill:observability` | 0.2.0 | Structured logging and reasoning traces for agentic pipelines | — |
| `skill:preview-delivery` | 0.1.0 | Build previews, share links or binaries, collect feedback, and iterate | — |

> `skill:echo-test` is a smoke-test fixture — safe to ignore.

---

## 🗺️ Dependency Map

```text
workflow:sdd-tdd
  ├── agent:planner          → skill:task-management
  ├── agent:test-writer
  ├── agent:implementer      → skill:debugging
  ├── agent:reviewer-spec
  ├── agent:reviewer-quality
  ├── agent:docs-writer      → skill:task-management
  ├── skill:task-management
  └── skill:preview-delivery

workflow:content-review
  ├── agent:drafter
  ├── agent:fact-checker
  ├── agent:editor
  └── skill:task-management

workflow:bug-fix
  ├── agent:implementer      → skill:debugging
  ├── agent:reviewer-spec
  ├── agent:reviewer-quality
  ├── agent:docs-writer      → skill:task-management
  └── skill:debugging

workflow:artifact-factory
  ├── skill:task-management
  └── skill:preview-delivery

skill:refactoring
  ├── agent:implementer      → skill:debugging
  └── agent:reviewer-quality
```

---

## 🔗 How Artifacts Work Together

### Software Development

The `workflow:sdd-tdd` is the flagship pipeline:

1. `agent:router` — understands the request and routes it to the right pipeline
2. `agent:planner` — breaks the spec into a sequenced task list (`TASKS.md`)
3. `agent:test-writer` — writes the failing tests (RED phase)
4. `agent:implementer` — writes code until tests pass (GREEN phase)
5. `agent:reviewer-spec` + `agent:reviewer-quality` — dual code review gate
6. `agent:docs-writer` — updates all documentation

For bug reproduction and fixing, `workflow:bug-fix` provides a tighter loop with `skill:debugging` for structured hypothesis tracking.

### Content & Documentation

The `workflow:content-review` pipeline:

1. `agent:drafter` — produces a structured first draft
2. `agent:fact-checker` — verifies every claim, link, and number
3. `agent:editor` — polishes for clarity, flow, and style

### Extending the Library

Use `workflow:artifact-factory` to author new agents, skills, and workflows and contribute them back.

---

## 🤝 Contributing

1. **Install the factory**: `linxmd add workflow:artifact-factory --yes`
2. **Scaffold** your new artifact using the factory workflow
3. **Add an entry** to `lib/index.json` with correct `version`, `deps`, and `tags`
4. **Open a pull request**

All artifacts follow the frontmatter schema defined in `src/Agentsmd/Models/FrontMatter.cs`. Use existing artifacts in `lib/` as reference implementations.

---

## 📌 Version

Library version: **0.2.0** · [Changelog](../CHANGELOG.md)
