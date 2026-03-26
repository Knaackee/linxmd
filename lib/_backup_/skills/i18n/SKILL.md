---
name: i18n
type: skill
level: core
version: 2.0.0
description: >
  Internationalization: string externalization, locale management, RTL support,
  pluralization, date/number formatting.
tags: [core, i18n, localization, translation, internationalization]
---

# Internationalization (i18n) Skill

> Build software that works in any language from day one. Externalize strings, handle plurals, format dates correctly.

## Core Principles

1. **Externalize all user-facing strings** — no hardcoded text in source code
2. **Use ICU message format** — handles plurals, gender, and complex patterns
3. **Format dates/numbers with locale** — never format manually
4. **Support RTL** — layout should flip for Arabic, Hebrew, etc.
5. **Test with pseudo-localization** — catch truncation and hardcoded strings early

## String Externalization

```
// ❌ Bad
return "Welcome back, " + user.name + "!";

// ✅ Good
return t('welcome.back', { name: user.name });
```

Translation file:
```json
{
  "welcome.back": "Welcome back, {name}!",
  "items.count": "{count, plural, =0 {No items} one {1 item} other {{count} items}}"
}
```

## Key Rules

- **Key naming**: `feature.section.element` (e.g., `auth.login.submit`)
- **No concatenation**: Use placeholders, never build strings from parts
- **Pluralization**: Use ICU plural rules, not if/else
- **Context**: Provide translator context for ambiguous strings
- **Max string length**: Design UI for 40% text expansion (German/French are longer)

## Date/Number Formatting

```
// ❌ Bad
return date.toLocaleDateString();

// ✅ Good
return new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(date);
```

Always use the platform's built-in `Intl` API (JS) or equivalent locale-aware formatter.
