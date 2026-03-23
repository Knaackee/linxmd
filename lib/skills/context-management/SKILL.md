---
name: context-management
type: skill
level: core
version: 2.0.0
description: >
  Manage LLM context windows effectively. Prioritize information, summarize
  strategically, and avoid context overflow.
tags: [core, context, llm, token-management]
---

# Context Management Skill

> LLM context windows are finite. Manage them like a scarce resource: prioritize, summarize, and drop what's not needed.

## Priority Stack

Load information in this order (highest priority first):

1. **PROJECT.md** — always loaded, non-negotiable
2. **Current task** — frontmatter, acceptance criteria, spec
3. **Relevant source files** — files being modified
4. **Recent traces** — last 1–2 sessions on the same task
5. **ADRs** — only those relevant to the current task
6. **Test files** — for the code being modified
7. **Everything else** — only on demand

## Context Window Strategies

### Chunking
When a file is too large to fit:
- Load the relevant section (function, class, module)
- Include 10–20 lines of surrounding context
- Note what was omitted: "Lines 1–50 and 200–300 omitted"

### Summarization
For long documents:
- Extract key points as bullet list
- Keep headings and first sentence of each section
- Preserve any data tables or schemas in full

### Caching
Between tool calls within a session:
- Remember what you already read
- Don't re-read unchanged files
- Track which files have been modified this session

## Anti-Patterns

- **Loading everything** — reading 50 files "just in case" wastes context
- **No prioritization** — treating all files as equally important
- **Ignoring summaries** — re-reading full files when a summary from traces exists
- **Context thrashing** — loading and unloading the same files repeatedly
