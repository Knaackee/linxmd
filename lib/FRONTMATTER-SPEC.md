# Artifact Frontmatter Spec v2.1

This document defines the shared YAML frontmatter schema for all Linxmd artifacts.

## Shared Fields

```yaml
---
name: artifact-name
type: agent|skill|workflow|pack
version: 2.1.0
description: Short summary
deps: []
tags: []
supported: []
quickActions: []
lifecycle: {}
---
```

## `quickActions`

`quickActions` expose context-aware prompt shortcuts.

```yaml
quickActions:
  - id: write-e2e
    label: Generate E2E test
    prompt: Write an E2E test for the current file.
    trigger:
      fileMatch:
        - "^src/pages/.*\\.(ts|tsx)$"
        - "^src/routes/.*\\.(ts|tsx)$"
      fileExclude:
        - "\\.(spec|test|e2e)\\.(ts|tsx)$"
      workspaceHas:
        - "playwright.config.ts"
      workspaceMissing:
        - "PROJECT.md"
      languageId:
        - "typescript"
        - "typescriptreact"
      contentMatch:
        - "export default function"
```

### Trigger Semantics

- `fileMatch` is required and must be a non-empty list of regex patterns. OR semantics.
- `fileExclude` is a list of regex patterns. OR semantics.
- `workspaceHas` is a list of files/folders that must exist.
- `workspaceMissing` is a list of files/folders that must not exist.
- `languageId` is a list of editor language IDs.
- `contentMatch` is a list of regex patterns tested against file content.
- `fileExclude`, `workspaceHas`, `workspaceMissing`, `languageId`, and `contentMatch` are optional.
- All provided trigger groups are combined with AND semantics.

## `lifecycle`

Lifecycle hooks define prompts around install/remove/update operations.

```yaml
lifecycle:
  preInstall:
    - id: check-prerequisites
      label: Validate prerequisites
      prompt: Verify required toolchain and config files.
      blocking: true
      requiresConfirmation: true

  postInstall:
    - id: kickoff
      label: Start onboarding
      prompt: Run the kickoff onboarding flow for this artifact.

  preUninstall:
    - id: dependents-check
      label: Check dependents
      prompt: Validate there are no unresolved dependents.
      blocking: true

  postUninstall:
    - id: cleanup
      label: Cleanup hints
      prompt: Suggest cleanup actions after uninstall.

  preUpdate:
    - id: backup
      label: Backup state
      prompt: Create backup before updating.

  postUpdate:
    - id: migrate
      label: Migration hints
      prompt: Provide migration checklist for this update.
```

### Lifecycle Semantics

- `pre*` hooks run before action and may block.
- `post*` hooks run after action and should provide guidance.
- `blocking` defaults to `false`.
- `requiresConfirmation` defaults to `false`.

## Backward Compatibility

- Unknown frontmatter fields are ignored by parser.
- Missing optional sections default to empty lists/empty object.
- Existing artifacts without `quickActions` or `lifecycle` remain valid.
