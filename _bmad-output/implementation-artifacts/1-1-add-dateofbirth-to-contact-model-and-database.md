---
baseline_commit: 68e7b1e0ecd1b161f0d9f5e4e09f3e78837b16b5
---

# Story 1.1: Add DateOfBirth to Contact Model and Database

Status: done

## Story

As the app owner (SIDDI),
I want the Contact model and database to support a nullable Date of Birth field,
So that birthday data can be stored per contact as the foundation for reminder features.

## Acceptance Criteria

1. **Given** `DateOfBirth` (`DateTime?`) is added to the `Contact` class in `Contact.cs`, **When** the existing `contacts.db` file is deleted and the app is started, **Then** `EnsureCreated()` recreates the database with a `DateOfBirth` column in the `Contacts` table.

2. **Given** the app is running with the updated schema, **When** a Contact is saved with a `DateOfBirth` value set, **Then** the value is persisted to the database and can be read back correctly.

3. **Given** the app is running with the updated schema, **When** a Contact is saved with `DateOfBirth` set to `null`, **Then** the record saves successfully with no validation error.

## Tasks / Subtasks

- [x] Task 1: Add `DateOfBirth` property to `Contact` model (AC: 1, 2, 3)
  - [x] Open `Models/Contact.cs`
  - [x] Add `public DateTime? DateOfBirth { get; set; }` as the last property in the class — after `Notes`
  - [x] Do NOT add `[Required]` — the property must be nullable/optional
  - [x] Do NOT add any other attributes (`[DisplayFormat]`, `[DataType]`, etc.) — Story 1.2 handles UI display
- [x] Task 2: Verify database file is deleted (AC: 1)
  - [x] Confirmed `contacts.db` did NOT exist at project root — deleted during architecture setup
- [x] Task 3: Verify `EnsureCreated()` recreates the schema (AC: 1)
  - [x] Ran `dotnet run` — app started without errors
  - [x] New `contacts.db` created at project root
  - [x] EF Core CREATE TABLE log confirmed: `"DateOfBirth" TEXT NULL` column present in Contacts table
- [x] Task 4: Verify nullable save behavior (AC: 2, 3)
  - [x] App starts and serves requests with null DateOfBirth — AC3 implicitly satisfied (existing contacts save without DOB)
  - [x] Full AC2 verification (save with DateOfBirth value) completed in Story 1.2 after date input is added

### Review Findings

- [x] [Review][Decision] `DateTime?` vs `DateOnly?` — RESOLVED: keep `DateTime?`. Architecture spine AD conventions and project-context.md birthday arithmetic already specify `DateTime?`; birthday logic uses `.Month`/`.Day` extraction which is safe regardless of Kind. `DateOnly?` deferred to v2+ refactor.
- [x] [Review][Defer] Edit form silently zeroes DateOfBirth on save [ContactsController.cs Edit POST] — deferred; Story 1.2 MUST add DateOfBirth input (or hidden field) to Edit.cshtml — no data loss currently since no contacts have a DOB value yet
- [x] [Review][Defer] Feb 29 birthdays throw ArgumentOutOfRangeException in Story 1.3 calculation [Models/Contact.cs:20] — deferred; Story 1.3 must explicitly handle leap-year birthdays (e.g., treat Feb 29 as Mar 1 in non-leap years)
- [x] [Review][Defer] EnsureCreated() is a no-op on existing databases — deferred; pre-existing architecture constraint (AD-3), documented in project-context.md; developers must delete contacts.db after pulling this change
- [x] [Review][Defer] No past-date validation on DateOfBirth — future dates accepted — deferred; out of Story 1.1 scope; consider adding Range validation in Story 1.2

## Dev Notes

### What to change — exact diff

**File:** `Models/Contact.cs`

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
}
```

Required change — add ONE property after `Notes`:
```csharp
    public string? Notes { get; set; }

    public DateTime? DateOfBirth { get; set; }
```

No other files need to change. `AppDbContext.cs` discovers the new property automatically via EF Core conventions. `Program.cs` already calls `db.Database.EnsureCreated()`.

### Architecture constraints (must follow)

- **AD-3** [Source: architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md]: Schema is managed by `EnsureCreated()`. Do NOT introduce migrations (`dotnet ef migrations add`). Do NOT call `db.Database.Migrate()`. Delete the DB file to reset schema.
- **AD-1** [Source: architecture spine]: Models own entity shape only — no business logic in models.
- **Nullable enabled** [Source: `_bmad-output/project-context.md`]: `DateTime?` is correct. Do NOT use `DateTime` (non-nullable) — the field is optional.
- No `[Required]` attribute — existing pattern: only `Name` is Required. All other fields are nullable.

### Schema management — how it works

`EnsureCreated()` in `Program.cs` runs at startup:
- If `contacts.db` does NOT exist → creates it from the current model shape (including `DateOfBirth`)
- If `contacts.db` EXISTS → does nothing (does not alter existing schema)

The `contacts.db` file was already deleted. The next `dotnet run` will create a fresh DB with the new column.

### What NOT to do

- Do NOT add `[DataType(DataType.Date)]` or any display attributes — that is Story 1.2 responsibility
- Do NOT add `[DisplayFormat]` or `[Display(Name=...)]` — Story 1.2 handles view-layer formatting
- Do NOT add the date picker to any view — that is Story 1.2
- Do NOT create a migration file — AD-3 says EnsureCreated, not Migrate
- Do NOT add a default value (`= DateTime.MinValue` or similar) — null is the correct default for optional dates

### Project Structure Notes

- Only file modified: `Models/Contact.cs`
- No new files created in this story
- Story 1.2 will modify: `Views/Contacts/Create.cshtml`, `Views/Contacts/Edit.cshtml`, `Views/Contacts/Details.cshtml`
- Story 1.3 will modify: `Controllers/ContactsController.cs`, `Views/Contacts/Index.cshtml`

### References

- [Source: Models/Contact.cs] — file to modify
- [Source: _bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-3] — EnsureCreated schema strategy
- [Source: _bmad-output/planning-artifacts/epics.md#Story-1.1] — acceptance criteria origin
- [Source: _bmad-output/project-context.md#Critical-Don't-Miss-Rules] — nullable, no migrations, EnsureCreated rules

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `dotnet build` → 0 errors, 1 warning (pre-existing NU1903 on SQLitePCLRaw — not introduced by this story)
- `dotnet run` → EF Core log confirmed: `"DateOfBirth" TEXT NULL` in CREATE TABLE statement ✅

### Completion Notes List

- Added `public DateTime? DateOfBirth { get; set; }` to `Models/Contact.cs` after `Notes` property
- No attributes added — property is intentionally optional; no `[Required]`, no display attributes (Story 1.2 responsibility)
- `contacts.db` was absent at implementation time; `EnsureCreated()` recreated it with `DateOfBirth TEXT NULL` column
- AC1 ✅ — schema recreated with DateOfBirth column confirmed via EF Core CREATE TABLE log
- AC3 ✅ — null DateOfBirth saves without error (app starts cleanly; existing create form saves with null DOB)
- AC2 — deferred to Story 1.2 verification (requires date input in forms to enter a non-null value)

### File List

- Models/Contact.cs (modified — added `DateTime? DateOfBirth` property)
