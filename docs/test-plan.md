# Test Plan: Expense Tracker, Phase 1 (Web + API MVP)

| | |
|---|---|
| **Document version** | 0.1 (draft) |
| **Author / QA owner** | Dragan Novokmet |
| **Status** | Draft, under review |
| **Last updated** | 2026-11-09 |

## 1. Purpose

This plan describes how Phase 1 of Expense Tracker will be tested: what is in scope, which risks drive the testing effort, which test levels and techniques are used, how test data and environments are handled, and when testing is considered complete.

The plan is a living document. It is updated when scope, risks or the approach change, and every change is recorded in the revision history.

## 2. Scope

### 2.1 In scope

**Phase 1a: core**

| Area | Features |
|---|---|
| Authentication | Registration, sign-in, sign-out, access to protected pages and endpoints |
| Accounts | Create, view, edit and delete bank accounts; country selection; account number and IBAN validation; bank recognition for Bosnia and Herzegovina; user-defined banks |
| Categories | Predefined categories; create, rename and archive custom categories |
| Expenses | Create, view, edit and delete expenses; list with filters, search, sorting and paging; input validation |

**Phase 1b: reports and budget**

| Area | Features |
|---|---|
| Reports | Daily, weekly (Monday to Sunday) and monthly totals, overall and per category, with a chart |
| Budget | Monthly budget, overall and per category; budget vs actual; warnings at 80% and 100% |
| Export | Export of expenses to CSV |

### 2.2 Out of scope for Phase 1

| Item | Reason |
|---|---|
| Smart categorization and AI features | Phase 2 |
| Android app | Phase 3 |
| Payment capture from notifications, statement import, reconciliation | Phase 4 |
| Localization (Serbian, Bosnian, Croatian) | Phase 5 |
| Demo mode | Decided after Phase 1, together with the public demo |
| Load and performance testing | Single-user personal app at this stage; basic response times are watched in CI only |
| Penetration testing | Not planned. Authentication and authorization are tested functionally (see risk R2) |
| Browsers other than current Chromium, Firefox and WebKit | Not targeted |

## 3. References

- User stories with acceptance criteria: `docs/user-stories/` (to be written)
- Test cases and traceability matrix: `docs/test-cases/` (to be written)
- Project and test rules: `CLAUDE.md`

## 4. Risk analysis

Testing effort is driven by risk. Each risk is rated for likelihood (L) and impact (I) from 1 (low) to 3 (high). The risk score is L × I and sets the test priority.

### 4.1 Product risks

| ID | Risk | L | I | Score | How testing addresses it |
|---|---|---|---|---|---|
| R1 | Money is calculated or rounded incorrectly (totals, budget percentages, remaining amount) | 2 | 3 | **6** | Unit tests with boundary values and decimal edge cases; API and SQL checks that stored amounts match what was entered |
| R2 | A user can see or change another user's data (accounts, expenses, categories, banks) | 2 | 3 | **6** | Dedicated API tests for every endpoint with two users; unauthenticated requests rejected (401); foreign resources not found (404) |
| R3 | Report periods are wrong at boundaries (week start, month end, leap year, year change, time of day) | 3 | 2 | **6** | Controlled clock (`TimeProvider`); boundary value tests on period start and end; decision table for period × date |
| R4 | Invalid account numbers are accepted or valid ones rejected (check digits, length, country format) | 2 | 2 | 4 | Data-driven tests with valid and invalid IBANs and BBANs; equivalence partitions per country; mod 97 boundary cases |
| R5 | Bank is recognized incorrectly from the account number | 2 | 2 | 4 | Data-driven tests over the bank code catalog; unknown codes fall back to manual entry |
| R6 | Validation gaps let invalid expenses into the database (zero or negative amounts, future dates, too-long text) | 2 | 2 | 4 | Boundary values and equivalence partitions at API and UI level; database checks |
| R7 | Deleting or archiving a category or account corrupts or hides related expenses | 2 | 3 | **6** | State transition tests for categories and accounts; checks that history and reports stay correct |
| R8 | Filters, search, sorting or paging return wrong or incomplete results | 2 | 2 | 4 | Combination tests with pairwise selection; boundaries of page size and last page |
| R9 | CSV export is wrong or unsafe (decimal separator, encoding of local characters such as č, ć, š, đ, ž; formula injection through text fields) | 2 | 2 | 4 | Content checks of the exported file; values starting with `=`, `+`, `-`, `@` are neutralized |
| R10 | Budget warnings appear at the wrong moment or not at all | 2 | 2 | 4 | Boundary values around 80% and 100% (just below, exactly at, just above) |
| R11 | Session problems: user stays signed in after sign-out, or protected pages are reachable without signing in | 1 | 3 | 3 | UI and API tests for sign-out and for direct navigation to protected URLs |
| R12 | Accessibility barriers prevent keyboard or screen reader use | 2 | 1 | 2 | Automated axe checks on main pages; manual keyboard-only pass during exploratory sessions |

### 4.2 Project risks

| ID | Risk | Mitigation |
|---|---|---|
| P1 | Flaky UI tests reduce trust in the suite | No fixed waits; auto-waiting assertions; independent tests with unique data; stability gate of 10 consecutive green runs before Phase 1 exit |
| P2 | AI-generated code or tests contain mistakes that look correct | Every change is reviewed by the QA owner; new tests are also reviewed by an independent test-review agent |
| P3 | Real personal or banking data ends up in the public repository | Synthetic data only (see section 7); review before every push |
| P4 | Single person in the QA role; no independent tester | Exploratory sessions with written charters; review checklist; test review agent as a second pair of eyes |

## 5. Test approach

### 5.1 Test levels

| Level | Tool | Focus | Runs |
|---|---|---|---|
| Unit | xUnit v3 | Domain rules in `Core`: money, periods, budget, validation, account number checks | Every push |
| Integration | xUnit v3 + `WebApplicationFactory` | API with a real database, in-process: persistence, migrations, error handling | Every push |
| API (black-box) | Playwright `APIRequestContext` + NUnit | The running API over HTTP: status codes, validation messages, response shape, authorization | Every push |
| Database checks | SQL (Dapper) inside API and UI tests | Data is stored exactly as the API or UI reports | With API and UI tests |
| UI end-to-end | Playwright + NUnit | Key user flows in a real browser | Every push (Chromium); nightly (Chromium, Firefox, WebKit) |
| Accessibility | axe-core for Playwright | WCAG 2.1 AA rules on main pages | Nightly |
| Exploratory | Manual, session-based | Areas where scripted tests are weak: usability, unexpected input, combinations | Once per feature area, before Phase 1 exit |

**Automation principle.** A check is automated at the lowest level that can catch the defect. Business rules are covered by unit tests; the API layer checks contracts and authorization; UI tests cover the flows a user really goes through, not every rule again. UI tests prepare their data through the API and only verify the behavior under test.

### 5.2 Test design techniques

| Technique | Where it is used |
|---|---|
| Equivalence partitioning | Amounts, text lengths, account number formats per country, filter values |
| Boundary value analysis | Amount limits, date limits, page sizes, budget thresholds (80%, 100%), report period start and end |
| Decision tables | Report period × date; budget state (no budget, under, warning, over); category state × action |
| State transition testing | Category lifecycle (active, archived); account lifecycle; sign-in session |
| Pairwise testing | Combinations of list filters and sorting |
| Error guessing and checklists | Empty input, whitespace only, local characters, very long input, double submit, browser back button |
| Exploratory testing | Charters per feature area, time-boxed to 60–90 minutes, notes kept in `docs/exploratory/` |

### 5.3 Test types

- **Functional:** each acceptance criterion has at least one positive and one negative test.
- **Data integrity:** what is shown in the UI, returned by the API and stored in the database must match.
- **Authorization:** each endpoint is tested with no user, with the owner and with a different user.
- **Cross-browser and responsive:** Chromium, Firefox and WebKit; desktop (1280×720) and mobile (390×844) viewports for main flows.
- **Accessibility:** automated axe checks plus a manual keyboard-only pass.
- **Regression:** the full automated suite runs on every push; every fixed defect gets a regression test.

## 6. Test environments

| Environment | Database | Purpose |
|---|---|---|
| Local | SQL Server LocalDB | Development, debugging, exploratory testing |
| CI (GitHub Actions, Ubuntu) | SQL Server container | All automated tests on every push and pull request; nightly full run |

- **Time control:** the app reads the current time from an injected `TimeProvider`. In tests the clock is fixed, so period boundaries can be tested for any date.
- **Test-only endpoints** (for example data seeding) exist only in the Test environment and are checked to be unavailable elsewhere.

## 7. Test data

- **Synthetic data only.** No real names, account numbers, bank statements or notifications are committed. This applies to the code, the tests and the documentation.
- **Valid test account numbers** are generated with correct check digits, using real bank codes from the public catalog but invented account numbers.
- **Each test creates its own data** with unique values (for example a unique user per test class), so tests are independent and can run in parallel.
- **Reference data** (BiH bank catalog, predefined categories) is seeded and treated as read-only by tests.
- **Test data builders** keep test setup short and readable.

## 8. Entry, exit and suspension criteria

### 8.1 Entry criteria (per feature)

- User story with acceptance criteria is written and reviewed.
- Test cases are designed and linked in the traceability matrix.
- The feature builds and the existing suite is green.

### 8.2 Exit criteria (Phase 1)

- All Phase 1 user stories are implemented and every acceptance criterion is covered by at least one test (traceability matrix complete).
- All automated tests pass in CI.
- The suite has passed **10 consecutive runs** without a flaky failure.
- No open defects of severity Critical or High. Medium and Low defects are either fixed or documented with a decision.
- At least one exploratory session per feature area is completed and its findings are handled.
- The README lets someone set up and run the project on a clean machine.
- A test summary report for Phase 1 is written.

### 8.3 Suspension and resumption

Testing of a feature is suspended when the build or the deployment of the app is broken, or when a blocking defect prevents meaningful testing. It resumes when the cause is fixed and a smoke check (health check plus sign-in) passes.

## 9. Defect management

- Defects are reported as **GitHub Issues** using a bug report template: summary, steps to reproduce, expected result, actual result, environment, severity, evidence (screenshot, Playwright trace, logs).
- **Severity** describes the impact on the product:

| Severity | Meaning | Example |
|---|---|---|
| Critical | Data loss, wrong money calculations, or one user can access another user's data | Budget total ignores some expenses |
| High | A main feature does not work and there is no workaround | Expense cannot be saved |
| Medium | A feature works partly or a workaround exists | Filter by category ignores archived categories |
| Low | Cosmetic issue or minor inconvenience | Misaligned label |

- **Priority** (P1–P3) is set separately and decides the order of fixing.
- **Lifecycle:** New → Confirmed → In progress → Fixed → Verified → Closed (or Rejected / Won't fix with a reason). A defect is closed only after it is verified and a regression test exists.

## 10. Deliverables

| Deliverable | Location |
|---|---|
| Test plan (this document) | `docs/test-plan.md` |
| User stories with acceptance criteria | `docs/user-stories/` |
| Test cases and traceability matrix | `docs/test-cases/` |
| Exploratory session notes | `docs/exploratory/` |
| Bug reports | GitHub Issues |
| Automated test suites | `tests/` |
| CI test results | GitHub Actions runs |
| Test summary report for Phase 1 | `docs/reports/` |

## 11. Roles and responsibilities

| Role | Responsibility |
|---|---|
| QA owner (Dragan Novokmet) | Test planning, risk analysis, test design, exploratory testing, review and approval of all code and tests, defect triage, release decision |
| AI assistant (Claude Code) | Drafting implementation, tests and documents under the rules in `CLAUDE.md`; nothing is merged or published without the QA owner's approval |
| Test review agent | Independent review of new tests for flakiness, weak assertions, duplication and missed boundaries |

## 12. Metrics

| Metric | Target at Phase 1 exit |
|---|---|
| Acceptance criteria covered by tests | 100% |
| Automated test pass rate in CI | 100% |
| Consecutive green runs without flaky failures | 10 |
| Open Critical / High defects | 0 |
| CI duration for a push | Under 10 minutes |

Defects found are also tracked by severity and by the level where they were found (unit, API, UI, exploratory), to see which test levels are most effective.

## 13. Milestones

1. ✅ Project skeleton, health check and CI
2. Test plan and Phase 1 user stories reviewed
3. Phase 1a: authentication → accounts → categories → expenses, each with its tests
4. Phase 1b: reports → budget → budget vs actual → CSV export, each with its tests
5. Exploratory sessions, defect fixing, stability gate (10 green runs)
6. Test summary report and Phase 1 exit

## 14. Revision history

| Version | Date | Change |
|---|---|---|
| 0.1 | 2026-11-09 | First draft |
