---
name: api-design
type: skill
version: 0.3.0
description: Design REST APIs that last — OpenAPI 3.1 spec writing, versioning strategies, standard error schemas, pagination patterns, and a security checklist per endpoint
deps: []
tags:
  - api
  - rest
  - openapi
  - design
---

# API Design Skill

Triggered by: "design this API", "write the OpenAPI spec", "review this endpoint", "how should we version this?", or any request involving HTTP API structure and contracts.

## Principles

1. **Contract first** — write the spec before writing the handler
2. **Stable by default** — versioning is harder to retrofit than to start with
3. **Fail loudly** — error responses must be as informative as success responses
4. **Least surprise** — follow RFC and community conventions before inventing patterns

## OpenAPI 3.1.0 Spec Writing

Always produce a valid OpenAPI 3.1.0 spec for any new API surface.

### Minimum Requirements per Endpoint
- `operationId` (unique, camelCase: `getUserById`, `createOrder`)
- `summary` (one sentence)
- `tags` (feature area)
- At least one `2xx` success response
- At least one `4xx` error response referencing the standard error schema

### Standard Error Schema

```yaml
ErrorResponse:
  type: object
  required: [code, message]
  properties:
    code:
      type: string
      description: Machine-readable error code (e.g., "AUTH_TOKEN_EXPIRED")
    message:
      type: string
      description: Human-readable description
    details:
      type: object
      description: Optional structured details (field errors, rate limit info)
    traceId:
      type: string
      description: Correlation ID for log lookup
```

Apply `ErrorResponse` to all `4xx` and `5xx` responses.

## Versioning Strategy

Choose one and apply it consistently across the entire API:

| Strategy | Format | Best For |
|---|---|---|
| URL path | `/v1/users`, `/v2/users` | Public APIs with known breaking change cadence |
| Accept header | `Accept: application/vnd.api.v2+json` | Internal APIs with known clients |
| Query param | `?api-version=2025-01-01` | Azure-style date versioning |

### Versioning Rules
- Never modify the behavior of a versioned endpoint — add `/v2/` instead
- Mark deprecated endpoints with `Deprecation` and `Sunset` response headers
- Maintain at least one previous version for 6 months after a breaking change

## REST Conventions

### Resource Naming
- Collections: plural nouns — `/users`, `/orders`, `/products`
- Resources: `/users/{userId}`, `/orders/{orderId}`
- Actions: verb as sub-resource — `/orders/{orderId}/cancel`, `/users/{userId}/verify-email`
- **Never** use verbs in collection or resource names: `/getUser` is wrong

### HTTP Methods

| Method | Meaning | Idempotent | Request Body |
|--------|---------|------------|------|
| GET    | Read    | ✅ | No |
| POST   | Create / trigger action | ❌ | Yes |
| PUT    | Replace (full update) | ✅ | Yes |
| PATCH  | Partial update | ❌ | Yes |
| DELETE | Delete  | ✅ | No |

### Status Codes

| Code | When to use |
|---|---|
| `200 OK` | Successful read or update |
| `201 Created` | Resource created (include `Location` header) |
| `202 Accepted` | Async operation started |
| `204 No Content` | Successful delete |
| `400 Bad Request` | Client error — validation failed or bad body |
| `401 Unauthorized` | Missing or invalid authentication |
| `403 Forbidden` | Authenticated but not authorized |
| `404 Not Found` | Resource does not exist |
| `409 Conflict` | State conflict (duplicate, version mismatch) |
| `422 Unprocessable Entity` | Semantic validation error |
| `429 Too Many Requests` | Rate limited (include `Retry-After` header) |
| `500 Internal Server Error` | Unexpected server failure |

### Pagination

Use **cursor-based pagination** for large, frequently-updated datasets:
```json
{
  "data": [...],
  "pagination": {
    "cursor": "eyJpZCI6MTAwfQ==",
    "hasMore": true,
    "count": 20
  }
}
```

Use **offset pagination** only for small, stable datasets with a known total count.

## Security Checklist (Per Endpoint)

- [ ] Authentication required? (or explicitly declared as public)
- [ ] Authorization: what role/scope is required?
- [ ] Input validation: required fields, type constraints, max lengths
- [ ] Output: does the response leak sensitive fields? (passwords, tokens, PII)
- [ ] Rate limiting applied?
- [ ] Idempotency key required for sensitive mutations?

## Rules

- Spec first, code second — no handler without a spec entry
- Never `200 OK` an empty result for a single resource — `404` for missing, `200` + empty array for empty collection
- Never return stack traces in error responses
- All PII fields must be documented in the spec with `x-pii: true`
- Request bodies must reference named schemas — no inline anonymous objects

## When NOT to Use

- For GraphQL or gRPC APIs — they have their own design conventions
- For internal function calls or message queue contracts — not HTTP
