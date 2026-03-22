---
name: code-translator
type: skill
version: 0.3.0
description: Port code between programming languages the right way — idiomatically, not literally — with test suite verification before and after
deps: []
tags:
  - translation
  - migration
  - porting
---

# Code Translator Skill

Triggered by: "translate to [language]", "port to [language]", "rewrite in [language]", or any request to convert code from one programming language to another.

## Principles

A code translation is only trustworthy if the same tests pass before and after.
Never translate syntax literally — translate intent idiomatically.

## Process

### Before You Start
1. Run the full test suite on the source code — it must be green before translation begins
2. Capture the passing test count as a baseline

### Step 1: Audit the Source Module
- Identify language-specific patterns (decorators, metaprogramming, generics, closures, macros)
- List stdlib/framework calls that need mapping (e.g., Python `dataclass` → TypeScript interface)
- Flag constructs with no direct target equivalent — write to `NOTES.md` as Open Items

### Step 2: Map the Dependency Surface
- Identify third-party libraries and find idiomatic target-language equivalents
- Prefer well-maintained, widely-used equivalents — don't just wrap the original library

### Step 3: Translate Module by Module
One logical module = one translation unit. Not file by file, not line by line.
Write the target-language module following idiomatic conventions: naming, error handling, nullability, concurrency patterns.

### Step 4: Port the Test Suite
- Every test from the source language must have a corresponding test in the target language
- Tests must pass on the original source before translation begins
- The same tests (ported) must pass on the translated code

### Step 5: Verify Correctness
- Run the full ported test suite — zero regressions allowed
- Any test that cannot be ported must be flagged in `NOTES.md`

### Step 6: Translation Report
```
Translation Report
==================
Source language:   [lang]
Target language:   [lang]
Modules translated: [N]
Tests: [N] source → [N] ported, all passing

Decisions:
- [library A] → [library B] because [reason]
- [pattern X] expressed as [pattern Y] in target

Open Items:
- [untranslatable construct] — no direct equivalent in target
```

## Idiomatic Mapping Reference

| Source Pattern | Target Idiomatic Equivalent |
|---|---|
| Python `dataclass` | TypeScript interface + record type / Rust `#[derive]` struct |
| Python `async/await` | TypeScript `async/await` / Go goroutines |
| Java checked exceptions | Swift/Kotlin `Result<T, Error>` / Rust `Result<T, E>` |
| C# LINQ | Kotlin `.map/.filter/.reduce` / Python list comprehension |
| Go error return | Rust `Result<T, E>` / C# `(value, error)` tuple |
| JavaScript `prototype` | Kotlin `companion object` / Python `@classmethod` |

## Rules

- Never translate before tests pass on the source code
- Translate intent, not syntax
- If a construct has no target equivalent, document it — do not invent a workaround
- All tests must pass after each module translation before moving to the next
- One module at a time — whole-project translation in one step always fails

## When NOT to Use

- When the target is the same language (minor refactoring) → use `skill:refactoring`
- When translating prose or documentation → use `skill:text-translator`
- When making a codebase i18n-ready → use `skill:i18n`
