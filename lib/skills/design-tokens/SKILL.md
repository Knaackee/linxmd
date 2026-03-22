---
name: design-tokens
type: skill
version: 0.3.0
description: Extract magic values from CSS/SCSS into a token system — one source of truth for colors, spacing, and typography, with Tailwind and CSS custom properties output
deps: []
tags:
  - design
  - css
  - tokens
  - frontend
---

# Design Tokens Skill

Triggered by: "extract design tokens", "standardize our colors/spacing", "create a design system", "audit CSS magic values", or any request to establish visual consistency through named constants.

## What Are Design Tokens?

Design tokens are named constants for visual primitives: colors, spacing, typography, shadows, border radii. They are the single source of truth — referenced by name everywhere rather than hard-coded as raw values.

`color.brand.primary` → `#5A4FCF` instead of `#5A4FCF` scattered across 47 files.

## Process

### Phase 1: Audit

1. Scan all CSS, SCSS, Tailwind config, inline style props, and styled-components
2. Collect all raw values: hex colors, RGB values, pixel/rem spacing, font sizes, font families, border radii, box shadows
3. Cluster similar values — group colors that are clearly the same brand color expressed differently (`#5a4fcf`, `rgb(90,79,207)`)
4. Produce an audit table:

```markdown
## Color Clusters (N unique values, M clusters)
| Value    | Count | Files               | Likely Token         |
|----------|-------|---------------------|----------------------|
| #5A4FCF  | 23    | app.css, button.css | color.brand.primary  |
| #F0EEFF  | 8     | card.css, hero.css  | color.brand.light    |
```

### Phase 2: Create Token File

Create `tokens.json` (W3C Design Token Format):

```json
{
  "color": {
    "brand": {
      "primary": { "$value": "#5A4FCF", "$type": "color" },
      "light":   { "$value": "#F0EEFF", "$type": "color" }
    },
    "neutral": {
      "900": { "$value": "#111111", "$type": "color" },
      "100": { "$value": "#F9F9F9", "$type": "color" }
    },
    "feedback": {
      "error":   { "$value": "#D32F2F", "$type": "color" },
      "success": { "$value": "#388E3C", "$type": "color" }
    }
  },
  "spacing": {
    "xs": { "$value": "4px",  "$type": "dimension" },
    "sm": { "$value": "8px",  "$type": "dimension" },
    "md": { "$value": "16px", "$type": "dimension" },
    "lg": { "$value": "24px", "$type": "dimension" },
    "xl": { "$value": "40px", "$type": "dimension" }
  }
}
```

### Phase 3: Generate Output

**CSS Custom Properties** (`tokens.css`):
```css
:root {
  --color-brand-primary: #5A4FCF;
  --color-neutral-900:   #111111;
  --spacing-xs:          4px;
}
```

**Tailwind Config** (`tailwind-tokens.js`):
```js
module.exports = {
  theme: {
    colors: {
      brand: { primary: '#5A4FCF', light: '#F0EEFF' },
      neutral: { 900: '#111111', 100: '#F9F9F9' }
    },
    spacing: { xs: '4px', sm: '8px', md: '16px' }
  }
}
```

### Phase 4: Replace Magic Values

For each raw value in the audit:
1. Replace with the token reference (`var(--color-brand-primary)` or `theme('colors.brand.primary')`)
2. After replacement, zero raw color/spacing values remain in source files except the token file itself

## Token Naming Rules

- Names describe **purpose**, not appearance: `color.feedback.error` not `color.red`
- Neutral scale uses numbers: `color.neutral.900` not `color.dark-grey`
- Spacing uses t-shirt scale or geometric progression — not arbitrary pixels
- Never use `primary` for a color that only appears once — that's a magic value with a fancy name

## Rules

- Never guess at a cluster's intent — ask if ambiguous (disabled state vs hover state?)
- Never rename tokens once they are in production — add new tokens instead
- After completion, the token file is the single source of truth
- No raw visual primitives outside the token file

## When NOT to Use

- For code that has no visual layer (pure backend/CLI)
- For a one-off style change — use the token directly; don't create a token for a single use
