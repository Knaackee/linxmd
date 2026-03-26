---
name: graph-memory
type: skill
version: 3.0.0
description: Memory policy for graph-backed knowledge in Linxmd. Defines what should be stored, validated, and invalidated in graph-memory; command interaction details are defined in skill graph.
level: core
tags: [memory, graph, continue, context, temporal, recall]
quickActions:
  - label: Capture new knowledge from referenced file
    icon: "🧠"
    prompt: Inspect only the referenced document and determine whether it contains new durable knowledge. If yes, write only validated facts/decisions/learnings to graph-memory with confidence and provenance. If no, report that no new knowledge should be stored.
    trigger:
      fileMatch: ["**/*.md", "**/*.mdx", "**/*.txt", "**/*.json", "**/*.yml", "**/*.yaml"]
---

# Graph Memory

Use this skill when agents need compact, relational, cross-session memory with temporal history.

This skill defines memory semantics. Command mechanics are defined in skill `graph`.

## When To Apply

Apply this skill by default for all agent sessions that need durable, structured memory.

Use it when:
- You need fast startup context with low token cost.
- You need dependency chains, decision impact, or relationship queries.
- You need to preserve historical state without deleting outdated facts.
- You need to write structured facts with no rich markdown body.

Do not use graph-memory as a replacement for markdown memory. Use both:
- `project-memory`: full text and human-readable documents.
- `graph-memory`: entities, relations, temporal validity, and compact recall.

## Rules

1. Never delete graph nodes. Invalidate temporally instead.
2. Write only observed or user-confirmed facts with confidence >= 0.7.
3. Keep session context compact and intent-focused.
4. Always include scope/source provenance for persisted knowledge.
5. Use graph-memory for structured durable knowledge, not rich narrative text.
6. If graph persistence fails, continue delivery and record intended memory action.
7. Do not store full markdown document bodies in graph nodes.
8. Use stable ids (`project_id`, `doc_id`) as identity; paths are mutable metadata.

## Document-Centric Memory Model

Use graph-memory as an index and relationship layer for project documents.

Identity/path metadata is graph-memory canonical metadata and should round-trip each run:

1. Read from graph-memory at init.
2. Mirror into markdown headers for human/audit visibility.
3. Write back via delta at post phase.

Recommended node model:

- `Project`:
  - `project_id` (stable)
  - `project_path_current` (mutable)
  - `project_path_aliases` (optional historical paths)
- `Document`:
  - `doc_id` (stable)
  - `project_id`
  - `doc_rel_path`
  - `content_hash`
  - `summary_short` (5-10 lines, optional)
- `Claim` / `Decision` / `Learning`:
  - linked to `Document` via derivation edges

Recommended edges:

- `(:Document)-[:BELONGS_TO]->(:Project)`
- `(:Claim)-[:DERIVED_FROM]->(:Document)`
- `(:Decision)-[:RELATES_TO]->(:Claim|:Document)`

### Path Rename Handling

When a project folder is renamed:

1. Keep `project_id` unchanged.
2. Update `project_path_current`.
3. Append old path to `project_path_aliases`.
4. Do not recreate document identity if `doc_id` and `content_hash` still match.

## Memory Lifecycle

### Step 1: Gather compact context

Every agent starts with compact graph context via skill `graph`.

Expected context includes:
- active work items and dependency chains
- recent decisions
- relevant learnings
- relevant preferences and constraints

### Step 2: Retrieve prior knowledge before re-deciding

Before creating new decisions, check whether valid decisions/learnings already exist.

### Step 3: Persist only durable knowledge

Persist facts, decisions, learnings, and relations that remain useful beyond the current message.
Persist document metadata (`doc_id`, `doc_rel_path`, `content_hash`) and concise summaries, not full document bodies.

### Step 4: Invalidate outdated knowledge

When knowledge is superseded, invalidate it while preserving history.

### Step 5: Session-end capture

After markdown updates, persist the session's durable deltas and include provenance.

## Fallback Behavior

If graph-memory persistence fails:
- Continue the workflow and do not block the task.
- Record the intended write/read action in trace notes.
- Retry once later in the same session if needed.

## Patterns

### Pattern: Startup context first

- Load compact graph context before reading multiple markdown files.
- Keep initial context small and intent-focused.

### Pattern: Recall before re-deciding

- Query prior decisions/learnings before creating new ADRs or changing architecture.
- Reuse prior decisions when still valid.

### Pattern: Work-item dependency expansion

Use Cypher for dependency chains:

```cypher
MATCH (n {id:$entityId})-[:BLOCKED_BY|DEPENDS_ON|RELATES_TO*1..3]->(dep)
RETURN labels(dep)[0] AS type, dep.id, dep.title
```

### Pattern: Historical decision checks

```cypher
MATCH (d:Decision)
WHERE d.valid_until IS NOT NULL AND d.tags CONTAINS "auth"
RETURN d.id, d.title, d.valid_from, d.valid_until
ORDER BY d.valid_until DESC
```

### Pattern: User preference lookup

```cypher
MATCH (:Person {id:$personId})-[r:PREFERS]->(x)
RETURN labels(x)[0] AS targetType, x.name, r.strength
ORDER BY r.strength DESC
```

### Pattern: Scope-based recall

```cypher
MATCH (n)
WHERE n.scope_id = $scopeId AND n.valid_until IS NULL
RETURN labels(n)[0] AS type, n.id
LIMIT 50
```

## Anti-Patterns

- Replacing markdown ADR/spec content with graph-only nodes.
- Persisting transient chat noise as durable graph facts.
- Persisting facts without provenance or confidence.
- Writing speculative facts without evidence.
- Deleting nodes instead of temporal invalidation.
- Blocking workflow execution when graph persistence fails.

## Exit Criteria

This skill is applied correctly when:
- Agents start with graph context or fallback safely.
- Relevant recall/query calls are used during planning and implementation.
- Structured facts and relations are written with confidence discipline.
- Outdated facts are invalidated, not deleted.
- Session-end memory deltas are captured with provenance.
- Team can reconstruct why decisions were made across sessions.
