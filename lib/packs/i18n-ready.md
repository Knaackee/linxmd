---
name: i18n-ready
type: pack
version: 2.0.0
description: >
  Internationalization bundle. String externalization, locale management,
  text translation, and i18n-aware code review.
workflow: feature-development
agents:
  - implementer
  - test-writer
  - reviewer-quality
skills:
  - i18n
  - text-translator
  - code-translator
  - trace-writing
  - conventional-commits
tags: [pack, i18n, localization, translation, internationalization]
---

# i18n-Ready Pack

> Everything you need to internationalize an application. String externalization, locale management, and translation workflows.

## What's Included

### Agents (3)
| Category | Agents |
|----------|--------|
| Core | `implementer`, `test-writer` |
| Control | `reviewer-quality` |

### Skills (5)
| Level | Skills |
|-------|--------|
| Core | `i18n`, `text-translator`, `code-translator` |
| Governance | `trace-writing`, `conventional-commits` |

## Workflow Integration

Uses the `feature-development` workflow with i18n-specific concerns:
- `test-writer` checks for hardcoded strings in tests
- `reviewer-quality` verifies string externalization, RTL support, plural handling
- `implementer` uses the `i18n` skill for all user-facing text

## i18n Review Checklist (added to reviewer-quality)

- [ ] No hardcoded user-facing strings in source code
- [ ] All strings use message format (ICU) for plurals/gender
- [ ] Date/number formatting uses locale-aware APIs
- [ ] Layout tested for 40% text expansion
- [ ] RTL direction supported where applicable
- [ ] Translation keys follow naming convention

## Install

```bash
linxmd install packs/i18n-ready
```
