---
name: e2e-testing
type: skill
level: governance
version: 2.0.0
description: >
  End-to-end testing strategy: user flows, critical paths, visual regression,
  and tooling (Playwright, Cypress). When and how to E2E test.
tags: [governance, testing, e2e, playwright, cypress, integration]
---

# E2E Testing Skill

> E2E tests verify that the whole system works from the user's perspective. They're expensive — use them strategically for critical paths.

## When to Use E2E Tests

| Scenario | E2E? | Why |
|----------|------|-----|
| Critical user flows (login, checkout, signup) | ✅ Yes | High business impact |
| API contract validation | ✅ Yes | Integration correctness |
| Edge case in a utility function | ❌ No | Unit test is better |
| Visual regression on key pages | ✅ Yes | UI correctness |
| Internal helper function | ❌ No | Unit test is cheaper |

## Test Strategy

### Test Pyramid
```
        ╱  E2E  ╲       ← Few: critical paths only
       ╱──────────╲
      ╱ Integration ╲    ← Some: component interactions
     ╱────────────────╲
    ╱    Unit Tests     ╲  ← Many: fast, isolated, comprehensive
```

### Critical Path Coverage
Identify the top 5–10 user journeys and ensure E2E coverage:
1. User registration → email verification → first login
2. Login → dashboard → key action
3. Create → edit → delete (CRUD cycle)
4. Payment → confirmation → receipt
5. Error states → recovery flows

## Test Structure

```typescript
test.describe('User Authentication', () => {
  test('should allow login with valid credentials', async ({ page }) => {
    // Arrange
    await page.goto('/login');
    
    // Act
    await page.fill('[data-testid="email"]', 'user@example.com');
    await page.fill('[data-testid="password"]', 'validpassword');
    await page.click('[data-testid="submit"]');
    
    // Assert
    await expect(page).toHaveURL('/dashboard');
    await expect(page.locator('[data-testid="welcome"]')).toBeVisible();
  });
});
```

## Rules

- **Use data-testid attributes** — not CSS classes or text content
- **Independent tests** — each test creates its own state
- **Deterministic** — no flaky tests (retry mechanisms for network, not for logic)
- **Fast startup** — use API calls for setup, UI interactions for the actual test
- **Clean up** — reset state after each test
- **Screenshot on failure** — automatically capture visual state when a test fails

## Anti-Patterns

- **Testing everything E2E** — use unit/integration tests for most things
- **Fragile selectors** — CSS classes change; data-testid attributes are stable
- **Sequential dependencies** — test B depends on test A having run first
- **Magic waits** — `sleep(3000)` instead of waiting for specific conditions
