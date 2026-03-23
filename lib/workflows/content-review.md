---
name: content-review
type: workflow
version: 2.0.0
description: >
  Create and review content: articles, blog posts, documentation pages.
  Draft → Edit → Fact-check → Review → Publish.
agents:
  - router
  - drafter
  - editor
  - fact-checker
  - reviewer-quality
skills:
  - trace-writing
gates: 3
tags: [workflow, content, writing, review, publishing]
---

# Content Review Workflow

> From blank page to published content. Structured drafting, editing, fact-checking, and review.

## Flow Diagram

```
BRIEF → DRAFT → ★GATE 1★ → EDIT → FACT-CHECK → ★GATE 2★ → FINAL POLISH → ★GATE 3★ → PUBLISH
```

## Phases

### 1. BRIEF
**Agent**: `router`
**Action**:
- Define: topic, audience, tone, length, format, deadline
- Record brief in `.linxmd/inbox/`

---

### 2. DRAFT
**Agent**: `drafter`
**Action**:
- Create outline first
- Write first draft with `[TODO]`, `[VERIFY]`, `[REVIEW]` markers
- Save to specified location

### ★ GATE 1: Draft Review ★
**Reviewer**: Human
**Validates**: Structure, coverage, tone direction, missing topics
**Outcome**: Approve for editing / Rewrite sections / Redirect

---

### 3. EDIT
**Agent**: `editor`
**Action**:
- Improve clarity, conciseness, flow
- Resolve `[TODO]` markers
- Fix grammar, style, consistency
- Produce edit summary

---

### 4. FACT-CHECK
**Agent**: `fact-checker`
**Action**:
- Verify all `[VERIFY]` markers
- Cross-reference technical claims against code/docs
- Check links, version numbers, examples
- Produce fact-check report

### ★ GATE 2: Edited Content Review ★
**Reviewer**: Human
**Validates**: Content quality, accuracy, fact-check results
**Outcome**: Approve / Request changes

---

### 5. FINAL POLISH
**Agent**: `editor`
**Action**:
- Final formatting pass
- Remove all remaining markers
- Ensure consistent style

### ★ GATE 3: Publish Approval ★
**Reviewer**: Human
**Validates**: Ready for publication
**Outcome**: Publish / Hold

---

### 6. PUBLISH
**Action**: Move to final location, update any indexes or links.

## Exit Criteria

- [ ] All markers resolved
- [ ] Fact-check passed
- [ ] Human approved all 3 gates
- [ ] Content in final location

