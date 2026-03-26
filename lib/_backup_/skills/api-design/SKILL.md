---
name: api-design
type: skill
level: core
version: 2.0.0
description: >
  REST/HTTP API design principles: resource naming, HTTP methods, status codes,
  versioning, pagination, error responses, and OpenAPI documentation.
quickActions:
  - id: qa-api-contract-review
    icon: "🔌"
    label: API Contract Review
    prompt: Review API contracts for endpoint consistency, payload shape, status codes, and error response clarity.
    trigger:
      fileMatch:
        - '^\.linxmd/specs/.*\.md$'
      languageId: [markdown]
      contentMatch:
        - 'API|endpoint|payload|status code|OpenAPI'
tags: [core, api, rest, http, design]
---

# API Design Skill

> Design APIs that are predictable, consistent, and well-documented. Clients should be able to guess the next endpoint.

## Resource Naming

| Pattern | Example | Rule |
|---------|---------|------|
| Collection | `/users` | Plural nouns |
| Single item | `/users/{id}` | Identifier in path |
| Sub-resource | `/users/{id}/posts` | Nested under parent |
| Action (rare) | `/users/{id}/activate` | Verb only when CRUD doesn't fit |

## HTTP Methods

| Method | Purpose | Idempotent | Safe |
|--------|---------|------------|------|
| GET | Read resource(s) | Yes | Yes |
| POST | Create new resource | No | No |
| PUT | Replace entire resource | Yes | No |
| PATCH | Partial update | No* | No |
| DELETE | Remove resource | Yes | No |

## Status Codes

| Code | When |
|------|------|
| 200 | Success with body |
| 201 | Created (POST success) |
| 204 | Success, no body (DELETE) |
| 400 | Client error (bad input) |
| 401 | Not authenticated |
| 403 | Authenticated but not authorized |
| 404 | Resource not found |
| 409 | Conflict (duplicate, state mismatch) |
| 422 | Validation error (semantically invalid) |
| 500 | Server error (unexpected) |

## Error Response Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Human-readable description",
    "details": [
      { "field": "email", "message": "Must be a valid email address" }
    ]
  }
}
```

## Pagination

```
GET /users?page=2&pageSize=20

Response headers:
X-Total-Count: 150
X-Page: 2
X-Page-Size: 20

Response body:
{
  "data": [...],
  "pagination": {
    "page": 2,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

## Versioning

Prefer URL prefix versioning: `/v1/users`, `/v2/users`

## Documentation

Every API must have an OpenAPI (Swagger) spec or equivalent documentation including:
- All endpoints with descriptions
- Request/response schemas
- Authentication requirements
- Example requests and responses
- Error codes and their meaning
