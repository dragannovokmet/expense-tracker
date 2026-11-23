# Expenses

**Phase:** 1a
**Status:** Reviewed
**Related risks:** R1 (money), R2 (data of another user), R3 (dates), R6 (validation gaps), R8 (list results), R13 (currencies), R17 (double submit)

---

### US-EXP-01: Add an expense

**As a** user
**I want** to record an expense with its amount, date and category
**so that** I know where my money goes

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am on the new expense form | I open it | Date and time are set to now (in my time zone), currency to KM, payment method to Card |
| AC2 | I am on the new expense form | I enter amount `35.58`, a category, and save | The expense is saved with exactly 35.58, and appears at the top of my expense list |
| AC3 | I am on the new expense form | I also fill in merchant, description, note, payment method and account | All values are saved and shown in the expense details |
| AC4 | I am on the new expense form | I enter amount `0` or a negative amount | Saving fails with "Amount must be greater than 0." |
| AC5 | I am on the new expense form | I enter `0.01` | It is accepted (smallest allowed amount) |
| AC6 | I am on the new expense form | I enter `999999.99` | It is accepted (largest allowed amount) |
| AC7 | I am on the new expense form | I enter `1000000.00` | Saving fails with "Amount can be at most 999,999.99." |
| AC8 | – | An API request creates or updates an expense with amount `10.555` | It is rejected with status 400 and "Amount can have at most 2 decimals." Nothing is saved; the amount is never rounded or truncated |
| AC9 | It is 23:59 on 30 November in my time zone | I save an expense dated 30 November | It is accepted |
| AC10 | It is 23:59 on 30 November in my time zone | I save an expense dated 1 December | Saving fails with "Date cannot be in the future." |
| AC11 | I am on the new expense form | I do not choose a category | Saving fails with "Category is required." |
| AC12 | I am on the new expense form | I enter a merchant or description longer than 100 characters, or a note longer than 1000 characters | Saving fails with a message about the maximum length of that field |
| AC13 | I am on the new expense form | I click Save twice quickly | Only one expense is created |
| AC14 | I am on the new expense form | I choose currency EUR and amount 15 | The expense is saved as 15 EUR, not converted to KM |
| AC15 | I am on the new expense form | I enter the amount with a decimal comma (`35,58`) or a decimal point (`35.58`) | Both are accepted and saved as 35.58 |
| AC16 | I am on the new expense form | I try to type a third decimal (`10.55` then `9`) | The field does not accept the third decimal; `10.55` stays visible |
| AC17 | I am on the new expense form | I paste `10.555` into the amount field | The form shows "Amount can have at most 2 decimals." and does not save until the amount is corrected |
| AC18 | I am on the new expense form | I enter `1.234` or `1,234` (meant as a thousands separator) | It is rejected with "Amount can have at most 2 decimals." It is never saved as 1234 or as 1.23 |

**Risks:** R1, R3, R6, R13, R17
**Notes:**
- Money is stored as `decimal` with 2 decimals.
- AC8, AC16 and AC17: the amount is never truncated or rounded silently. Truncating `10.555` to `10.55` would hide a typo (the user may have meant `105.55`). The UI stops a third decimal from being typed (AC16), and the API still rejects it (AC8), because the API can be called without the UI (Postman, tests, the Android app). AC17 covers pasting, which a typing mask often misses.
- AC15: in BiH the decimal separator is a comma, so both `35,58` and `35.58` are accepted. The amount is stored as a number (`decimal`), without any separator.
- AC18: a thousands separator is not supported in Phase 1. `1.234` means one thousand two hundred thirty-four in BiH, but one point two three four in English. Both `1.234` and `1,234` have three decimals, so the AC8 rule rejects them and the amount is never misread. The user enters `1234,56` or `1234.56`.
- Localization (Phase 5) changes only how amounts are displayed and read: `35,58` for Serbian, Bosnian and Croatian, `35.58` for English. The format follows the chosen language and region (en-US `1,234.56`, de-DE `1.234,56`, fr-FR `1 234,56`), not the currency. The stored value never changes, so reports, the API and CSV export always work with the same number.
- "Now" and "today" are based on the user's time zone, through `TimeProvider` (test plan, section 6). The server and CI run in UTC.
- **Time zone in Phase 1a:** every user's time zone is `Europe/Sarajevo` (UTC+1 in winter, UTC+2 in summer). Choosing a time zone in the user profile comes later, together with localization. Because the server runs in UTC, AC9 and AC10 are also tested on the days when daylight saving time starts and ends (last Sunday in March and in October), and just after midnight local time (00:30 on 1 December is still 30 November in UTC), when the local and the UTC date differ.
- In Phase 1 every manually entered expense gets source *manual* and status *confirmed*. Other sources and statuses come with Phase 4.
- Currencies in Phase 1: KM (default), EUR, USD, CAD. Card payments in a foreign currency are entered in that currency; the booked amount in KM is added from the bank statement in Phase 4.

---

### US-EXP-02: Payment method and account

**As a** user
**I want** to record how I paid
**so that** I can see what was paid in cash and what was charged to which account

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | Payment method is Cash | I look at the account field | It is empty and disabled; a cash expense has no account |
| AC2 | Payment method is Card or Mobile wallet | I choose an account (bank or online) | I can also choose one of the cards of that account (optional) |
| AC3 | Payment method is Card | I save without choosing an account or a card | It is accepted; account and card are optional, for a quick entry ("paid by card") |
| AC4 | Payment method is Online account, and I paid from the balance (for example AirCash) | I choose the online account | It is accepted; the expense is charged to the online account |
| AC5 | I chose an account and a card, then change the payment method to Cash | I save | Account and card are cleared |
| AC6 | An account is archived | I open the account list on the form | The archived account is not offered (see US-ACC-04/AC7) |
| AC7 | Payment method is Online account, and PayPal only passed the payment to my card | I choose my bank account and its card | It is accepted; the expense is charged to the bank account and the card |
| AC8 | Payment method is Bank transfer | I look at the account and card fields | Only bank accounts are offered, and no card can be chosen |
| AC9 | Payment method is Other | I choose an account and a card | It is accepted; both are optional |
| AC10 | Payment method is Card, Mobile wallet or Online account | I choose a card first, without choosing an account | The account of that card is set automatically, because a card belongs to exactly one account (US-ACC-03/AC8) |
| AC11 | – | An API request saves an expense with a card that does not belong to the chosen account | It is rejected with status 400 and "The card does not belong to the selected account." |
| AC12 | I save an expense paid by card without choosing an account or a card | I open it later, or edit another field and save | It still has no account and no card; the app never assigns an account by guessing (for example "the current account") |

**Risks:** R6
**Notes:**
- Payment methods: Cash, Card, Mobile wallet, Online account, Bank transfer, Other. The payment method says *how* I paid; the account says *where the money came from*. PayPal, AirCash and similar services are accounts (US-ACC-05), not separate payment methods.
- **Mobile wallet** covers Google Wallet, Apple Pay and Samsung Wallet, on a phone (NFC) or on a smartwatch connected to the wallet. Behind the wallet is a real card, so the expense is charged to that card and its account.
- AC4 and AC7: an expense is always charged to the place the money actually left at the moment of payment. Paying from the PayPal balance charges the PayPal account. When PayPal has no balance and charges my card, the expense is charged to the bank account and the card; PayPal is only an intermediary (it can be written in the merchant or note).
- "Card" means any card of mine: debit, credit, charge or prepaid, physical or virtual. The card type does not change the rules on this form.
- AC3 and AC10: account and card are optional, but never inconsistent. If a card is chosen, its account is always known, because every card belongs to one account. If neither is chosen, the expense is recorded only as "paid by card". This allows a quick entry, and recording expenses before any accounts are added.
- AC12: an expense without an account is counted correctly everywhere; only the information *which* account is missing. An account is set only when I choose it, when I choose a card (AC10), or, from Phase 4, when I confirm a match with a bank statement line.
- AC1–AC12 form a decision table: payment method × account type (none, bank, online) × card chosen.

---

### US-EXP-03: Edit and delete an expense

**As a** user
**I want** to correct or remove an expense
**so that** my records stay accurate

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have an expense | I change its amount from 35.58 to 36.58 and save | The new amount is shown in the list and stored in the database |
| AC2 | I edit an expense | I enter invalid values | The same validation rules as in US-EXP-01 apply |
| AC3 | I have an expense | I delete it | I am asked to confirm; after confirming, the expense is removed from the list and from the database |
| AC4 | I am asked to confirm a delete | I cancel | The expense stays unchanged |
| AC5 | An expense is paid from an account that is now archived | I edit the expense (for example its amount) and save | The archived account stays on the expense and saving works (same rule as US-CAT-03/AC4 for categories) |
| AC6 | The same expense is open in two tabs | I change the amount in the first tab and save, then change the description in the second tab and save | The second save is rejected with "This expense was changed in the meantime. Reload it." (status 409); the change from the first tab is kept |
| AC7 | An expense was deleted in another tab | I try to delete it again | I see "This expense no longer exists." and the list is refreshed; there is no server error |

**Risks:** R1, R6, R2
**Notes:**
- Editing or deleting another user's expense returns 404 (US-AUTH-04/AC4). It is listed here because of risk R2.
- AC5: a common bug is a form that does not find the archived account in its list and clears it silently on save.
- AC6: without this rule the second save silently overwrites the first change (a *lost update*). It is implemented with a row version column (optimistic concurrency).
- After the last expense of a category or an account is deleted, the category or account can be deleted, not only archived (US-CAT-03/AC6, US-ACC-04/AC5). This is a good end-to-end test across three stories.
- **Open question (Phase 4):** can the amount of an expense confirmed by a bank statement still be changed? Decided in Phase 4.

---

### US-EXP-04: Expense list with filters, search, sorting and paging

**As a** user
**I want** to find expenses quickly
**so that** I can check what I spent, when and where

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have expenses | I open the expense list | Expenses are sorted by date, newest first, 20 per page |
| AC2 | I have 20 expenses | I open the list | All 20 are on one page and there is no next page |
| AC3 | I have 21 expenses | I open the list | Page 1 has 20 expenses and page 2 has 1 |
| AC4 | I have expenses | I change the page size to 50 or 100 | The list shows that many per page |
| AC5 | I have expenses in several months | I filter by a period (from–to) | Only expenses inside the period are shown, including those on the first and the last day |
| AC6 | I have expenses in several categories | I filter by one or more categories | Only expenses in those categories are shown |
| AC7 | I have expenses with different payment methods | I filter by payment method | Only expenses with that method are shown |
| AC8 | I have expenses with different amounts | I filter by amount from 10 to 50 | Expenses of exactly 10 and exactly 50 are included |
| AC9 | I have expenses | I search for `bingo` | Expenses with "Bingo" in the merchant, description or note are shown (case-insensitive) |
| AC10 | I have an expense with description "Čišćenje" | I search for `cisc`, `čišć` or `CISC` | The expense is found in all three cases: search ignores diacritics (č, ć, š, đ, ž) and letter case |
| AC11 | I have expenses | I sort by amount (ascending or descending) or by date | The order is correct; expenses with the same value are always in the same order |
| AC12 | I combine filters (period + category + payment method + search) | I apply them | Only expenses that match all filters are shown |
| AC13 | No expenses match the filters | I apply them | I see "No expenses found." and can clear the filters |
| AC14 | Filtered expenses are in KM and EUR | I look at the total below the list | There is a total per currency (for example "KM 250.00 · EUR 15.00"), never one mixed total |
| AC15 | Some of my expenses have no account | I filter by account and choose "No account" | Only expenses without an account are shown, so I can complete them |
| AC16 | I have expenses paid from several accounts | I filter by one or more accounts | Only expenses paid from those accounts are shown |
| AC17 | I am on page 3 of the list | I change a filter, the search or the sorting | The list goes back to page 1, so I never see an empty page or miss results |
| AC18 | User B has expenses that match my filters | I (user A) open the list or search | Only my own expenses are shown, and totals include only my expenses |

**Risks:** R8, R13, R2
**Notes:**
- Filter combinations are tested with pairwise selection (test plan, section 5.2).
- AC15: reports by account (Phase 1b) show these expenses in their own "No account" group; this is repeated as an AC in the report stories.
- AC11: with the same date or amount, a second sort key (creation order) keeps the order stable, so paging never shows the same expense twice or skips one.
- AC10: search without diacritics finds words with diacritics, because on a phone it is easier to type without them. `đ` is matched by both `d` and `dj` (`Djurdjevak` and `Đurđevak`). A plain SQL `LIKE` does not do this by itself, so this needs its own tests.
