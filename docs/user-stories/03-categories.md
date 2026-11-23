# Categories

**Phase:** 1a
**Status:** Reviewed
**Related risks:** R7 (deleting or archiving), R2 (data of another user), R6 (validation gaps)

---

### US-CAT-01: Use predefined categories

**As a** new user
**I want** a ready set of common categories
**so that** I can start recording expenses right away

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I have just registered | I open the category list | I see Food, Fuel, Bills, Housing, Health, Clothing, Entertainment, Travel and Other, each with its own color |
| AC2 | I have renamed the predefined category "Food" to "Groceries" | Another user opens their category list | They still see "Food" |

**Risks:** R2
**Notes:**
- Each user gets their own copy of the predefined categories at registration. From then on they behave like the user's own categories and can be renamed, recolored and archived. A change never affects other users.

---

### US-CAT-02: Create a custom category

**As a** user
**I want** to create my own categories
**so that** expenses are grouped the way I think about them

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | I am on the new category form | I enter the name "Kindergarten", choose a color and save | The category appears in my list and can be selected for expenses |
| AC2 | I already have a category "Fuel" | I create a category named ` fuel ` | Saving fails with "You already have a category with this name." (compared case-insensitively, without leading and trailing spaces) |
| AC3 | User B has a category "Kindergarten" | I (user A) create "Kindergarten" | It is allowed; names are unique only within my own categories |
| AC4 | I am on the new category form | I leave the name empty or enter only spaces | Saving fails with "Name is required." |
| AC5 | I am on the new category form | I enter a name of 30 characters | It is accepted |
| AC6 | I am on the new category form | I enter a name of 31 characters | Saving fails with "Name can have at most 30 characters." |
| AC7 | I am on the new category form | I enter a name with local characters (`Čišćenje i održavanje`) | It is saved and shown exactly as entered |
| AC8 | I am on the new category form | I do not choose a color | A default color is assigned |

**Risks:** R6, R2

---

### US-CAT-03: Rename, archive, restore and delete a category

**As a** user
**I want** to change or retire categories I no longer use
**so that** the list of choices stays short without losing history

**Acceptance criteria**

| ID | Given | When | Then |
|---|---|---|---|
| AC1 | A category has expenses | I rename it | The new name is shown on all its existing expenses and in reports |
| AC2 | A category has expenses | I archive it | It is no longer offered when adding a new expense |
| AC3 | A category is archived | I open old expenses, filters and reports | Its expenses still show the category and are counted in reports; the category can be chosen in filters |
| AC4 | An expense has an archived category | I edit the expense (for example its amount) | The archived category stays on the expense and saving works |
| AC5 | A category is archived | I restore it | It is offered again for new expenses |
| AC6 | A category has no expenses | I delete it | The category is deleted |
| AC7 | A category has expenses | I try to delete it | Deleting is not allowed; I am offered to archive it instead |
| AC8 | All my categories but one are archived | I try to archive the last active category | It is not allowed: at least one category must stay active, so that new expenses can be recorded |
| AC9 | All my categories but one are deleted or archived | I try to delete the last active category | It is not allowed, for the same reason as in AC8 |
| AC10 | I deleted the category "Travel" (it had no expenses) | I create a new category "Travel" | It is allowed; a deleted category does not block its name |
| AC11 | I archived the category "Travel" | I create a new category "travel" | Saving fails with "You have an archived category with this name. Restore it instead?" and I am offered to restore it |

**Risks:** R7 (score 6)
**Notes:**
- A new user can delete or archive all predefined categories except one (AC8, AC9). Restoring the predefined set ("reset to defaults") is out of scope.
- AC10, AC11: a name must be unique among my active and archived categories. A deleted category no longer exists, so its name is free. An archived category keeps its name, because restoring it is better than having two categories with the same name in filters and reports.
- Reports come in Phase 1b. A deleted category had no expenses, so it never appears in reports. Archived categories do appear (AC3). The report stories will repeat this as their own ACs.
- Category states: active ⇄ archived; deleted only while the category has no expenses. Tested with state transition testing and the decision table "category state × action" (test plan, section 5.2).
