---
name: coninue
type: pack
version: 3.0.0
description: Bootstrap pack for Continue-first development. Starts with graph CLI interaction, graph memory policy, and product shaping as the first enterprise gate.
deps: []
tags: [pack, continue, bootstrap, enterprise, graph-memory]
---

# Coninue Pack

Bootstrap pack with the first stable Continue elements.

## Included Now

### Agents
- `product-manager`
- `router`
- `spec-writer`
- `planner`

### Skills
- `graph`
- `graph-memory`

### Workflow Coverage
- Stage 0 Product Clarity Gate via `product-manager`
- Shared lifecycle contract: Init -> Execute -> Post

## Purpose

Use this pack when you want to start enterprise-grade delivery with a strict discovery-first entry point before technical implementation agents are added.

## Incremental Expansion Plan

Planned next additions to this pack:
- `architect`
- `test-writer`
- `implementer`
- `reviewer`

## Mermaid Overview

```mermaid
flowchart LR
    A[Idea or Request] --> B[product-manager\nGate 0 Product Clarity]
    B --> C[Knowledge Delta\nwrite-node and write-edge]

    subgraph Current in coninue
      B
      C
      D[router]
      E[spec-writer]
      F[planner]
      G1[graph skill\nCLI interaction]
      G2[graph-memory skill\nmemory policy]
    end

    C --> D
    D --> E
    E --> F
    F -. next .-> H[architect]
    H -. next .-> I[test-writer]
    I -. next .-> J[implementer]
    J -. next .-> K[reviewer]

    subgraph Planned expansion
      H
      I
      J
      K
    end
```

## Exit Criteria for This Pack Stage

- Idea is converted into a clear product brief.
- Scope, success metrics, and NFRs are explicit.
- Durable knowledge is persisted with provenance.
- Handoff for technical workflow start is available.
