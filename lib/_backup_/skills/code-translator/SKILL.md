---
name: code-translator
type: skill
level: core
version: 2.0.0
description: >
  Translate code between programming languages while preserving behavior,
  idioms, and patterns of the target language.
tags: [core, translation, language, migration]
---

# Code Translator Skill

> Translate code between languages idiomatically. Don't transliterate — adapt to the target language's conventions and patterns.

## Process

1. **Understand the source** — What does this code do? What are the invariants?
2. **Map concepts** — How does the target language express the same concepts?
3. **Translate idiomatically** — Use the target language's patterns, not source patterns
4. **Verify** — Does the translated code preserve all behavior?

## Translation Principles

- **Preserve behavior, not syntax** — a Python list comprehension might become a LINQ query in C#
- **Use idiomatic patterns** — Java's Builder pattern → Kotlin's named parameters with defaults
- **Respect type systems** — dynamic → static requires explicit type annotations
- **Handle error models** — exceptions vs. Result types vs. error codes
- **Maintain tests** — translate tests to verify behavioral equivalence

## Common Mappings

| Concept | Python | TypeScript | C# | Go |
|---------|--------|-----------|-----|-----|
| Null handling | Optional/None | undefined/null | nullable | zero value |
| Error handling | Exception | throw/catch | Exception | error return |
| Async | async/await | async/await | async/await | goroutine |
| Collections | list, dict | Array, Map | List, Dictionary | slice, map |
| Iteration | for..in | for..of | foreach | for range |
