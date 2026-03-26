---
name: design-tokens
type: skill
level: core
version: 2.0.0
description: >
  Design token management: colors, spacing, typography, shadows as structured
  tokens. Single source of truth for design consistency.
tags: [core, design, tokens, css, ui]
---

# Design Tokens Skill

> Design tokens are the single source of truth for visual design: colors, spacing, typography, elevation. Change once, apply everywhere.

## Token Categories

| Category | Examples | Format |
|----------|---------|--------|
| **Color** | primary, secondary, error, surface | HEX, HSL, or RGB |
| **Spacing** | xs, sm, md, lg, xl, 2xl | rem or px |
| **Typography** | font-family, font-size, line-height, font-weight | Various |
| **Elevation** | shadow-sm, shadow-md, shadow-lg | CSS box-shadow |
| **Border** | radius-sm, radius-md, radius-lg, width | rem or px |
| **Breakpoint** | mobile, tablet, desktop | px |
| **Motion** | duration-fast, duration-normal, easing | ms, cubic-bezier |

## Token Structure

```json
{
  "color": {
    "primary": { "value": "#3B82F6", "description": "Primary brand color" },
    "primary-hover": { "value": "#2563EB" },
    "error": { "value": "#EF4444" },
    "surface": { "value": "#FFFFFF" },
    "text": { "value": "#1F2937" }
  },
  "spacing": {
    "xs": { "value": "0.25rem" },
    "sm": { "value": "0.5rem" },
    "md": { "value": "1rem" },
    "lg": { "value": "1.5rem" },
    "xl": { "value": "2rem" }
  }
}
```

## Rules

- **One source of truth** — tokens defined in one place, consumed everywhere
- **Semantic names** — `color-primary`, not `blue-500`
- **Theme-ready** — support light/dark themes via token overrides
- **Document every token** — include description and usage guidance
- **No magic numbers** — every design value references a token
