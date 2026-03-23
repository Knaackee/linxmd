---
name: performance-profiling
type: skill
level: governance
version: 2.0.0
description: >
  Before/after benchmarking, regression detection, profiling methodology,
  and performance report format. Measure, don't guess.
tags: [governance, performance, benchmarks, profiling, regression]
---

# Performance Profiling Skill

> Measure, don't guess. Every performance claim must be backed by data. Before/after comparisons are mandatory.

## Profiling Methodology

### Step 1: Establish Baseline
Before making changes, measure the current state:
- Run benchmarks on the affected code paths
- Record: p50, p95, p99 latency, throughput, memory usage
- Store baselines in `.linxmd/memory/benchmarks/`

### Step 2: Identify Targets
Based on the task, identify what to measure:

| Target Type | What to Measure | Tool Examples |
|-------------|----------------|---------------|
| API endpoints | Response time, throughput | k6, wrk, hey |
| Functions | Execution time, allocations | BenchmarkDotNet, pytest-benchmark |
| Build | Build time, bundle size | time, webpack-bundle-analyzer |
| Startup | Time to first response | Custom timer |
| Database | Query time, connection pool | EXPLAIN ANALYZE, query logs |
| Memory | Heap size, GC pressure, leaks | dotMemory, heaptrack, Chrome DevTools |

### Step 3: Profile
Run benchmarks in a **controlled environment**:
- Same machine/container spec as baseline
- Same data set
- No other processes interfering
- Multiple runs (minimum 3, prefer 10)
- Report median, not mean (less skew from outliers)

### Step 4: Compare

```markdown
## Performance Comparison — TASK-NNN

| Metric | Baseline | Current | Delta | Status |
|--------|----------|---------|-------|--------|
| p50 latency | 12ms | 11ms | -8% | ✅ improved |
| p95 latency | 45ms | 48ms | +7% | ⚠️ watch |
| p99 latency | 120ms | 180ms | +50% | 🚫 regression |
| Throughput | 1200 rps | 1180 rps | -2% | ✅ within tolerance |
| Memory (peak) | 128MB | 135MB | +5% | ✅ within tolerance |
```

### Step 5: Decide

| Delta | Action |
|-------|--------|
| Improvement | Celebrate, update baseline |
| < 10% regression | Note in report, acceptable if justified |
| 10–20% regression | Warning — requires justification in trace |
| > 20% regression | **Blocking** — must fix or get explicit human approval |

## Benchmark File Format

Store in `.linxmd/memory/benchmarks/`:

```json
{
  "benchmark": "api/users-list",
  "date": "2026-03-23",
  "environment": "dev-local, 16GB RAM, M1",
  "runs": 10,
  "results": {
    "p50_ms": 12,
    "p95_ms": 45,
    "p99_ms": 120,
    "throughput_rps": 1200,
    "memory_peak_mb": 128
  }
}
```

## Anti-Patterns

- **Premature optimization** — profile first, optimize second
- **Micro-benchmarking without context** — a 2ms function called once doesn't matter
- **Single run** — always do multiple runs to account for variance
- **No baseline** — "it feels faster" is not a measurement
- **Optimizing the wrong thing** — profile to find the actual bottleneck first
