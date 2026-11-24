# Traceability Matrix

Links every acceptance criterion to the test cases that check it, the automated tests that run them, and the product risks it reduces. The Risks column lists the risks of the test cases written for that AC; positive controls borrowed from another AC are not counted. The priority of each test case comes from its own risk (README). It answers two questions:

- **Is every acceptance criterion tested?** Each row needs at least one positive and one negative test case.
- **What is affected when something changes?** A changed AC shows which test cases and automated tests must be updated.

The **Automated test** column gets the test method name when the test is written (for example `RegisterTests.Duplicate_email_is_rejected`). **Status** follows the README: Planned → Automated → Passing in CI, or Manual.

**The control is on the same level as the test it controls.** A UI negative test is paired with a UI positive test, and an API negative test with an API positive test. A row with several levels has a pair on each level.

A positive test case listed for a negative AC is a **positive control**: it shows that the same request or page works in the valid case, so the negative test cannot pass for the wrong reason (for example a request that was never authorized in the first place). Where possible, the control is a step inside the same test case (marked `+ / −`), as in TC-AUTH-026: the same request returns 200 before sign-out and 401 after it.

## Coverage summary

| Area | ACs | ACs with + and − test cases | Automated | Status |
|---|---|---|---|---|
| Authentication | 27 | 27 | 0 | Planned |
| Accounts | 44 | – | – | Test cases not written yet |
| Categories | 21 | – | – | Test cases not written yet |
| Expenses | 55 | – | – | Test cases not written yet |

## Authentication

| AC | Risks | Positive test cases | Negative test cases | Levels | Automated test | Status |
|---|---|---|---|---|---|---|
| US-AUTH-01/AC1 | R6 | TC-AUTH-001, TC-AUTH-002, TC-AUTH-034 | TC-AUTH-003, TC-AUTH-010, TC-AUTH-034 | UI, API, DB | – | Planned |
| US-AUTH-01/AC2 | R2 | TC-AUTH-002 | TC-AUTH-003, TC-AUTH-033 | API, DB | – | Planned |
| US-AUTH-01/AC3 | R2 | TC-AUTH-004 (first registration stored normalized) | TC-AUTH-004 | API, DB | – | Planned |
| US-AUTH-01/AC4 | R6 | TC-AUTH-005 (valid emails), TC-AUTH-002 (API), TC-AUTH-001 (UI) | TC-AUTH-005, TC-AUTH-006, TC-AUTH-011 | Unit, API, UI | – | Planned |
| US-AUTH-01/AC5 | R6 | TC-AUTH-007 (8 characters), TC-AUTH-002 (API), TC-AUTH-001 (UI) | TC-AUTH-007, TC-AUTH-009, TC-AUTH-011 | Unit, API, UI | – | Planned |
| US-AUTH-01/AC6 | R6 | TC-AUTH-007 | TC-AUTH-007 (7 and 129 characters) | Unit | – | Planned |
| US-AUTH-01/AC7 | R6 | TC-AUTH-008 | TC-AUTH-008 | Unit | – | Planned |
| US-AUTH-01/AC8 | R6 | TC-AUTH-001 (matching confirmation) | TC-AUTH-010 | UI, DB (confirmation is not sent to the API) | – | Planned |
| US-AUTH-01/AC9 | R2 | TC-AUTH-012 (user can sign in) | TC-AUTH-012 (plain password not stored, password and hash not returned) | Integration, DB | – | Planned |
| US-AUTH-01/AC10 | R2 | TC-AUTH-013 | TC-AUTH-013 (no changes of another user copied) | API | – | Planned |
| US-AUTH-02/AC1 | R11 | TC-AUTH-014 (UI), TC-AUTH-016 (API control) | TC-AUTH-036 (UI), TC-AUTH-016 (API) | UI, API | – | Planned |
| US-AUTH-02/AC2 | R11 | TC-AUTH-015 | TC-AUTH-015 (one character different) | API | – | Planned |
| US-AUTH-02/AC3 | R2, R11 | TC-AUTH-016 (control: correct password), TC-AUTH-014 (UI) | TC-AUTH-016, TC-AUTH-036 (UI) | API, UI | – | Planned |
| US-AUTH-02/AC4 | R2, R11 | TC-AUTH-016 (control: known email), TC-AUTH-014 (UI) | TC-AUTH-017, TC-AUTH-036 (UI) | API, UI | – | Planned |
| US-AUTH-02/AC5 | R2 | TC-AUTH-018 (4 failures, not locked) | TC-AUTH-018 (5 failures, locked) | Integration | – | Planned |
| US-AUTH-02/AC6 | R2 | TC-AUTH-019 (5:00) | TC-AUTH-019 (4:59) | Integration | – | Planned |
| US-AUTH-02/AC7 | R2 | TC-AUTH-019, TC-AUTH-020 | TC-AUTH-019 (4:59) | Integration | – | Planned |
| US-AUTH-02/AC8 | R2 | TC-AUTH-021 | TC-AUTH-021 (5th failure locks) | Integration | – | Planned |
| US-AUTH-02/AC9 | R11, R2 | TC-AUTH-022 | TC-AUTH-023 | UI | – | Planned |
| US-AUTH-03/AC1 | R11 | TC-AUTH-024 | TC-AUTH-035 | UI | – | Planned |
| US-AUTH-03/AC2 | R11 | TC-AUTH-025 (control: data visible before sign-out) | TC-AUTH-025 | UI | – | Planned |
| US-AUTH-03/AC3 | R11 | TC-AUTH-026 (control: 200 before sign-out) | TC-AUTH-026 | API | – | Planned |
| US-AUTH-04/AC1 | R11 | TC-AUTH-027 (signed in, every page opens) | TC-AUTH-027 | UI | – | Planned |
| US-AUTH-04/AC2 | R2 | TC-AUTH-028 (control: with a session), TC-AUTH-029 | TC-AUTH-028 | API | – | Planned |
| US-AUTH-04/AC3 | R2 | TC-AUTH-030 | TC-AUTH-030 | API | – | Planned |
| US-AUTH-04/AC4 | R2 | TC-AUTH-031 (control: own expense) | TC-AUTH-031 | API, DB | – | Planned |
| US-AUTH-04/AC5 | R2 | TC-AUTH-032 (own account and category) | TC-AUTH-032 | API, DB | – | Planned |
