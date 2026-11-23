# User Stories

User stories describe what the app must do from the user's point of view. Each story has acceptance criteria (ACs) that define when the story is done. The ACs are the basis for the test cases and the traceability matrix (`docs/test-cases/`).

## Files

One file per feature area:

| File | Area | Phase | Status |
|---|---|---|---|
| [01-authentication.md](01-authentication.md) | Registration, sign-in, sign-out, access control | 1a | Reviewed |
| [02-accounts.md](02-accounts.md) | Bank and online accounts, IBAN validation, bank recognition, cards | 1a | Reviewed |
| [03-categories.md](03-categories.md) | Predefined and custom categories | 1a | Reviewed |
| [04-expenses.md](04-expenses.md) | Expenses, payment method, list, filters, validation | 1a | Reviewed |

## Story format

```
### US-AREA-NN: Short title

**As a** <type of user>
**I want** <goal>
**so that** <reason>

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | ... | ... | ... |

**Risks:** R2, R11 (from docs/test-plan.md, section 4)
**Notes:** decisions, open questions, what is out of scope
```

## Rules

- **IDs are permanent.** A story is `US-<AREA>-<NN>` (for example `US-AUTH-01`), and an acceptance criterion is referenced as `US-AUTH-01/AC3`. Tests, test cases and bug reports refer to these IDs, so they are never renumbered. A removed AC is marked as removed instead of deleted.
- **One behavior per AC.** Each AC can be checked with a clear pass or fail result.
- **Concrete values.** ACs use real examples ("7 characters", "5 failed attempts"), not vague words ("short", "several").
- **Behavior, not implementation.** ACs describe what the user or an API client observes, not how the code works. Technical decisions go into Notes.
- **Every AC has at least one positive and one negative test** (test plan, section 5.3).
- **Status:** Draft → Reviewed → Implemented → Tested.
