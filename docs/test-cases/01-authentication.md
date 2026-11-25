# Test Cases: Authentication and Access Control

**User stories:** `docs/user-stories/01-authentication.md`
**Status:** Draft
**Related risks:** R2 (score 6), R6 (score 4), R11 (score 3)

---

## US-AUTH-01: Register an account

| ID | Title | Covers | Type | Technique | Level | Test data | Expected result | Risk | Priority |
|---|---|---|---|---|---|---|---|---|---|
| TC-AUTH-001 | Register through the form and land on the empty expense list | AC1 | + | – | UI | `ana@example.com`, `Lozinka123` twice | Signed in; empty expense list is shown. Smoke test | R6 | High (Smoke) |
| TC-AUTH-002 | Register through the API stores exactly one user | AC1 | + | – | API, DB | `ana@example.com`, `Lozinka123` | Success status; one row in the users table with the email in normalized form | R6 | Medium |
| TC-AUTH-003 | Register with an email that already exists | AC2 | − | – | API, DB | `ana@example.com` registered first, then again | Rejected with "An account with this email already exists."; still one user in the database | R2 | High |
| TC-AUTH-004 | Email is compared without case and surrounding spaces | AC3 | + / − | EP | API, DB | First register ` Ana@Example.COM ` → accepted and stored as `ana@example.com`; then register `ANA@EXAMPLE.COM`, `ana@example.com `, `ana@example.com` → rejected | Rejected variants give the message from TC-AUTH-003; one user in the database | R2 | High |
| TC-AUTH-005 | Email format validation | AC4 | + / − | EP, EG, BVA | Unit | Valid: `ana@example.com`, `ana.maric+test@mail.example.com`. Invalid: `ana@`, `ana.example.com`, `@example.com`, `ana@@example.com`, `ana@example` (no dot), a space inside, empty, spaces only. Length: 254 characters → valid, 255 → invalid | Valid values pass; each invalid value gives "Enter a valid email address." | R6 | Medium |
| TC-AUTH-006 | Invalid email is rejected by the API | AC4 | − | EP | API | `ana.example.com` | 400 with "Enter a valid email address." | R6 | Medium |
| TC-AUTH-007 | Password length boundaries | AC5, AC6 | + / − | BVA | Unit | 7 chars `abcdef1` → rejected; 8 chars `abcdefg1` → accepted; 128 chars → accepted; 129 chars → rejected | 7 characters: "Password must be at least 8 characters and contain a letter and a digit." 129 characters: "Password can have at most 128 characters." | R6 | Medium |
| TC-AUTH-008 | Password composition | AC7 | + / − | EP | Unit | `abcdefgh` (no digit) → rejected; `12345678` (no letter) → rejected; `abcdefg1` → accepted; `šćčđžab1` (local letters) → accepted | Same message as TC-AUTH-007 for rejected values | R6 | Medium |
| TC-AUTH-009 | Short password is rejected by the API | AC5 | − | BVA | API, DB | `abcdef1` | 400 with the password message; no user is created | R6 | Medium |
| TC-AUTH-010 | Password and confirmation do not match | AC8 | − | – | UI, DB | `Lozinka123` and `Lozinka124` | "Passwords do not match."; no user is created | R6 | Medium |
| TC-AUTH-011 | Validation messages are shown next to the fields on the form | AC4, AC5 | − | – | UI | Email `ana@`, password `abc` | Both messages are visible; the form stays open; nothing is sent until fixed | R6 | Medium |
| TC-AUTH-012 | Password is stored only as a hash and never returned | AC9 | + / − | – | Integration, DB | `Lozinka123` | The stored value is not `Lozinka123` and does not contain it; the registration response contains neither the password nor the hash; the user can still sign in | R2 | High |
| TC-AUTH-013 | New user gets exactly the predefined categories | AC10 | + / − | – | API | User A has renamed Food to Groceries and added Kindergarten; then user B registers | User B's list contains exactly Food, Fuel, Bills, Housing, Health, Clothing, Entertainment, Travel, Other; none of user A's changes | R2 | High |
| TC-AUTH-033 | Two registrations with the same email at the same moment | AC2 | − | EG | API, DB | Two parallel requests with `ana@example.com` | One succeeds, the other is rejected; exactly one user in the database | R2 | High |
| TC-AUTH-034 | Password is not trimmed | AC1 | + / − | EG | API | Register with ` Lozinka123 ` (spaces around it); sign in with ` Lozinka123 ` → signed in; sign in with `Lozinka123` → rejected | First sign-in succeeds; the second gets 401 with "Invalid email or password." Spaces are part of the password | R6 | Medium |

## US-AUTH-02: Sign in

| ID | Title | Covers | Type | Technique | Level | Test data | Expected result | Risk | Priority |
|---|---|---|---|---|---|---|---|---|---|
| TC-AUTH-014 | Sign in through the form | AC1 | + | – | UI | Registered `ana@example.com`, `Lozinka123` | Expense list is shown. Smoke test | R11 | High (Smoke) |
| TC-AUTH-015 | Sign in with email in different case or with spaces | AC2 | + / − | EP | API | ` ANA@example.com ` → signed in; `ana@example.co` (one character different) → rejected | First: signed in. Second: 401 with "Invalid email or password." Only case and surrounding spaces are ignored | R11 | Low |
| TC-AUTH-016 | Wrong password | AC3 | + / − | EP | API | `ana@example.com` with `Lozinka123` → signed in (control); with `Lozinka124` → rejected | Correct password: signed in. Wrong password: 401 with "Invalid email or password." | R2 | High |
| TC-AUTH-017 | Unknown email gets the same answer as a wrong password | AC4 | − | EG | API | `nobody@example.com` | Same status code and same message as TC-AUTH-016; the two responses cannot be told apart | R2 | High |
| TC-AUTH-018 | Lockout after the 5th failed attempt | AC5 | + / − | BVA, ST | Integration | User 1: 4 failed attempts, then the correct password → signed in. User 2: 5 failed attempts → locked | User 1 is signed in. User 2 gets "Too many failed attempts. Try again in 5 minutes." after the 5th failure | R2 | High |
| TC-AUTH-019 | Locked account stays locked until 5 minutes have passed | AC6, AC7 | + / − | BVA, ST | Integration | Fixed clock: correct password at 4:59 after locking → locked; at 5:00 → signed in | 4:59 gives the lockout message; 5:00 signs in | R2 | High |
| TC-AUTH-020 | Counter is reset after the lockout ends | AC7 | + | ST | Integration | After signing in at 5:00: 4 failed attempts | Not locked (the counter started from 0) | R2 | High |
| TC-AUTH-021 | Counter is reset after a successful sign-in | AC8 | + / − | ST | Integration | 3 failures, success, then 4 failures | Not locked; a 5th failure locks | R2 | High |
| TC-AUTH-036 | Wrong credentials on the sign-in form | AC1, AC3, AC4 | − | EP | UI | `ana@example.com` with `Lozinka124`; `nobody@example.com` with `Lozinka123` | "Invalid email or password." is shown for both; the sign-in page stays open; the expense list is not reachable | R11 | Low |
| TC-AUTH-022 | Return to the requested page after sign-in | AC9 | + | – | UI | Open `/expenses` while signed out | Sign-in page; after signing in, `/expenses` is shown | R11 | Low |
| TC-AUTH-023 | Return address to another site is ignored | AC9 | − | EG | UI | Return URL `https://evil.example`, `//evil.example`, `/\evil.example` | After signing in, the app's start page is shown; the browser never leaves the app | R2 | High |

## US-AUTH-03: Sign out

| ID | Title | Covers | Type | Technique | Level | Test data | Expected result | Risk | Priority |
|---|---|---|---|---|---|---|---|---|---|
| TC-AUTH-024 | Sign out | AC1 | + | ST | UI | Signed-in user | Sign-in page is shown | R11 | Low |
| TC-AUTH-025 | Back button and direct URL after sign-out | AC2 | + / − | EG | UI | Signed in: `/expenses` shows the user's expense (control). After sign-out: Back button; then open `/expenses` | Sign-in page both times; the expense is not visible | R11 | Low |
| TC-AUTH-026 | Old session is rejected by the API after sign-out | AC3 | + / − | ST | API | Request with the session before sign-out → 200 (control); same request with the same session after sign-out | Before sign-out: 200. After sign-out: 401 Unauthorized | R11 | Low |
| TC-AUTH-035 | Sign out when the session is no longer valid | AC1 | − | ST, EG | UI | Two tabs, same user: sign out in tab 1, then click Sign out in tab 2 | Sign-in page is shown in tab 2; no server error, no error message | R11 | Low |

## US-AUTH-04: Protect pages and API endpoints

| ID | Title | Covers | Type | Technique | Level | Test data | Expected result | Risk | Priority |
|---|---|---|---|---|---|---|---|---|---|
| TC-AUTH-027 | Protected pages need sign-in | AC1 | + / − | EP | UI | Every page except sign-in and registration (`/expenses`, `/accounts`, `/categories`, ...), signed in and signed out | Signed in: each page opens. Signed out: sign-in page for each | R11 | Low |
| TC-AUTH-028 | Every API endpoint requires a session | AC2 | + / − | EP | API | Every endpoint from the OpenAPI description, except health, registration and sign-in; first with a valid session (control), then without one | With a session: not 401. Without a session: 401 for each. New endpoints are included automatically | R2 | High |
| TC-AUTH-029 | Public endpoints work without a session | AC2 | + | EP | API | `/health`, registration, sign-in | Not 401 | R2 | High |
| TC-AUTH-030 | Lists return only the user's own data | AC3 | + / − | – | API | Users A and B, each with their own accounts, categories and expenses | A sees only A's items; B sees only B's | R2 | High |
| TC-AUTH-031 | Another user's expense cannot be read, changed or deleted | AC4 | + / − | DT | API, DB | User A calls GET, PUT and DELETE on A's own expense (control), then on user B's expense | Own expense: success. B's expense: 404 for each, and it is unchanged in the database | R2 | High |
| TC-AUTH-032 | An expense can use only the user's own account and category | AC5 | + / − | DT | API, DB | User A creates an expense with A's account and category → stored; with B's account → rejected; with B's category → rejected | Rejected requests store nothing | R2 | High |

**Notes:**
- TC-AUTH-033 to TC-AUTH-036 were added after the first review. They are listed under their story, so their numbers do not follow the order of the table (IDs are permanent).
- Several test cases start with a **positive control** step (TC-AUTH-016, 025, 026, 027, 028, 031): the same page or request first works for the owner or while signed in, then fails in the negative case. This proves the negative result comes from the rule under test, not from a broken request.
- TC-AUTH-018 to TC-AUTH-021 run in-process (Integration level), because the lockout clock is replaced by a fixed `TimeProvider`. A running API cannot have its clock changed from outside.
- TC-AUTH-028 reads the list of endpoints from the OpenAPI description (`/openapi/v1.json`). When a new endpoint is added and is not protected, this test fails without being changed.
- TC-AUTH-030 to TC-AUTH-032 are repeated for every new resource (accounts, categories, planned expenses, groups), as US-AUTH-04 says.
- TC-AUTH-010: the password confirmation is checked only in the UI. It is not sent to the API, so there is no API test for US-AUTH-01/AC8.
- TC-AUTH-017 compares status codes and messages. A difference in response time (the password hash is checked only when the email exists) can also reveal which emails have an account. Timing is not tested automatically, because such tests are unstable; it is looked at in an exploratory session.
- **Dependencies on later features.** Some test cases need data from features that are built after authentication:
  - TC-AUTH-013 (user A's changed categories) needs categories (US-CAT).
  - TC-AUTH-030 to TC-AUTH-032 need accounts, categories and expenses.
  - TC-AUTH-025 and TC-AUTH-027 need the expense list and the other pages.
  - TC-AUTH-001, TC-AUTH-014, TC-AUTH-022 and TC-AUTH-036 use the expense list as the page shown after sign-in. Until the expense list exists, a simple placeholder page at `/expenses` is used, so these tests do not change later.

  With authentication, the parts that are already possible are automated. The rest stays Planned in the traceability matrix and is completed with the feature it depends on. Authentication is not marked Tested until then.
- TC-AUTH-033: a duplicate check in code does not stop two requests that arrive at the same moment (race condition). Only a unique index on the normalized email in the database does.
- TC-AUTH-027 is Low because R11 (score 3) covers exactly this case. If it should be more important, the likelihood of R11 is raised in the test plan; the priority is not changed by hand.
