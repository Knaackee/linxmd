---
name: graph-memory
type: skill
level: growth
version: 1.0.0
description: >
  Temporal Knowledge Graph memory protocols for agents. When and how to use the
  graph layer: session-start context injection, NL recall, Cypher queries,
  writing graph-native entities and relationships, temporal fact invalidation.
  Requires OpenCode.Continue with memory endpoints running.
tags: [memory, knowledge-graph, context, temporal, graph, kuzu, continue]
lifecycle:
  postInstall:
    prompt: >
      Verify that OpenCode.Continue is running with memory endpoints enabled.
      If not yet deployed, this skill gracefully degrades to project-memory.
---

# Graph Memory Skill

> The Knowledge Graph is a co-equal truth store alongside Markdown. It holds entities, relationships, and temporal facts that have no Markdown equivalent — and connects everything across projects and sessions.

## When to Apply

Use this skill **in addition to** `project-memory` when:

- The project runs OpenCode.Continue with memory endpoints enabled
- You need cross-session or cross-project knowledge retrieval
- You're working with relational facts (who knows whom, what depends on what)
- Temporal queries matter: "what was decided in 2025?", "is this still valid?"
- Token pressure is high and context injection must stay lean (~200 Token)

**Do not replace** `project-memory`. Both work together:
- `project-memory` = Markdown fulltext (ADR body, spec, trace)
- `graph-memory` = entities, relationships, temporal recall

## Prerequisites

OpenCode.Continue must be reachable at the configured base URL (default: `http://localhost:5000`). Check with:

```
GET /health
```

If the endpoint returns an error, fall back silently to `project-memory`. Never fail a session because the graph is unavailable.

---

## Session-Start Protocol

**Every agent** calls this at startup **before** reading any Markdown files:

```
GET /memory/context?projectId=<current>&depth=2
```

This returns a compact Markdown block (~200 Token) containing:
- Active tasks and their blocker-chains
- Recent decisions (top 5 by date)
- Relevant learnings for the current task
- User preferences relevant to this session

**Replace** the manual trace-reading step from `project-memory` with this call when available. The context injection is pre-assembled from graph traversal — faster and leaner than reading 10 individual files.

### Example Response

```markdown
## Active Context — 2026-03-25

Project: MyApp (React + .NET 8, strict TDD)
Person: Ben (senior dev, thorough reviews, prefers explicit over clever)

Active Task: TASK-087 — Refactor Auth Module
  Status: in-progress (~3h remaining)
  Blocking: TASK-086 (Integration Tests) — noch offen
  Related Decisions: ADR-003 (JWT), ADR-011 (Refresh Token Flow)

Last Session: 2026-03-24 — implementer
  - ADR-011 erstellt (supersedes ADR-003 in Token-Part)
  - 3 Files modified: auth.service.ts, token.helper.ts, auth.spec.ts
  - Open: Rate limiting noch nicht implementiert

Relevant Learnings:
  - "BCrypt cost 10 causes 200ms test slowdown" → use cost 4 in tests
  - "JWT RS256 > HS256 for multi-service" (2026-01-12)
```

---

## Reading: Recall

For natural-language questions about project history, decisions, or learnings:

```
POST /memory/recall
{ "query": "Was haben wir über Performance entschieden?", "projectId": "myapp" }
```

The backend converts NL to Cypher via a mini-LLM call and streams the result.

**Use recall for:**
- "What was decided about X?" → finds relevant Decisions
- "Have we solved this problem before?" → finds Learnings
- "What depends on ADR-NNN?" → finds impact chains
- "What does the user prefer?" → finds personal Facts and Preferences

**Do not use** for current task state — use `GET /memory/context` instead.

---

## Reading: Raw Cypher Query

For precise graph exploration when you know exactly what structure you're looking for:

```
POST /memory/query
{ "cypher": "MATCH (d:Decision) WHERE d.valid_until IS NULL RETURN d.id, d.title ORDER BY d.date DESC LIMIT 10" }
```

⚠️ Only read-only queries are accepted. `CREATE`, `DELETE`, `SET`, `MERGE`, `DETACH`, `DROP`, `CALL` are blocked.

### Useful Exploration Patterns

```cypher
-- All currently valid decisions
MATCH (d:Decision) WHERE d.valid_until IS NULL
RETURN d.id, d.title, d.date ORDER BY d.date DESC

-- What does ADR-003 affect?
MATCH (d:Decision {id:"ADR-003"})<-[:IMPLEMENTS|BLOCKED_BY*1..3]-(affected)
RETURN labels(affected)[0] AS type, affected.id, affected.title

-- What was decided in 2025 about auth (now superseded)?
MATCH (d:Decision)
WHERE d.valid_from >= date("2025-01-01") AND d.valid_until IS NOT NULL
  AND d.tags CONTAINS "auth"
RETURN d.id, d.title, d.valid_from, d.valid_until

-- User's technology preferences
MATCH (:Person {name:"Ben"})-[r:PREFERS]->(t:Technology)
WHERE r.strength > 0.7
RETURN t.name, t.category, r.strength ORDER BY r.strength DESC

-- All projects using the same technology
MATCH (p:Project)-[:USES]->(t:Technology {name:"React"})
RETURN p.name, p.status
```

---

## Writing: Graph-Native Entities

When you discover facts that have **no Markdown equivalent** — persons, relationships, preferences, cross-project patterns — write them directly to the graph.

```
POST /memory/graph/node
{
  "label": "Fact",
  "properties": {
    "id": "fact-<uuid>",
    "content": "Ben bevorzugt explizite Typen über implizite Inferenz",
    "confidence": 0.9,
    "project_id": null
  }
}
```

```
POST /memory/graph/edge
{
  "from": "ben-person-id",
  "fromLabel": "Person",
  "to": "typescript-tech-id",
  "toLabel": "Technology",
  "type": "PREFERS",
  "properties": { "strength": 0.85, "since": "2026-01-01" }
}
```

### What belongs in the graph (not in Markdown)

| Write to Graph | Write to Markdown |
|---|---|
| Person "Ben" works on Project "Alpha" | ADR body text |
| Technology X is alternative to Y | Spec or trace |
| User prefers detailed code reviews | Task description |
| Cross-project pattern: DTO validation | Learning (also graph-synced via frontmatter) |
| Company "Acme" is FinTech client | — |

**Rule of thumb:** If the fact has no meaningful fulltext body and is purely a structured relationship or attribute, it belongs in the graph.

---

## Writing: Temporal Invalidation

When a fact becomes outdated (superseded decision, changed preference, closed project):

```
POST /memory/graph/invalidate
{ "nodeId": "ADR-003" }
```

This sets `valid_until = NOW` on the node. The node is **not deleted** — it remains queryable for historical context.

**Never** manually delete graph nodes. Always invalidate.

---

## Sync: Derived Nodes from Markdown

After writing or updating Markdown files with frontmatter (Tasks, Decisions, Learnings), trigger a sync to keep the graph in step:

```
POST /memory/graph/sync?projectId=<current>
```

The sync only touches **Derived Nodes** (Task, Decision, Session, Learning) — it never modifies Graph-Native nodes.

Call this at session-end, or anytime after bulk Markdown changes.

---

## Graceful Degradation

If the graph backend is unavailable:

1. Log: "Graph memory unavailable — falling back to project-memory"
2. Read `PROJECT.md`, recent traces, and relevant ADRs manually (see `project-memory` skill)
3. Skip all `/memory/*` calls silently
4. Continue the session normally — never block on graph availability

---

## Anti-Patterns

- **Replacing Markdown with graph nodes** — ADR body text stays in Markdown; the graph holds the ADR's relationships and metadata only
- **Deleting nodes** — always invalidate, never delete. Historical facts are valuable.
- **Writing LLM-generated speculation as Fact** — only write facts with `confidence > 0.7` that are based on observed behavior or explicit user input
- **Skipping context injection** — loading 15 ADRs manually instead of calling `GET /memory/context` wastes token budget
- **Raw Cypher for simple recall** — use `/memory/recall` (NL) for fuzzy questions; use `/memory/query` only when precision matters
- **Blocking on graph unavailability** — the graph is an enhancement, not a dependency
