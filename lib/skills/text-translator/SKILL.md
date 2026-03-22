---
name: text-translator
type: skill
version: 0.3.0
description: Translate written content between human languages while preserving tone, meaning, and brand voice — with a glossary to keep terminology consistent
deps: []
tags:
  - translation
  - localization
  - content
---

# Text Translator Skill

Triggered by: "translate this to [language]", "localize for [market]", or any request to convert written content between human languages.

## Principles

Translation is not word substitution. Good translation preserves:
- **Meaning** — what is being said
- **Tone** — how it is being said
- **Intent** — why it is being said
- **Voice** — who is saying it

## Process

### Step 1: Establish Context
- What type of content? (UI copy, documentation, marketing, legal, technical)
- What is the target audience? (locale, expertise level, formality)
- Is there an existing glossary or style guide? Read it first.

### Step 2: First Pass — Meaning Translation
- Translate for accuracy first, naturalness second
- Preserve structure: headings, lists, code blocks stay untouched
- Technical terms stay in their internationally accepted form unless localized equivalents exist

### Step 3: Tone Alignment
- Read the translation mentally — does it sound natural in the target language?
- Adjust for formality level (German `Sie` vs `du`, French `vous` vs `tu`, Japanese keigo levels)
- Flag any phrases that are culturally awkward or potentially offensive in the target locale

### Step 4: Glossary Management
- If a glossary does not exist, create `docs/translations/glossary-[lang].md`
- Key terms and their approved translations
- The same term must use the same translation throughout — no variation

### Step 5: Review Pass
- Every sentence translated? No source-language text left in output?
- Code blocks, variable names, and markup tags untouched?
- Links updated to locale-specific resources where they exist?

## Output Format

After translation, summarize:
```
Translation: [target language] — [content type]
Source: [original language]

[translated content]

---
Glossary additions: [list of new terms, or "none"]
Cultural notes: [any awkward or ambiguous passages flagged, or "none"]
```

## Rules

- Never translate code, variable names, or markup tags
- Always preserve heading hierarchy and document structure
- If a term has no accepted translation, leave it in the source language and note it
- Never invent locale-specific resources (laws, contacts, addresses) — leave placeholders
- Cultural flags are informational — wait for user decision before adapting

## When NOT to Use

- For making a codebase i18n-ready (extracting strings, creating locale files) → use `skill:i18n`
- For translating code between programming languages → use `skill:code-translator`
