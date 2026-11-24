# Test Cases

Test cases turn the acceptance criteria from `docs/user-stories/` into concrete checks: which data is used, at which test level, and what result is expected. The traceability matrix (`traceability.md`) links every acceptance criterion to its test cases, the automated tests and the product risks.

## Files

| File | Area | Phase | Status |
|---|---|---|---|
| [01-authentication.md](01-authentication.md) | Registration, sign-in, sign-out, access control | 1a | Draft |
| [traceability.md](traceability.md) | Acceptance criterion → test cases → automated tests → risks | 1a | Draft |

Test cases for accounts, categories and expenses are added before each of those features is implemented.

## Test case format

Each story has one table:

| Column | Meaning |
|---|---|
| **ID** | `TC-<AREA>-NNN`, for example `TC-AUTH-005`. Permanent, never renumbered |
| **Title** | What is checked, as a short behavior |
| **Covers** | The acceptance criteria it checks (`AC4`, or `AC5, AC6`) |
| **Type** | `+` positive (valid input, expected success), `−` negative (invalid input, expected rejection), or `+ / −` when one test case checks both, often with a positive control step |
| **Technique** | ISTQB technique used to choose the data: EP (equivalence partitioning), BVA (boundary value analysis), DT (decision table), ST (state transition), EG (error guessing), or – for a plain functional check |
| **Level** | Unit, Integration, API, UI, plus DB when the database is checked directly |
| **Test data** | The concrete values. Several rows of data mean a data-driven test |
| **Expected result** | What must happen, including the message and the status code |
| **Risk** | The product risk this test case reduces (test plan, section 4.1), or – if none |
| **Priority** | High, Medium or Low, set from the risk (see below) |

Preconditions and steps come from the Given and When of the acceptance criterion and are not repeated. Detailed steps are written only for manual tests.

## Rules

- **Lowest level that can catch the defect** (test plan, section 5.1). Rules such as the password policy are tested with many values as unit tests. The API and UI tests then check one or two values to prove that the rule is wired in.
- **Every acceptance criterion has at least one positive and one negative test** (test plan, section 5.3). When an AC describes only a rejection (for example a password of 7 characters), its positive counterpart is the test of the neighboring AC (8 characters). The traceability matrix shows these pairs.
- **A positive control is on the same level as the negative test it controls** (UI with UI, API with API). Where possible it is a step inside the same test case. See the traceability matrix for the full explanation.
- **Priority comes from the product risk of the test case** (the Risk column), not from the AC. One AC can have test cases of different weight: returning to the requested page after sign-in is a convenience (R11), while blocking a return address to another site protects the account (R2).
  - High: risk score 6
  - Medium: risk score 4
  - Low: risk score 3 or lower
  - **Smoke tests are always High**, whatever their risk, because every other test depends on them.
  - **A test case without a risk from the table is Medium.**
- **Priority is never changed by hand.** If a test case seems more or less important than its priority, the risk rating in the test plan is reviewed instead.
- **Test data is synthetic.** Emails use the reserved domain `example.com` (or a subdomain of it), and other sites use the reserved top-level domain `.example` (for example `evil.example`). These names can never belong to a real person or company. Automated tests make every email unique (for example `ana+<guid>@example.com`), so tests can run in parallel. The tables show the readable form.
- **Status** of a test case (in the traceability matrix): Planned → Automated → Passing in CI. A test case that stays manual is marked Manual.
