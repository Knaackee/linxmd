---
name: observability
type: skill
level: governance
version: 2.0.0
description: >
  Code-level observability standards: structured logging, distributed tracing,
  error tracking, and health signals. Defines what "observable" code looks like.
tags: [governance, observability, logging, tracing, monitoring]
---

# Observability Skill

> Observable code tells you what's happening without reading source. Logging, tracing, error tracking, and health signals — all mandatory.

## The Four Pillars

### 1. Structured Logging

Every significant operation emits a log entry with context:

```json
{
  "timestamp": "2026-03-23T14:30:00Z",
  "level": "info",
  "message": "User authenticated",
  "context": {
    "userId": "usr_123",
    "method": "JWT",
    "ip": "192.168.1.1",
    "duration_ms": 12
  }
}
```

**Log Levels**:
| Level | When |
|-------|------|
| `error` | Something failed that shouldn't have. Needs attention. |
| `warn` | Something concerning but handled. Should be monitored. |
| `info` | Significant business operations (user actions, state changes). |
| `debug` | Technical details useful during development/debugging. |

**Rules**:
- Always structured (JSON), never printf-style strings
- Always include context (who, what, when, result)
- Never log secrets, tokens, passwords, or PII
- Use consistent field names across the project

### 2. Distributed Tracing

Request flows must be traceable end-to-end:

```
[Client] → [API Gateway] → [Auth Service] → [User Service] → [Database]
   │           │                │                │                │
   └── trace_id: abc123 ───────────────────────────────────────────┘
```

**Rules**:
- Every incoming request gets a trace ID (generate if not present)
- Pass trace ID through all service calls
- Each significant operation is a **span** within the trace
- Span names follow: `<service>.<operation>` (e.g., `auth.validateJwt`)
- Record span duration, status, and key attributes

### 3. Error Tracking

Every error must be captured with context for reproduction:

```json
{
  "error": "NullReferenceException",
  "message": "User profile was null after auth",
  "stack": "...",
  "context": {
    "userId": "usr_123",
    "endpoint": "GET /profile",
    "trace_id": "abc123"
  }
}
```

**Rules**:
- No silent `catch` blocks — every caught exception must be logged
- Include enough context to reproduce the error
- Categorize: transient (retry) vs. permanent (fix needed)
- Link errors to traces via trace_id

### 4. Health Signals

Critical paths expose observable health:

| Signal Type | Example | Purpose |
|-------------|---------|---------|
| Health endpoint | `GET /health` → `{ "status": "healthy" }` | Load balancer / monitoring |
| Readiness probe | `GET /ready` → checks DB, cache, deps | Kubernetes / orchestration |
| Metrics | Request count, error rate, latency p50/p95/p99 | Dashboards, alerting |
| Heartbeat | Periodic "I'm alive" log entry | Long-running processes |

## Quality Checklist

For every feature, verify:
- [ ] Significant operations have log entries at appropriate levels
- [ ] Request flows have trace spans with durations
- [ ] Error paths capture context, not just the exception message
- [ ] No silent catch blocks
- [ ] No secrets in log output
- [ ] Health endpoints exist for all services
