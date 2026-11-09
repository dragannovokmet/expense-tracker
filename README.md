# Expense Tracker

[![CI](https://github.com/dragannovokmet/expense-tracker/actions/workflows/ci.yml/badge.svg)](https://github.com/dragannovokmet/expense-tracker/actions/workflows/ci.yml)

A personal expense tracking app, built as a **QA portfolio project**. The focus is on how the product is tested: test strategy, test design, manual test artefacts and automation at every level.

> 🚧 Work in progress. The project skeleton and CI are in place; features are being added phase by phase.

## Tech stack

- **App:** .NET 10, ASP.NET Core Web API, Blazor WebAssembly, EF Core, SQL Server
- **Tests:** xUnit v3, NUnit, Playwright for .NET (API and UI)
- **CI:** GitHub Actions with a SQL Server container

## Test levels

| Level | Tooling | What it covers |
|---|---|---|
| Unit | xUnit v3 | Domain logic |
| Integration | xUnit v3 + `WebApplicationFactory` | API with a real database, in-process |
| API (black-box) | Playwright `APIRequestContext` + NUnit | The running API over HTTP |
| UI end-to-end | Playwright + NUnit | User flows in a real browser |

## Running locally

Prerequisites: .NET 10 SDK, SQL Server LocalDB.

```bash
dotnet build
dotnet test --project tests/ExpenseTracker.UnitTests
dotnet test --project tests/ExpenseTracker.IntegrationTests

# Start the app (two terminals)
dotnet run --project src/ExpenseTracker.Api --launch-profile http   # http://localhost:5180
dotnet run --project src/ExpenseTracker.Web --launch-profile http   # http://localhost:5190

# Install the Playwright browser once, then run the API and UI tests
pwsh tests/ExpenseTracker.UiTests/bin/Debug/net10.0/playwright.ps1 install chromium
dotnet test --project tests/ExpenseTracker.ApiTests
dotnet test --project tests/ExpenseTracker.UiTests
```

## Author

Dragan Novokmet, QA Engineer · [LinkedIn](https://www.linkedin.com/in/dragan-novokmet)
