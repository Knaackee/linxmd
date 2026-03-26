---
name: graph
type: skill
version: 3.0.0
description: CLI-only interaction protocol for Continue graph-memory. Defines how agents read, query, and write graph data via opencode-continue graph-memory commands.
level: core
tags: [graph, memory, cli, continue, cypher]
quickActions:
  - label: Persist knowledge from referenced file to graph
    icon: "💾"
    prompt: Inspect only the referenced document. If it contains durable new knowledge, persist it with opencode-continue graph-memory CLI commands (write-node/write-edge) including confidence and source. If not, report no persistence needed.
    trigger:
      fileMatch: ["**/*.md", "**/*.mdx", "**/*.txt", "**/*.json", "**/*.yml", "**/*.yaml"]
  - label: Rufe Wissen ab
    icon: "🔎"
    prompt: Nutze opencode-continue graph-memory, um relevantes Wissen zur aktuellen Chat-Anfrage abzurufen. Gib eine kurze, priorisierte Zusammenfassung mit Quellenhinweis (Node IDs oder Query-Kontext).
    trigger:
      chat: true
---

# Graph CLI Protocol

This skill defines only command interaction with graph-memory via Continue CLI.

## When To Apply

Apply this skill whenever an agent needs to read or write graph-memory.

Use it for:
- compact context load
- Cypher exploration
- node and edge writes
- temporal invalidation

Do not define memory policy here. Memory policy belongs to graph-memory skill.

## Rules

1. Use only `opencode-continue graph-memory <command>`.
2. Do not call raw HTTP graph endpoints from agents.
3. Prefer `context` for startup, `query` for precision, `write-node`/`write-edge` for persistence.
4. Never delete nodes; use `invalidate`.
5. Include provenance fields where possible (for example source file path, scope id, agent id).
6. Graph command failures must not block core workflow execution.

## Command Patterns

### Startup context

```bash
opencode-continue graph-memory context <scopeId> --depth 2
```

### List entities

```bash
opencode-continue graph-memory nodes --project-id <scopeId>
opencode-continue graph-memory nodes --type Decision --project-id <scopeId>
```

### List relations

```bash
opencode-continue graph-memory edges --from ITEM-042
opencode-continue graph-memory edges --to DEC-011
```

### Precise query

```bash
opencode-continue graph-memory query "MATCH (d:Decision) WHERE d.valid_until IS NULL AND d.scope_id = '<scopeId>' RETURN d.id, d.title ORDER BY d.date DESC LIMIT 10"
```

### Write node

```bash
opencode-continue graph-memory write-node --label Fact --id fact-<uuid> --project-id <scopeId> --properties '{"content":"Observed latency spike in checkout.","confidence":0.9,"source":"<filePath>"}'
```

### Upsert project metadata (rename-safe)

```bash
opencode-continue graph-memory write-node --label Project --id <projectId> --properties '{"project_path_current":"<projectPath>","project_path_aliases":["<oldPath>"],"updated_at":"<isoTime>"}'
```

### Upsert document metadata

```bash
opencode-continue graph-memory write-node --label Document --id <docId> --project-id <projectId> --properties '{"doc_rel_path":"<relPath>","content_hash":"<sha256>","summary_short":"<short-summary>","source":"<filePath>"}'
```

### Link document to project

```bash
opencode-continue graph-memory write-edge --from <docId> --from-label Document --to <projectId> --to-label Project --type BELONGS_TO
```

### Write edge

```bash
opencode-continue graph-memory write-edge --from ITEM-042 --from-label WorkItem --to DEC-011 --to-label Decision --type IMPLEMENTS
```

### Temporal invalidation

```bash
opencode-continue graph-memory invalidate ADR-003
```

## Error Handling Pattern

If a graph CLI command fails:
- Log the failed command intent in trace notes.
- Continue the current work without blocking.
- Retry once later in the same session.

## Anti-Patterns

- Mixing CLI and raw HTTP for the same workflow.
- Running broad query loops when one focused query is enough.
- Writing nodes without source or confidence context.
- Deleting or overwriting historical nodes instead of invalidation.

## Exit Criteria

This skill is applied correctly when:
- All graph interactions use Continue graph-memory CLI commands.
- Reads and writes are traceable to scope/agent/source.
- Invalidation is used for outdated facts.
- Graph command failures do not stop delivery.
