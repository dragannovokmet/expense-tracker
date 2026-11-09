# CLAUDE.md

Guidance for Claude Code when working in this repository.

## Project

Expense Tracker: a personal expense tracking app (web now, Android later) built as a **QA portfolio project**. The app exists to be tested well: the test strategy, the test design and the automation are the product. Every change should make the app more testable, not less.

Owner: Dragan Novokmet, QA Engineer. He reviews and approves every change. **Never push, open Issues or PRs, or publish anything without his explicit confirmation.**

## Stack

- .NET 10 (pinned in `global.json`), C# everywhere
- `src/ExpenseTracker.Api`: ASP.NET Core Web API (controllers), health check at `/health`
- `src/ExpenseTracker.Web`: Blazor WebAssembly (standalone) calling the API
- `src/ExpenseTracker.Core`: domain logic, no infrastructure dependencies
- `src/ExpenseTracker.Infrastructure`: EF Core + SQL Server (`AppDbContext`, migrations in `Persistence/Migrations`)
- Database: SQL Server LocalDB locally (`(localdb)\MSSQLLocalDB`), SQL Server container in CI
- NuGet: only nuget.org (`nuget.config` clears machine-level feeds on purpose)

## Tests

| Project | Framework | Needs running app? |
|---|---|---|
| `tests/ExpenseTracker.UnitTests` | xUnit v3 | No |
| `tests/ExpenseTracker.IntegrationTests` | xUnit v3 + `WebApplicationFactory` | No (needs a database) |
| `tests/ExpenseTracker.ApiTests` | NUnit + Playwright `APIRequestContext` | Yes, API on `API_BASE_URL` |
| `tests/ExpenseTracker.UiTests` | NUnit + Playwright | Yes, API and web on `WEB_BASE_URL` |

All test projects use Microsoft.Testing.Platform (`global.json` → `test.runner`).

## Commands

```bash
dotnet build
dotnet test --project tests/ExpenseTracker.UnitTests
dotnet test --project tests/ExpenseTracker.IntegrationTests

# Run the app (two terminals)
dotnet run --project src/ExpenseTracker.Api --launch-profile http   # http://localhost:5180
dotnet run --project src/ExpenseTracker.Web --launch-profile http   # http://localhost:5190

# With the app running
dotnet test --project tests/ExpenseTracker.ApiTests
dotnet test --project tests/ExpenseTracker.UiTests

# Playwright browsers (once, after build)
pwsh tests/ExpenseTracker.UiTests/bin/Debug/net10.0/playwright.ps1 install chromium

# New migration
dotnet ef migrations add <Name> --project src/ExpenseTracker.Infrastructure --startup-project src/ExpenseTracker.Api --output-dir Persistence/Migrations
```

The build treats warnings as errors, including NuGet vulnerability warnings. Fix the cause; do not suppress.

## Code rules

- Every element a test interacts with gets a `data-testid` attribute. Tests locate by test id or accessible role, never by CSS classes or layout.
- Money is `decimal`, never `double` or `float`.
- Current time comes from an injected `TimeProvider`, never `DateTime.Now`/`UtcNow` directly, so reports can be tested for any date (month end, leap year, year change).
- Domain rules live in `Core` and are unit tested there.
- Test-only endpoints (data seeding) exist only in the Test environment.

## Test rules

- UI tests use the Page Object Model (`tests/ExpenseTracker.UiTests/Pages`).
- Set up data through the API, then verify the behavior under test in the UI.
- Every test creates its own unique data and does not depend on other tests or on execution order. Tests must be able to run in parallel.
- No `Thread.Sleep` or fixed waits. Use Playwright's auto-waiting assertions (`Expect(...)`).
- Test names describe behavior: `Health_endpoint_returns_healthy`.
- Apply ISTQB test design techniques where they fit (boundary values, equivalence partitions, decision tables) and name the technique in the test or data source.

## Data protection

- **Never commit real bank statements, account numbers, notifications or personal data.** Tests use synthetic data only.
- Real statements may be used locally to learn a bank's format; they are not stored after import.

## Language

Code, comments, commit messages, README and docs are in English. The UI is English (localization to Serbian, Bosnian and Croatian comes later).

## Definition of Done (per feature)

1. User story with acceptance criteria
2. Test cases, linked in the traceability matrix (`docs/`)
3. Implementation
4. Automated tests at the right levels (unit, integration, API, UI)
5. Green CI
