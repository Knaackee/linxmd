# project-memory SKILL — Pre-v1 Plan

> Target: `version: 1.0.0` — a skill where the CLI is the primary interface and every agent
> can use it correctly from first principles with zero guessing.

---

## Problem Inventory

### P1 — Mental model is Markdown-first, CLI-secondary (CRITICAL)

The current structure puts 90% of the body on format rules (ADR template, CHANGELOG format,
KNOWN_ISSUES tables) and buries the CLI in a subsection of "Search Protocol".
The actual value-add of this skill is the `linxmd memory` pipeline.
A new agent reading this skill will learn Markdown conventions, look at the CLI section
as an "advanced optional" and fall back to manual file-reading.

**Fix:** Restructure the skill. CLI commands first, format reference second.

---

### P2 — No agent session protocol (CRITICAL)

The skill never answers: **"When exactly should I call `linxmd memory`?"**
There's no protocol for:
- Session start → is the index fresh?
- Before an architectural decision → mandatory search
- After recording a decision → re-index
- What to do when the index doesn't exist yet

**Fix:** Add an explicit "Agent Protocol" section with ordered steps.

---

### P3 — `linxmd memory index` timing undefined

The skill says "Build or refresh the index whenever memory files change" — but agents
can't watch files. They need a concrete rule:

> At the start of every session where memory may be consulted, run `linxmd memory index`
> once to ensure the index is fresh. It is idempotent and fast (full rebuild from scratch).

**Fix:** Add concrete timing rule to the protocol.

---

### P4 — `stats` output example is factually wrong

Skill shows:
```
# "42 decisions, 8 issues, 120 changelog entries"
```
Actual CLI output:
```
  changelog        120
  decision          42
  issue              8
```
(Sorted alphabetically by type, one line each — not a single sentence.)

**Fix:** Replace with the actual output format.

---

### P5 — `--limit` / `-n` option undocumented

Both `linxmd memory search` (default: 5) and `linxmd memory recent` (default: 10) accept
`--limit` / `-n`. Agents need this to adjust result counts.

**Fix:** Document the option in the command reference.

---

### P6 — FTS5 query syntax not explained

The skill says "FTS5 full-text search" but never explains what query formats work.
Agents will type natural language and get no results when they should use FTS5 syntax:

| Pattern | Example |
|---------|---------|
| Keyword | `postgres` |
| Phrase | `"event sourcing"` |
| Boolean | `auth AND jwt` |
| Prefix | `deploy*` |
| Column filter | `title:postgres` |
| Implicit AND | `postgres performance` (space = AND) |

**Fix:** Add an FTS5 Quick Reference section (compact table).

---

### P7 — `--project` / `-p` flag not documented

All `linxmd memory` commands accept `--project <dir>` to run from any cwd.
Agents that operate from a subdirectory (e.g., `src/`) will get wrong paths without this.

**Fix:** Document `--project` in command reference.

---

### P8 — No "index missing" recovery path

What should the agent do when `.linxmd/memory.db` doesn't exist?
Currently: the CLI prints an error and exits. The skill says nothing about this case.

**Fix:** Add explicit recovery instruction: if index missing → run `linxmd memory index`
then retry the search.

---

### P9 — Search-vs-Read disambiguation missing

The skill doesn't explain when to:
- Use `linxmd memory search` (find relevant entries by keyword — always first)
- vs read the actual `.md` source file (when you need full context of a specific ADR)

Search returns titles + snippets. You still need to read the file if the snippet shows
the decision is relevant to your current task.

**Fix:** Add a "Read vs Search" note in the protocol.

---

### P10 — Indexer limitations not documented

The indexer is hardcoded to three source paths:
- `docs/decisions/*.md` (excluding `README.md`)
- `CHANGELOG.md` (at project root)
- `KNOWN_ISSUES.md` (at project root)

Agents may try to add custom memory files (e.g., `docs/notes/*.md`) and wonder why they
don't appear in search results.

**Fix:** Add a "What Gets Indexed" table and note that custom paths require a CLI upgrade.

---

### P11 — `recent` command use case not explained

Agents don't know when to prefer `recent` over `search`. The use case is:
- `search` → when you know what you're looking for (keyword-driven)
- `recent` → at session start, to see what changed since last time; or to check
  if a decision was made recently without knowing its keywords

**Fix:** Add a one-liner use-case note next to each command.

---

### P12 — Version stuck at 0.3.0

The skill was substantially extended (CLI section, stability contract, search protocol)
but `version:` in the frontmatter was never updated.

**Fix:** Bump to 0.5.0 now; bump to 1.0.0 when this plan is complete.

---

## Scope: What is OUT of v1

- Custom indexer paths (requires CLI change) — defer to v1.1
- Automatic index refresh on file save (IDE hook) — not in scope
- Multi-project memory (cross-repo) — not in scope
- `linxmd memory export` / `import` — not in scope
- ADR template variations (lightweight, full) — minor, not blocking

---

## v1 Implementation Plan

### Step 1 — Restructure sections

New order:
1. Frontmatter (version: 1.0.0)
2. Trigger phrases
3. **Agent Protocol** ← new, moves to top
4. **CLI Command Reference** ← promoted, expanded
5. **FTS5 Quick Reference** ← new
6. Memory File Structure (format reference — demoted)
7. ADR Format
8. CHANGELOG format
9. KNOWN_ISSUES format
10. Markdown fallback protocol (renamed from "Without the index")
11. Rules
12. When NOT to Use
13. Stability Contract

### Step 2 — Write "Agent Protocol" section

```markdown
## Agent Protocol

Run this protocol at the start of every session where project history is relevant:

1. **Ensure index is fresh**
   ```bash
   linxmd memory index   # idempotent — always safe to run
   ```

2. **Orient: what was recently recorded?**
   ```bash
   linxmd memory recent --limit 5
   ```

3. **Before any architectural decision — search first**
   ```bash
   linxmd memory search "<topic keywords>"
   ```
   If results appear: read the matched source file(s) before deciding.
   If no results: no prior decision exists — proceed, then record a new ADR.

4. **After recording a decision** (new ADR written):
   ```bash
   linxmd memory index   # rebuild so the new ADR is searchable
   ```

5. **If `.linxmd/memory.db` is missing:**
   ```bash
   linxmd memory index   # creates the db and indexes everything
   ```
```

### Step 3 — Expand CLI Command Reference

```markdown
## CLI Command Reference

| Command | Default | Description |
|---------|---------|-------------|
| `linxmd memory index` | — | Clear and rebuild the full index from all source files |
| `linxmd memory search <query>` | `--limit 5` | FTS5 search; returns type, title, snippet, file path |
| `linxmd memory stats` | — | Entry count per type (decision / changelog / issue) |
| `linxmd memory recent` | `--limit 10` | Most recently indexed entries, newest first |

All commands accept `--project <dir>` (or `-p <dir>`) to specify the project root
when running from a subdirectory.

`--limit` / `-n` overrides the default result count for `search` and `recent`.
```

### Step 4 — Add FTS5 Quick Reference table

### Step 5 — Fix stats example output

### Step 6 — Add "What Gets Indexed" table

### Step 7 — Bump version to 1.0.0, update description

---

## Acceptance Criteria

A skill is v1-ready when:

- [ ] An agent with ZERO prior knowledge of the project can load this skill and immediately
      run the correct `linxmd memory` commands without guessing
- [ ] The stats output matches what the CLI actually prints
- [ ] All CLI flags that affect agent behavior are documented
- [ ] The protocol defines what to do when the index doesn't exist
- [ ] Commands have one-line use-case notes (not just descriptions)
- [ ] FTS5 tip table present
- [ ] `version: 1.0.0` in frontmatter
- [ ] Pure-markdown fallback is clearly labeled as "fallback, use only if linxmd not installed"
