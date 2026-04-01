# Linxmd Library

> Current shipped library artifacts for discovery-first Linxmd workflows.

## Current Scope

The library currently ships a small core set of artifacts:

- 4 agents: `product-manager`, `router`, `spec-writer`, `planner`
- 2 skills: `graph`, `graph-memory`
- 1 pack: `coninue`
- 0 standalone workflows in `lib/workflows/`

Legacy backup artifacts have been removed from the shipped library and are no longer indexed.

## Quick Install

```bash
linxmd add pack:coninue --yes
linxmd add agent:planner --yes
linxmd add skill:graph --yes
```

## Included Artifacts

### Agents

- `product-manager`: shapes ideas into a clear product brief
- `router`: classifies work and routes it to the right flow
- `spec-writer`: turns requests into scoped specifications
- `planner`: decomposes specs into executable plans

### Skills

- `graph`: CLI interaction protocol for graph-memory
- `graph-memory`: durable memory policy for graph-backed context

### Pack

- `coninue`: bootstrap pack bundling the current discovery-first core

## Directory Structure

```text
lib/
├── PRINCIPLES.md
├── ARCHITECTURE.md
├── FRONTMATTER-SPEC.md
├── README.md
├── index.json
├── agents/
│   ├── product-manager.md
│   ├── router.md
│   ├── spec-writer.md
│   ├── planner.md
│   └── SPEC.md
├── skills/
│   ├── graph/
│   │   └── SKILL.md
│   ├── graph-memory/
│   │   └── SKILL.md
│   └── SPEC.md
├── packs/
│   ├── coninue.md
│   └── SPEC.md
└── workflows/
    └── SPEC.md
```

## Specs

- Shared frontmatter schema: [FRONTMATTER-SPEC.md](d:\Development\linxmd\lib\FRONTMATTER-SPEC.md)
- Agents: [agents/SPEC.md](d:\Development\linxmd\lib\agents\SPEC.md)
- Skills: [skills/SPEC.md](d:\Development\linxmd\lib\skills\SPEC.md)
- Workflows: [workflows/SPEC.md](d:\Development\linxmd\lib\workflows\SPEC.md)