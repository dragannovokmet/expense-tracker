# Test Plan: Expense Tracker, Phase 1 (Web + API MVP)

| | |
|---|---|
| **Document version** | 0.3 (draft) |
| **Author / QA owner** | Dragan Novokmet |
| **Status** | Draft, under review |
| **Last updated** | 2026-11-23 |

## 1. Purpose

This plan describes how Phase 1 of Expense Tracker will be tested: what is in scope, which risks drive the testing effort, which test levels and techniques are used, how test data and environments are handled, and when testing is considered complete.

The plan is a living document. It is updated when scope, risks or the approach change, and every change is recorded in the revision history.

## 2. Scope

### 2.1 In scope

**Phase 1a: core**

| Area | Features |
|---|---|
| Authentication | Registration, sign-in, sign-out, access to protected pages and endpoints |
| Accounts | Bank accounts: create, view, edit, archive, restore and delete; country selection; account number and IBAN validation; bank recognition for Bosnia and Herzegovina; user-defined banks; changing the account number with a confirmation. Online accounts (PayPal, AirCash, Revolut and similar) with a provider instead of an account number. Cards on bank and online accounts: name, last 4 digits, card type, virtual flag |
| Categories | Predefined categories (a copy per user); create, rename, archive, restore and delete custom categories; at least one active category |
| Expenses | Create, view, edit and delete expenses, including description, note and currency (KM, EUR, USD, CAD); payment method and the account and card it was paid from; amounts with a decimal comma or point; list with filters (including "No account"), search that ignores diacritics, sorting and paging; input validation; concurrent edits |

**Phase 1b: reports and budget**

| Area | Features |
|---|---|
| Reports | Daily, weekly (Monday to Sunday) and monthly totals, overall, per category and per account (expenses without an account in their own group), with a chart. Totals are calculated **per currency**; amounts in different currencies are never added together |
| Budget | Monthly budget, overall and per category; budget vs actual; warnings at 80% and 100% |
| Planned expenses | Planned items for future months with an expected date and amount; statuses planned → paid / skipped; "Pay" creates a linked actual expense; plan vs actual per item and per month; copying selected items to the next month; delete and edit rules for future, current and past months |
| Expense groups | Groups such as "Basket" that collect real expenses, each keeping its own date and category; the group total is always calculated |
| Export | Export of expenses to CSV |

### 2.2 Out of scope for Phase 1

| Item | Reason |
|---|---|
| Smart categorization and AI features | Phase 2 |
| Android app | Phase 3 |
| Payment capture from notifications, statement import, reconciliation | Phase 4 |
| Booked amount in KM taken from the bank statement for card payments in a foreign currency | Phase 4, as part of reconciliation |
| Reminders for planned expenses | Phase 3, as Android notifications |
| Transfers between own accounts (cash withdrawal, credit card repayment, foreign currency to current account, top-up of an online account) | Phase 4. In Phase 1 they are not recorded; a transfer is never an expense |
| Income and the balance of online accounts | Phase 5 |
| Choosing a time zone per user | Later, with localization. In Phase 1 every user's time zone is `Europe/Sarajevo` |
| Localization (Serbian, Bosnian, Croatian), including number formats with a thousands separator | Phase 5 |
| Demo mode | Decided after Phase 1, together with the public demo |
| Load and performance testing | Single-user personal app at this stage; basic response times are watched in CI only |
| Penetration testing | Not planned. Authentication and authorization are tested functionally (see risk R2) |
| Branded browsers (Chrome, Edge) and real Safari | Chrome and Edge are covered through Chromium, which uses the same engine. Real Safari on macOS and iOS is not tested; WebKit in Playwright is used as the closest approximation |

## 3. References

- User stories with acceptance criteria: `docs/user-stories/` (Phase 1a reviewed; Phase 1b to be written)
- Test cases and traceability matrix: `docs/test-cases/` (to be written)
- Project and test rules: `CLAUDE.md`

## 4. Risk analysis

Testing effort is driven by risk. Each risk is rated for likelihood (L) and impact (I) from 1 (low) to 3 (high). The risk score is L × I and sets the test priority.

- **Product risks** are risks that the app itself fails or behaves incorrectly. Testing reduces them.
- **Project risks** are risks to the work of building and testing the app. Decisions and process changes reduce them, not tests.
- **Impact** is the consequence for the user:
  - 3: wrong money, lost data or another user's data
  - 2: a feature is wrong but the user notices it and can work around it
  - 1: a minor inconvenience
- **Likelihood** is an estimate, not a measurement. It is based on:
  - the complexity of the logic and the number of conditions
  - new code versus a mature library
  - measures that already prevent the defect, such as `decimal` for money
  - defects found so far

  Ratings are reviewed as testing finds defects.
- **Risk IDs are permanent.** Tests, the traceability matrix and bug reports refer to them, so the table stays in ID order and IDs are never renumbered. The score, not the position in the table, sets the priority.

### 4.1 Product risks

| ID | Risk | L | I | Score | How testing addresses it |
|---|---|---|---|---|---|
| R1 | Money is calculated or rounded incorrectly (totals, budget percentages, remaining amount) | 2 | 3 | **6** | Unit tests with boundary values and decimal edge cases; API and SQL checks that stored amounts match what was entered |
| R2 | A user can see or change another user's data (accounts, expenses, categories, banks) | 2 | 3 | **6** | Dedicated API tests for every endpoint with two users; unauthenticated requests rejected (401); foreign resources not found (404) |
| R3 | Report periods are wrong at boundaries (week start, month end, leap year, year change, time of day, daylight saving time) | 3 | 2 | **6** | Controlled clock (`TimeProvider`); boundary value tests on period start and end; decision table for period × date; days when daylight saving time starts and ends; just after local midnight, when the local and the UTC date differ |
| R4 | Invalid account numbers are accepted or valid ones rejected (check digits, length, country format) | 2 | 2 | 4 | Data-driven tests with valid and invalid IBANs and BBANs; equivalence partitions per country; mod 97 boundary cases |
| R5 | Bank is recognized incorrectly from the account number | 2 | 2 | 4 | Data-driven tests over the bank code catalog; unknown codes fall back to manual entry |
| R6 | Validation gaps let invalid expenses into the database (zero or negative amounts, future dates, too-long text) | 2 | 2 | 4 | Boundary values and equivalence partitions at API and UI level; database checks |
| R7 | Deleting or archiving a category or account corrupts or hides related expenses | 2 | 3 | **6** | State transition tests for categories and accounts; checks that history and reports stay correct |
| R8 | Filters, search, sorting or paging return wrong or incomplete results | 2 | 2 | 4 | Combination tests with pairwise selection; boundaries of page size and last page; search with and without diacritics and in different letter case; return to page 1 after a filter change |
| R9 | CSV export is wrong or unsafe (decimal separator, encoding of local characters such as č, ć, š, đ, ž; formula injection through text fields) | 2 | 2 | 4 | Content checks of the exported file; values starting with `=`, `+`, `-`, `@` are neutralized |
| R10 | Budget warnings appear at the wrong moment or not at all | 2 | 2 | 4 | Boundary values around 80% and 100% (just below, exactly at, just above) |
| R11 | Session problems: user stays signed in after sign-out, or protected pages are reachable without signing in | 1 | 3 | 3 | UI and API tests for sign-out and for direct navigation to protected URLs |
| R12 | Accessibility barriers prevent keyboard or screen reader use | 2 | 1 | 2 | Automated axe checks on main pages; manual keyboard-only pass during exploratory sessions |
| R13 | Amounts in different currencies are added together as if they were one currency (10 EUR + 10 KM shown as 20) | 2 | 3 | **6** | Reports, budget and group totals tested with mixed currencies; each currency has its own total; equivalence partitions per currency |
| R14 | Copying a plan to the next month produces wrong dates or duplicates, or changes a closed month | 2 | 3 | **6** | Boundary values on day 28–31, leap year and December → January; copying into a month that already has a plan changes nothing; decision table for month type × operation × paid or unpaid item |
| R15 | A group total differs from the sum of its expenses, or planned amounts are counted as spent | 2 | 3 | **6** | Invariant checks after adding, editing and deleting expenses in a group; reports checked with planned and paid items in the same month |
| R16 | A database migration loses or corrupts existing data | 1 | 3 | 3 | Migrations applied to a database that already contains data; row counts and key values checked before and after |
| R17 | A double click or double submit creates the same expense twice | 2 | 2 | 4 | UI test with a double click on save; API test with the same request sent twice |
| R18 | Two edits of the same expense overwrite each other (lost update) | 1 | 3 | 3 | Edit the same expense in two browser contexts; the second save is rejected (409) and the first change is kept; repeated delete of the same expense |
| R19 | An amount is read differently from what the user meant (decimal comma or point, thousands separator, pasted value, third decimal) | 2 | 3 | **6** | Equivalence partitions of input formats; boundary values on the number of decimals; UI tests for typing and pasting; API tests that reject three decimals; database check of the stored amount |
| R20 | An expense is linked to the wrong account or card (a card of another account, an account guessed by the app, an archived account cleared on edit) | 2 | 2 | 4 | Decision table payment method × account type × card; API test with a card that does not belong to the account; edit of an expense with an archived account |

### 4.2 Project risks

| ID | Risk | Mitigation |
|---|---|---|
| P1 | Flaky UI tests reduce trust in the suite | No fixed waits; auto-waiting assertions; independent tests with unique data; stability gate of 10 consecutive green runs before Phase 1 exit |
| P2 | AI-generated code or tests contain mistakes that look correct | Every change is reviewed by the QA owner; new tests are also reviewed by an independent test-review agent |
| P3 | Real personal or banking data ends up in the public repository | Synthetic data only (see section 7); review before every push |
| P4 | Single person in the QA role; no independent tester | Exploratory sessions with written charters; review checklist; test review agent as a second pair of eyes |
| P5 | Scope creep: new ideas keep entering Phase 1, so the phase never ends | Each new idea first goes into the project plan with a target phase. It enters Phase 1 only by a conscious decision, together with an update of this test plan. |
| P6 | Limited time: the project is built alongside a full-time job | Small steps; each feature is finished to its Definition of Done before the next one starts |
| P7 | Local and CI environments differ (Windows and LocalDB versus Linux and a SQL Server container, time zones) | The same tests run in both environments; time comes from `TimeProvider`; the CI runner is pinned to `ubuntu-24.04` |

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

**Smoke tests.** A small set of existing API and UI tests is marked with `[Category("Smoke")]`. These tests check the health endpoint, that the web app loads, sign-in, and adding an expense that then appears in the list. No separate smoke tests are written. The smoke set runs:
- first in CI. If it fails, the full UI suite is not started (fail fast).
- locally before an exploratory session.
- after a deployment, once a public demo exists.

The health check alone is not a smoke test. It only shows that the API is running and can reach the database.

**Automation principle.** A check is automated at the lowest level that can catch the defect. Business rules are covered by unit tests; the API layer checks contracts and authorization; UI tests cover the flows a user really goes through, not every rule again. UI tests prepare their data through the API and only verify the behavior under test.

### 5.2 Test design techniques

| Technique | Where it is used |
|---|---|
| Equivalence partitioning | Amounts and amount input formats (decimal comma, decimal point, thousands separator), text lengths, account number formats per country, account types (bank, online), card types, filter values |
| Boundary value analysis | Amount limits, date limits, page sizes, budget thresholds (80%, 100%), report period start and end; day 28–31 when copying a plan; month change at 23:59 and 00:00 in the user's time zone; days when daylight saving time starts and ends; 2 and 3 decimals in an amount |
| Decision tables | Report period × date; budget state (no budget, under, warning, over); category state × action; payment method × account type × card; month type (future, current, past) × operation × paid or unpaid planned item |
| State transition testing | Category lifecycle (active, archived, deleted only without expenses); account lifecycle, for bank and online accounts; sign-in session; planned item (planned → paid or skipped; no return from paid while linked to an expense) |
| Pairwise testing | Combinations of list filters and sorting |
| Error guessing and checklists | Empty input, whitespace only, local characters, very long input, double submit, browser back button, pasted values, the same record open in two tabs |
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
| Local, manual use | LocalDB `ExpenseTracker` | Development, debugging, exploratory testing. Automated tests never write to this database. |
| Local, integration tests | LocalDB `ExpenseTracker_IntegrationTests` | Integration tests. The database is dropped and migrated again at the **start** of each run, so every run begins clean; data is kept after the run for debugging. |
| Local, API and UI tests | LocalDB `ExpenseTracker_E2E` | API and UI tests against the running app, started with the `e2e` launch profile (Test environment, test-only seeding endpoint enabled). Set up together with the first feature that writes data (registration). |
| CI (GitHub Actions, `ubuntu-24.04`) | SQL Server container | All automated tests on every push and pull request; nightly full run. The container is new for every run. |

- **Time control:** the app reads the current time from an injected `TimeProvider`. In tests the clock is fixed, so period boundaries can be tested for any date.
- **Time zones:** the server and CI run in UTC. In Phase 1 every user's time zone is `Europe/Sarajevo` (UTC+1, UTC+2 in summer), and "today" is calculated in that zone.
- **Test-only endpoints** (for example data seeding) exist only in the Test environment and are checked to be unavailable elsewhere.

## 7. Test data

- **Synthetic data only.** No real names, account numbers, bank statements or notifications are committed. This applies to the code, the tests and the documentation.
- **Valid test account numbers** are generated with correct check digits, using real bank codes from the public catalog but invented account numbers.
- **Three kinds of test data:**

| Kind | How it is created | Example |
|---|---|---|
| Reference data | Seeded by migrations; read-only for tests | BiH bank catalog, predefined categories |
| Per-test data | Created by each test through builders, the API or the seeding endpoint, with unique values | A unique user per test class with its own accounts and expenses |
| Synthetic bulk data | Generated with Bogus for paging, filtering and report tests and for exploratory sessions | Three months of invented expenses |

- **Each test creates the data it needs** and does not rely on data left by other tests. Shared, unexplained data ("mystery guest") is avoided, because a test should show everything it depends on.
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

Testing of a feature is suspended when the build or the deployment of the app is broken, or when a blocking defect prevents meaningful testing. It resumes when the cause is fixed and the smoke set (section 5.1) passes.

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
2. ✅ Test plan and Phase 1a user stories reviewed
3. Phase 1a: authentication → accounts → categories → expenses, each with its tests
4. Phase 1b user stories, then Phase 1b: reports → budget → budget vs actual → planned expenses and copying a month → expense groups → CSV export, each with its tests
5. Exploratory sessions, defect fixing, stability gate (10 green runs)
6. Test summary report and Phase 1 exit

## 14. Revision history

| Version | Date | Change |
|---|---|---|
| 0.1 | 2026-11-09 | First draft |
| 0.2 | 2026-11-22 | After the first review:<br>- added planned expenses, expense groups, description, note and per-currency totals to the scope<br>- clarified browser coverage<br>- explained how likelihood and impact are rated<br>- added risks R13–R17 and P5–P7<br>- added the smoke set<br>- added test databases per environment and the three kinds of test data |
| 0.3 | 2026-11-23 | After the Phase 1a user stories:<br>- added online accounts, card types, virtual cards, payment methods, CAD and the "No account" filter to the scope<br>- moved transfers, income, online account balance and the time zone choice out of scope<br>- extended R3 (daylight saving time) and R8 (diacritics)<br>- added risks R18–R20<br>- added the time zone rule to the test environments<br>- extended the test design techniques |
