# Authentication and Access Control

**Phase:** 1a
**Status:** Reviewed
**Related risks:** R2 (data of another user), R11 (session problems), R6 (validation gaps)

---

### US-AUTH-01: Register an account

**As a** new user
**I want** to create an account with my email address and a password
**so that** my expenses are stored privately under my own account

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am not signed in | I register with a valid email and a valid password, and confirm the password | My account is created, I am signed in and I see my (empty) expense list |
| AC2 | An account with `ana@example.com` already exists | I register with `ana@example.com` | Registration fails with the message "An account with this email already exists." and no second account is created |
| AC3 | An account with `ana@example.com` already exists | I register with ` Ana@Example.COM ` (different case, spaces around it) | It is treated as the same email: registration fails as in AC2 |
| AC4 | I am on the registration form | I enter an email in an invalid format (for example `ana@`, `ana.example.com`, empty) | Registration fails with "Enter a valid email address." |
| AC5 | I am on the registration form | I enter a password with 7 characters | Registration fails with "Password must be at least 8 characters and contain a letter and a digit." |
| AC6 | I am on the registration form | I enter a password with exactly 8 characters that contains a letter and a digit | The password is accepted |
| AC7 | I am on the registration form | I enter a password of 8 or more characters without a digit, or without a letter | Registration fails with the same message as in AC5 |
| AC8 | I am on the registration form | The password and the confirmation do not match | Registration fails with "Passwords do not match." |
| AC9 | I have registered | The account is stored | The password is not stored in plain text; only a hash is stored (checked in the database) |
| AC10 | I have just registered | I open the category list | The predefined categories are available (Food, Fuel, Bills, Housing, Health, Clothing, Entertainment, Travel, Other) |

**Risks:** R6, R2
**Notes:**
- Email is the user name. It is compared case-insensitively and trimmed. Maximum length is 254 characters (the limit of an email address in RFC 5321); a longer email gets the message from AC4.
- Password rule: at least 8 characters, at least one letter and one digit. Any letter counts, including local letters (š, ć, č, đ, ž); only 0–9 count as digits. Maximum length is 128 characters, with the message "Password can have at most 128 characters."
- The password is not trimmed: spaces at the start or the end are part of the password. Only the email is trimmed.
- The message in AC2 tells that the email is already registered. This makes it possible to check whether an email has an account (account enumeration). For a personal app this is accepted; sign-in (US-AUTH-02) does not reveal it.
- Out of scope for Phase 1: email confirmation, password reset by email, two-factor authentication, sign-in with Google or other providers, deleting an account.

---

### US-AUTH-02: Sign in

**As a** registered user
**I want** to sign in with my email and password
**so that** I can see and manage my expenses

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have an account | I sign in with the correct email and password | I am signed in and see my expense list |
| AC2 | I have an account | I sign in with my email in a different case or with spaces around it | I am signed in (same as AC1) |
| AC3 | I have an account | I sign in with a wrong password | Sign-in fails with "Invalid email or password." |
| AC4 | No account exists for `nobody@example.com` | I sign in with `nobody@example.com` | Sign-in fails with the same message as in AC3, so it is not revealed whether the email has an account |
| AC5 | I have failed to sign in 4 times in a row | I fail for the 5th time | My account is locked for 5 minutes, with the message "Too many failed attempts. Try again in 5 minutes." |
| AC6 | My account is locked | I sign in with the correct password before 5 minutes have passed | Sign-in fails with the lockout message |
| AC7 | My account was locked | 5 minutes have passed and I sign in with the correct password | I am signed in and the failed attempt counter is reset |
| AC8 | I have failed to sign in 3 times | I sign in successfully | The failed attempt counter is reset to 0 |
| AC9 | I tried to open a protected page (for example the expense list) without being signed in and was sent to sign-in | I sign in | I am taken back to the page I tried to open |

**Risks:** R11, R2
**Notes:**
- Lockout tests use a controlled clock (`TimeProvider`), so they do not wait 5 real minutes. Boundaries: 4 and 5 failed attempts; 4:59 and 5:00 minutes after locking.
- AC9 only returns to pages inside the app. A return address pointing to another site is ignored (protection against open redirect).

---

### US-AUTH-03: Sign out

**As a** signed-in user
**I want** to sign out
**so that** nobody else using the same browser can see my expenses

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am signed in | I sign out | I am taken to the sign-in page |
| AC2 | I have signed out | I press the browser's Back button or open the expense list URL directly | I see the sign-in page, not my data |
| AC3 | I have signed out | A request is sent to the API with the session I had before signing out | The API rejects it with 401 Unauthorized |

**Risks:** R11
**Notes:**
- AC3 is checked at the API level. It ensures that signing out ends the session on the server too, not only in the browser.

---

### US-AUTH-04: Protect pages and API endpoints

**As a** user
**I want** my data to be reachable only by me after I sign in
**so that** nobody else can see or change my accounts and expenses

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am not signed in | I open any page except sign-in and registration | I am redirected to the sign-in page |
| AC2 | No session is sent | A request is sent to any API endpoint except health, registration and sign-in | The API responds with 401 Unauthorized |
| AC3 | Users A and B each have their own data | User A requests a list (accounts, categories, expenses) | Only user A's data is returned |
| AC4 | User B has an expense with ID `x` | User A requests, edits or deletes the expense `x` through the API | The API responds with 404 Not Found and the expense is unchanged |
| AC5 | User B has an expense | User A creates an expense that references user B's account or category | The request is rejected and nothing is stored |

**Risks:** R2 (score 6)
**Notes:**
- AC4 uses 404, not 403. A 403 would confirm that the resource exists and belongs to someone else.
- AC3–AC5 apply to every resource added later (accounts, categories, expenses, planned expenses, groups). Each new area gets the same isolation tests with two users.
- The health endpoint stays public, because CI and monitoring use it before anyone signs in.
