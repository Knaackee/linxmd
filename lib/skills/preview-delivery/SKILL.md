---
name: preview-delivery
type: skill
level: governance
version: 2.0.0
description: >
  How to prepare preview packages for human acceptance testing. Artifact bundles,
  live preview URLs, and acceptance evidence documentation.
tags: [governance, preview, delivery, acceptance-testing]
---

# Preview Delivery Skill

> Every significant change gets a preview before merge. The human validates in a real environment, not just via code review.

## Preview Options

### Option A: Artifact Bundle
Package the outputs for offline review:
```
.linxmd/previews/TASK-NNN/
├── README.md           ← What changed, how to test
├── screenshots/        ← Before/after screenshots
│   ├── before-*.png
│   └── after-*.png
├── release-notes.md    ← User-facing change description
├── migration-notes.md  ← If breaking changes exist
└── build-output/       ← Compiled artifacts if applicable
```

### Option B: Live Preview
Provide a running instance for hands-on testing:
- Dev server URL (e.g., Tailscale link, ngrok, Vercel preview)
- Include the URL in the task file and trace
- Ensure the preview environment has realistic data

### Option C: Both (Recommended)
For significant features, provide both artifact bundle AND live preview.

## Preview README Template

```markdown
# Preview: TASK-NNN — <Title>

## What Changed
Brief description of the changes.

## How to Test
1. Step-by-step testing instructions
2. Expected behavior for each step
3. Edge cases to verify

## Screenshots
| Before | After |
|--------|-------|
| ![before](screenshots/before-1.png) | ![after](screenshots/after-1.png) |

## Known Limitations
- Things that aren't finished yet
- Workarounds needed

## Preview URL (if applicable)
https://preview-url.example.com
```

## Review Criteria for Human

The human evaluates:
- [ ] Feature matches the spec and acceptance criteria
- [ ] UI/UX is correct (if applicable)
- [ ] No visual regressions
- [ ] Edge cases work
- [ ] Performance is acceptable

## Outcome

After human review:
- **Approved** → proceed to merge
- **Changes requested** → create fix tasks, route back to implementation
- **Rejected** → revisit spec, possibly redesign
