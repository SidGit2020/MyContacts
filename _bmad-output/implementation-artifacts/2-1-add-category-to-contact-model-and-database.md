---
baseline_commit: 2084c8d
---

# Story 2.1: Add Category to Contact Model and Database

Status: ready-for-dev

## Story

As the app owner (SIDDI),
I want the Contact model and database to store a structured Category instead of free-text Notes,
So that contacts are classified consistently and ready for group-based features.

## Acceptance Criteria

1. **Given** `Category` (`string?`) is added to the `Contact` class and `Notes` (`string?`) is removed, **When** `contacts.db` is deleted and the app is restarted, **Then** `EnsureCreated()` recreates the database with a `Category` column and no `Notes` column in the `Contacts` table.

2. **Given** the schema has been recreated, **When** a Contact is saved with a `Category` value (e.g., "Family"), **Then** the value is persisted and can be read back correctly.

3. **Given** the schema has been recreated, **When** a Contact is saved with `Category` set to `null` (no category chosen), **Then** the record saves successfully with no validation error.

## Tasks / Subtasks

- [ ] Task 1: Update `Contact` model — remove Notes, add Category (AC: 1, 2, 3)
  - [ ] Open `Models/Contact.cs`
  - [ ] Remove the `public string? Notes { get; set; }` property entirely
  - [ ] Add `public string? Category { get; set; }` in its place (same position in the class)
  - [ ] Do NOT add `[Required]` — Category is optional/nullable
  - [ ] Do NOT add validation attributes — category value enforcement is the view's responsibility (dropdown in Story 2.2)
- [ ] Task 2: Remove Notes references from views — required to prevent build failure (AC: 1)
  - [ ] `Views/Contacts/Create.cshtml` — remove the entire `<div class="mb-3">` block containing the `asp-for="Notes"` textarea (lines 31–34)
  - [ ] `Views/Contacts/Edit.cshtml` — remove the entire `<div class="mb-3">` block containing the `asp-for="Notes"` textarea (lines 32–35)
  - [ ] `Views/Contacts/Details.cshtml` — remove the `<dt>Notes</dt>` and `<dd>@Model.Notes</dd>` row (lines 22–23)
  - [ ] Do NOT add Category to any view — that is Story 2.2's sole responsibility
- [ ] Task 3: Reset database and verify schema (AC: 1, 2, 3)
  - [ ] Delete `contacts.db` from the project root
  - [ ] Run `dotnet run` and confirm the app starts without errors and no build warnings about missing Notes property
  - [ ] Verify EF Core CREATE TABLE log shows `"Category" TEXT NULL` and no `Notes` column
  - [ ] Create a test contact via the UI (Name only) — confirm it saves with null Category and no error (AC: 3)

## Dev Notes

### CRITICAL: Removing Notes from the model breaks 3 view files — fix them in this story

`Notes` is currently referenced in 3 views via Tag Helpers and Razor expressions. Removing `Notes` from `Contact.cs` without updating these files causes a **compile error** that prevents the app from starting. The Notes blocks must be removed in this story before the model change is complete.

| File | What to remove |
|---|---|
| `Views/Contacts/Create.cshtml` | Lines 31–34: entire `<div class="mb-3">` block with `asp-for="Notes"` textarea |
| `Views/Contacts/Edit.cshtml` | Lines 32–35: entire `<div class="mb-3">` block with `asp-for="Notes"` textarea |
| `Views/Contacts/Details.cshtml` | Lines 22–23: `<dt class="col-sm-2">Notes</dt>` + `<dd class="col-sm-10">@Model.Notes</dd>` |

`Views/Contacts/Index.cshtml` — has NO Notes column; no change needed.
`Views/Contacts/Delete.cshtml` — has NO Notes reference; no change needed.

**Do NOT add Category to any view** — Story 2.2 owns all Category view additions.

### What to change — exact diffs

**File 1: `Models/Contact.cs`**

Current state (entire file):
```csharp
using System.ComponentModel.DataAnnotations;

namespace MyContacts.Models;

public class Contact
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Notes { get; set; }

    public DateTime? DateOfBirth { get; set; }
}
```

Required result (replace `Notes` with `Category`):
```csharp
using System.ComponentModel.DataAnnotations;

namespace MyContacts.Models;

public class Contact
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Category { get; set; }

    public DateTime? DateOfBirth { get; set; }
}
```

---

**File 2: `Views/Contacts/Create.cshtml`** — remove these 4 lines:
```html
            <div class="mb-3">
                <label asp-for="Notes" class="form-label"></label>
                <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
            </div>
```

---

**File 3: `Views/Contacts/Edit.cshtml`** — remove these 4 lines:
```html
            <div class="mb-3">
                <label asp-for="Notes" class="form-label"></label>
                <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
            </div>
```

---

**File 4: `Views/Contacts/Details.cshtml`** — remove these 2 lines:
```html
    <dt class="col-sm-2">Notes</dt>
    <dd class="col-sm-10">@Model.Notes</dd>
```

### Architecture constraints (must follow)

- **AD-3** [Source: `_bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md` line 44]: Schema is managed by `EnsureCreated()`. Do NOT run `dotnet ef migrations add`. Do NOT call `db.Database.Migrate()`. Delete `contacts.db` to apply schema changes.
- **AD-1** [Source: architecture spine line 32]: Models own entity shape only — no business logic in `Contact.cs`.
- **Nullable enabled** [Source: `_bmad-output/project-context.md`]: `string?` is correct for Category. Do NOT use non-nullable `string`.
- No `[Required]` on Category — only `Name` carries `[Required]`. All other contact fields are nullable.

### Schema management — how it works

`EnsureCreated()` in `Program.cs` runs at startup:
- If `contacts.db` does NOT exist → creates fresh schema from the current model (will include `Category`, will NOT include `Notes`)
- If `contacts.db` EXISTS → does nothing; schema remains stale with old columns

**Delete `contacts.db` before running after this change.** The file lives at the project root (`c:\MyContacts\contacts.db`).

### Valid category values

These strings are the five valid options — enforced by the `<select>` dropdown added in Story 2.2, stored verbatim in the DB:

| Stored value | Display label |
|---|---|
| `"Family"` | Family |
| `"Colleague"` | Colleague |
| `"Friends"` | Friends |
| `"Close Friends"` | Close Friends |
| `"Childhood Friends"` | Childhood Friends |
| `null` | (no category) |

The model stores any `string?` — no enum, no `[RegularExpression]` constraint. The dropdown in Story 2.2 constrains values at the UI level.

### What NOT to do

- Do NOT add `[Required]` to Category — it must be optional
- Do NOT create an enum for Category — a plain `string?` is correct; the dropdown in Story 2.2 constrains values
- Do NOT add Category to any Razor view — Story 2.2 owns all view additions for Category
- Do NOT leave `Notes` in the model and just add Category alongside it — Notes must be fully removed (substitution, not addition)
- Do NOT remove `DateOfBirth` — added in Epic 1, must be preserved
- Do NOT run EF migrations — `EnsureCreated()` is the schema strategy (AD-3)
- Do NOT leave Notes references in views — the app will not compile if you skip Task 2

### Project Structure Notes

- **Modified files (this story):** `Models/Contact.cs`, `Views/Contacts/Create.cshtml`, `Views/Contacts/Edit.cshtml`, `Views/Contacts/Details.cshtml`
- **Not modified:** `Controllers/ContactsController.cs` (no Notes reference), `Views/Contacts/Index.cshtml` (no Notes column), `Views/Contacts/Delete.cshtml` (no Notes reference)
- No new files created in this story
- Story 2.2 will modify: `Views/Contacts/Create.cshtml`, `Views/Contacts/Edit.cshtml`, `Views/Contacts/Details.cshtml` (add Category `<select>` dropdown)
- Story 2.3 will modify: `Views/Contacts/Index.cshtml`, `Controllers/ContactsController.cs` (Category column + filter)

### References

- [Source: `Models/Contact.cs`] — file to modify (remove Notes, add Category)
- [Source: `_bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-3`] — EnsureCreated schema strategy
- [Source: `_bmad-output/planning-artifacts/epics.md#Story-2.1`] — acceptance criteria origin
- [Source: `_bmad-output/project-context.md#Critical-Implementation-Rules`] — nullable types, no migrations, EnsureCreated rules

### Referenced Planning Artifacts

Specific sections relevant to this story. Use `Read` with `offset`/`limit` or `grep` to jump directly.

**Architecture** — `_bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md`
- Line 44 `### AD-3 — EnsureCreated Schema Management [ADOPTED]` — how schema changes are applied; delete DB and restart
- Line 68 `## Consistency Conventions` — naming conventions confirming `Category` (PascalCase property, `string?`)
- Line 94 `## Structural Seed` — confirms `Models/` as the correct location for `Contact.cs`

**PRD** — `_bmad-output/planning-artifacts/prds/prd-MyContacts-2026-06-29/prd.md`
- Line 102 `## 5. Non-Goals (Explicit)` — confirms no EF migrations; EnsureCreated is intentional
- Line 163 `## Constraints and Guardrails` — hard constraints governing schema management approach

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

### Completion Notes List

### File List
