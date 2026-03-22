---
name: i18n
type: skill
version: 0.3.0
description: Make any codebase internationalisation-ready — audits hardcoded strings, extracts them to locale files, replaces with i18n calls, and verifies completeness across all locales
deps: []
tags:
  - i18n
  - localization
  - accessibility
---

# Internationalisation Skill (i18n)

Triggered by: "make this i18n-ready", "extract strings", "add locale support", "support multiple languages", or any request to prepare a codebase for multilingual users.

## Principles

i18n is not translation — it is the structural work that makes translation possible.
After i18n work is complete, adding a new locale should require only a new locale file.

## Process

### Phase 1: Audit

1. Scan all source files for hardcoded human-readable strings:
   - UI labels, button text, error messages, tooltips, notifications
   - Email templates, log messages visible to users
   - Date, number, and currency format assumptions
2. Produce `docs/i18n-audit.md`:
   - Total string count
   - Files affected
   - String categories (UI, errors, notifications, etc.)

### Phase 2: Choose Locale Format

Pick the format appropriate for the tech stack:

| Stack | Format |
|---|---|
| JavaScript/TypeScript | `locales/en.json` (flat or nested key-value) |
| .NET | `.resx` resource files |
| Python | `.po` files (GNU gettext) |
| iOS/macOS | `.strings` or `.xcstrings` |
| Android | `res/values/strings.xml` |

### Phase 3: Extraction

1. Extract all found strings to `locales/en.[ext]` (base locale)
2. Use semantic keys, not positional: `error.network.timeout` not `msg_42`
3. Group by feature/domain: `auth.login.button`, `auth.error.invalid_password`
4. Context must be recoverable from the key alone — future translators won't see the source

### Phase 4: Code Replacement

Replace every hardcoded string with a locale lookup call using the framework's i18n API:
- `t('auth.login.button')` in i18next / react-i18next
- `Resources.AuthLoginButton` in .NET
- `_('auth.login.button')` in Python gettext
- `NSLocalizedString("auth.login.button", ...)` in Swift

After replacement: zero hardcoded user-visible strings in source files except the locale files themselves.

### Phase 5: Format & Placeholder Audit

1. Hardcoded date/time format strings → replace with locale-aware formatters (`Intl.DateTimeFormat`, `CultureInfo`, etc.)
2. Number and currency formatting → replace with `Intl.NumberFormat`, `CultureInfo.CurrentCulture`, etc.
3. String concatenation that embeds variables → replace with templates that support argument reordering:
   - ❌ `"Hello, " + name + "!"` — breaks in right-to-left languages
   - ✅ `t('greeting', { name })` with `"Hello, {{name}}!"` in locale file

### Phase 6: Completeness Verification

1. List all locale files (other than base `en`)
2. Verify every key in the base locale exists in every other locale file
3. Missing keys → report in `docs/i18n-audit.md` gap table

## Audit Report Template

```markdown
# i18n Audit Report

## Summary
- Strings extracted: [N]
- Files modified: [N]
- Base locale: en
- Existing locales: [list]

## Completeness
| Locale | Coverage | Missing Keys |
|--------|----------|-------------|
| de     | 94%      | 3 keys      |

## Open Items
- [format assumptions that need locale-aware replacement]
- [complex plural forms or gender-inflected strings requiring review]
```

## Rules

- Never translate strings during this process — extraction only; hand off to `skill:text-translator`
- Use semantic keys, never positional or hash-based keys
- After extraction, zero hardcoded user-visible strings remain in source (except locale files)
- All format strings must support argument reordering
- Always produce the audit report

## When NOT to Use

- For translating content between languages → use `skill:text-translator`
- For code that has no user-visible strings (pure backend/infrastructure)
