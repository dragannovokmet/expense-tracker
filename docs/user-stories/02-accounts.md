# Accounts and Cards

**Phase:** 1a
**Status:** Reviewed
**Related risks:** R4 (account number validation), R5 (bank recognition), R7 (deleting or archiving), R2 (data of another user)

---

### US-ACC-01: Add a bank account

**As a** user
**I want** to add my bank accounts with their account numbers
**so that** I can link expenses to the account they were paid from

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am on the new account form | I open it | Country is set to Bosnia and Herzegovina and currency to KM by default |
| AC2 | I am on the new account form | I enter a name and a valid 16-digit BiH account number and save | The account is saved and appears in my account list |
| AC3 | I am on the new account form | I enter a valid IBAN of another country (for example a German `DE` IBAN) | Country changes automatically to that country (Germany) |
| AC4 | I am on the new account form | I enter a valid BiH IBAN (`BA` followed by 18 digits) | The account is saved; country stays Bosnia and Herzegovina |
| AC5 | I am on the new account form | I enter an account number with wrong check digits | Saving fails with "Account number is not valid (check digits do not match)." |
| AC6 | I am on the new account form | I enter an IBAN with the wrong length for its country (one character too short or too long) | Saving fails with "IBAN has the wrong length for <country>." |
| AC7 | I am on the new account form | I enter an IBAN with a country code that does not exist in the IBAN registry (for example `XX`) | Saving fails with "Unknown IBAN country code." |
| AC8 | I am on the new account form | I enter the number with spaces or lower-case letters (`ba39 3380 …`) | It is accepted and stored in a normalized form (no spaces, upper case) |
| AC9 | I already have an account with this number | I add the same number again | Saving fails with "You already have an account with this number." |
| AC10 | I am on the new account form | I leave the name empty, or enter a name longer than 50 characters | Saving fails with a validation message for the name |

**Risks:** R4, R2
**Notes:**
- IBAN rules (length and position of the bank code per country) come from one source, the SWIFT IBAN Registry, for about 80 countries. Check digits are verified with mod 97 (ISO 7064).
- The domestic BiH format has 16 digits and its own check digits. A BiH IBAN is `BA39` followed by the same 16 digits.
- Test data uses invented account numbers with correct check digits and real bank codes from the public catalog (test plan, section 7).
- Supported account currencies in Phase 1: KM (main currency and default), EUR, USD, CAD. More can be added later.
- This story covers bank accounts. Online accounts (PayPal, AirCash...) are added with US-ACC-05.

---

### US-ACC-02: Recognize the bank from the account number

**As a** user
**I want** the bank to be recognized from my BiH account number
**so that** I do not have to choose or type it myself

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | The first three digits of a BiH account number are a bank code from the catalog | I enter the account number (domestic format or IBAN) | The bank name is filled in automatically and cannot be edited |
| AC2 | The first three digits are not in the catalog | I enter the account number | The bank field stays empty and I can type the bank name myself |
| AC3 | The account is from another country | I enter its IBAN | The bank is not recognized automatically; I can type the bank name myself |
| AC4 | I typed a bank name that is not in the catalog | I add another account | My bank name is offered for selection |
| AC5 | User A typed a bank name that is not in the catalog | User B adds an account | User A's bank name is not offered to user B and is not added to the global catalog |

**Risks:** R5, R2
**Notes:**
- The catalog is built from the official list of the Central Bank of Bosnia and Herzegovina. It covers banks from both entities, because each bank has its own code.
- The catalog is organized by country, so adding another country later is one new seed file.
- Data-driven tests go over every code in the catalog, plus unknown codes.

---

### US-ACC-03: Add cards to an account

**As a** user
**I want** to add the cards linked to an account
**so that** I can record which card I paid with

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have an account | I add a card with a name (for example "Visa Debit") and the last 4 digits | The card is saved under that account |
| AC2 | I am adding a card | I enter 3 or 5 digits, or letters, as the last 4 digits | Saving fails with "Enter the last 4 digits of the card." |
| AC3 | I am adding a card | I try to enter a full card number | It is not possible: the field accepts only 4 digits |
| AC4 | I am adding a card | I open the form | Card type is set to Debit; I can change it to Credit, Charge or Prepaid |
| AC5 | I have a card of type Credit | I open the account details | The card is shown with its type ("Shop & Fun Sparkasse · Credit · ****4821") |
| AC6 | I have an online account (US-ACC-05), for example AirCash | I add a card to it | The card is saved under the online account, the same way as under a bank account |
| AC7 | I am adding a card | I mark it as virtual | The card is saved as virtual and shown with a "Virtual" label |
| AC8 | A card is saved under one account | I look for a way to link it to a second account | It is not possible: a card belongs to exactly one account. One account can have several cards |

**Risks:** R6
**Notes:**
- The app never stores full card numbers, only a name and the last 4 digits. This avoids storing sensitive card data at all.
- Card types:
  - **Debit:** spends money from the account directly.
  - **Credit:** a credit limit, repaid from the account in installments or as revolving debt. In BiH, "shopping" and "revolving" cards are credit cards. One card often works in all three ways: installments, deferred payment, revolving.
  - **Charge:** deferred payment; the full debt is taken from the account once a month.
  - **Prepaid:** spends money loaded onto the card in advance.
- A virtual card is a card without a physical plastic (for example an AirCash or Revolut virtual card), used for online payments. "Virtual" is a yes/no flag, separate from the card type, because a virtual card can be debit or prepaid.
- In Phase 1 the card type is only a label, with no logic. A credit card is added under the current account it is repaid from.
- A purchase with a credit card is recorded once, as an expense with the full amount on the purchase date. Installments and debt repayments are not new expenses. How they are recognized on bank statements is planned for Phase 4 (PLAN.md).

---

### US-ACC-04: View, edit, archive and delete accounts

**As a** user
**I want** to manage my accounts
**so that** my list stays up to date when I close or change an account

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have accounts | I open the account list | I see name, bank, currency and the account number masked except for the last 4 digits (`****1234`) |
| AC2 | I have an account | I open its details | I can reveal the full account number |
| AC3 | I have an account | I change its name and save | The new name is shown everywhere, including on existing expenses |
| AC4 | An account has 30 expenses | I change its account number | I am asked to confirm: "This account has 30 expenses. The new number applies to all of them." After confirming, the number is changed and the expenses stay linked to the account |
| AC5 | An account has no expenses | I delete it | The account is deleted |
| AC6 | An account has expenses | I try to delete it | Deleting is not allowed; I am offered to archive it instead |
| AC7 | An account is archived | I add a new expense | The archived account is not offered for selection |
| AC8 | An account is archived | I open old expenses and reports | Expenses paid from it still show the account and are counted in reports |
| AC9 | An account is archived | I restore it | It is offered again for new expenses |
| AC10 | An account has expenses | I change its account number to an invalid number, or to the number of another of my accounts | Saving fails with the same messages as in US-ACC-01 (AC5–AC7, AC9) |
| AC11 | An account has expenses | I change its number to a number of a different bank | The bank is recognized again from the new number (US-ACC-02), and reports by bank show the expenses under the new bank |
| AC12 | I cancel the confirmation from AC4 | – | The account number stays unchanged |
| AC13 | An account in KM has expenses | I try to change its currency to EUR | It is not allowed: "Currency cannot be changed for an account with expenses." While the account has no expenses, the currency can be changed |
| AC14 | I have no accounts | I open the account list | I see "No accounts yet." and an Add account button |
| AC15 | I have an online account | I open the account list | It is shown with its provider instead of a bank, and without an account number |

**Risks:** R7 (score 6), R2
**Notes:**
- Account states: active ⇄ archived. Deleting is possible only from either state while the account has no expenses. This is tested with state transition testing.
- AC4, AC10–AC12: the usual reason to change the number is a typo found after expenses were already entered. All validation rules from US-ACC-01 apply again. AC10–AC12 were added after AC9, so their numbers follow AC9 (IDs are permanent).
- AC13: a wrong account number is usually a typo, so it can be corrected (AC4). A different currency means a different account in reality (a foreign currency account has its own number or sub-account), so it is added as a new account instead. In Phase 4 the account currency is also what the booked amount from the statement is in.
- From Phase 4 the account number is locked once a bank statement has been imported for the account, because the number is then used to match statement lines to the account. Changing it would cause duplicates or unmatched lines on the next import. This becomes a new AC in Phase 4.

---

### US-ACC-05: Add an online account

**As a** user
**I want** to add my online accounts, such as PayPal or AirCash
**so that** I can record expenses paid from their balance

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am on the new account form | I open it | Account type is set to Bank account; I can change it to Online account |
| AC2 | Account type is Online account | I look at the form | There is no account number, IBAN, country or bank field; there is a provider field |
| AC3 | Account type is Online account | I choose a provider from the list (PayPal, AirCash, Revolut, Wise, Skrill, Payoneer), enter a name and a currency, and save | The account is saved and appears in my account list |
| AC4 | Account type is Online account | The provider is not in the list | I can type the provider name myself; it is offered only to me afterwards (same rule as US-ACC-02/AC4–AC5) |
| AC5 | Account type is Online account | I leave the provider empty | Saving fails with "Provider is required." |
| AC6 | I have an online account | I edit, archive, restore or delete it | The same rules as for bank accounts apply (US-ACC-04), except the rules about the account number |

**Risks:** R6, R7, R2
**Notes:**
- An online account holds money that was moved there (for example 100 KM from the current account to AirCash). An expense paid from its balance is charged to the online account, not to the bank account the money came from.
- Moving money from a bank account to an online account is a transfer, not an expense (PLAN.md, Phase 4). Money another person sends to an online account is income (PLAN.md, Phase 5). In Phase 1 neither is recorded, and the balance of an online account is not tracked.
- Stripe is not an online account: it is a service merchants use to charge cards. Paying "via Stripe" is a card payment.
- Cards on an online account, physical or virtual, are covered by US-ACC-03/AC6 and AC7. They are not repeated here, so each behavior has one AC. Test cases combine them (for example a virtual card on an AirCash account).
