---
name: text-translator
type: skill
level: core
version: 2.0.0
description: >
  Translate human-language text between languages. Preserve tone, meaning,
  and cultural context. For documentation, UI strings, and content.
tags: [core, translation, language, content, localization]
---

# Text Translator Skill

> Translate meaning, not words. Preserve tone, cultural context, and intent across languages.

## Translation Process

1. **Understand the source** — full context, not just the sentence
2. **Identify tone** — formal, casual, technical, marketing
3. **Translate for meaning** — idiomatic in the target language
4. **Preserve formatting** — markdown, HTML, placeholders unchanged
5. **Review** — does it read naturally to a native speaker?

## Rules

- **Never translate**: code, variable names, technical identifiers, URLs, placeholders (`{name}`)
- **Preserve markdown**: `**bold**`, `[links](url)`, code blocks stay as-is
- **Match register**: technical docs stay technical, casual stays casual
- **Localize examples**: use culturally appropriate names, currencies, dates
- **Flag ambiguity**: if the source is ambiguous, note both interpretations

## Quality Checklist

- [ ] Translation reads naturally (not word-for-word)
- [ ] Technical terms are correctly translated (or kept in English where standard)
- [ ] Formatting is preserved
- [ ] Placeholders are untouched
- [ ] Tone matches the original
