---
baseline_commit: 2a84da7d0813341f225e1c0c03e8b833f5fa0fc1
---

# Story 1.2: Date of Birth on Contact Create, Edit, and Detail Views

Status: review

## Story

As the app owner (SIDDI),
I want to enter and view a contact's Date of Birth on the Create, Edit, and Detail pages,
So that I can manage birthday information for my contacts directly in the app.

## Acceptance Criteria

1. **Given** I navigate to the Create Contact page, **When** the page loads, **Then** an optional "Date of Birth" `<input type="date">` field is visible in the form.

2. **Given** I am on the Create Contact page, **When** I enter a date in the Date of Birth field and click Save, **Then** the contact saves successfully and the DOB is displayed on the Contact Detail page.

3. **Given** I am on the Create Contact page, **When** I leave the Date of Birth field empty and click Save, **Then** the contact saves successfully with no error and the Detail page shows no DOB value.

4. **Given** I am on the Edit Contact page for a contact with no existing DOB, **When** I enter a date and click Save, **Then** the DOB is saved and displayed on the Contact Detail page.

5. **Given** I am on the Edit Contact page for a contact with an existing DOB, **When** I clear the Date of Birth field and click Save, **Then** the DOB is removed (stored as null) and no error is shown.

## Tasks / Subtasks

- [x] Task 1: Add DateOfBirth field to Create.cshtml (AC: 1, 2, 3)
  - [x] Open `Views/Contacts/Create.cshtml`
  - [x] Added `<div class="mb-3">` block with explicit label text "Date of Birth" and `type="date"` input after Notes block
  - [x] Note: `asp-for` with empty label body renders "DateOfBirth" (no PascalCase split in .NET 10) — used explicit label text instead
- [x] Task 2: Add DateOfBirth field to Edit.cshtml (AC: 4, 5)
  - [x] Open `Views/Contacts/Edit.cshtml`
  - [x] Added same block as Create.cshtml — explicit "Date of Birth" label, `type="date"` input
  - [x] `<input type="hidden" asp-for="Id" />` preserved at line 13 — not touched
  - [x] Pre-population verified: `value="1990-01-15"` confirmed in Edit page HTML
- [x] Task 3: Add DateOfBirth display to Details.cshtml (AC: 2, 3, 4, 5)
  - [x] Added conditional `@if (Model.DateOfBirth.HasValue)` block after Notes row
  - [x] Label: "Date of Birth", value: `@Model.DateOfBirth.Value.ToString("d")` renders locale short date (e.g., `15-01-1990`)
- [x] Task 4: Verify all 5 ACs in browser
  - [x] AC1: Create page shows `<label>Date of Birth</label>` and `<input type="date">` ✅
  - [x] AC2: Contact saved with DOB `1990-01-15`, displayed as `15-01-1990` on Details page ✅
  - [x] AC3: Contact saved without DOB, Details page shows no "Date of Birth" row ✅
  - [x] AC4: Edit page pre-populates `value="1990-01-15"` for existing DOB contact ✅
  - [x] AC5: Edit POST with empty DateOfBirth → 302 redirect → Details shows no "Date of Birth" row ✅

## Dev Notes

### Exact changes — copy-paste ready

**File 1: `Views/Contacts/Create.cshtml`**

Add this block after the existing Notes block (after line 33 `</div>`) and before the `<button type="submit">`:

```html
<div class="mb-3">
    <label asp-for="DateOfBirth" class="form-label"></label>
    <input asp-for="DateOfBirth" type="date" class="form-control" />
</div>
```

**File 2: `Views/Contacts/Edit.cshtml`**

Add the SAME block after the Notes block (after line 35 `</div>`) and before the `<button type="submit">`:

```html
<div class="mb-3">
    <label asp-for="DateOfBirth" class="form-label"></label>
    <input asp-for="DateOfBirth" type="date" class="form-control" />
</div>
```

**File 3: `Views/Contacts/Details.cshtml`**

Add this AFTER the Notes row (after `<dd class="col-sm-10">@Model.Notes</dd>`) and BEFORE the Edit/Back anchor tags:

```html
@if (Model.DateOfBirth.HasValue)
{
    <dt class="col-sm-2">Date of Birth</dt>
    <dd class="col-sm-10">@Model.DateOfBirth.Value.ToString("d")</dd>
}
```

### Current state of each file being modified

**Create.cshtml** (current, lines 31–37 — the Notes block and buttons area):
```html
            <div class="mb-3">
                <label asp-for="Notes" class="form-label"></label>
                <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
            </div>

            <button type="submit" class="btn btn-primary">Save</button>
            <a asp-action="Index" class="btn btn-outline-secondary">Cancel</a>
```

**Edit.cshtml** (current, lines 33–39 — the Notes block and buttons area):
```html
            <div class="mb-3">
                <label asp-for="Notes" class="form-label"></label>
                <textarea asp-for="Notes" class="form-control" rows="3"></textarea>
            </div>

            <button type="submit" class="btn btn-primary">Save</button>
            <a asp-action="Index" class="btn btn-outline-secondary">Cancel</a>
```

**Details.cshtml** (current, lines 22–27 — Notes row and action links):
```html
    <dt class="col-sm-2">Notes</dt>
    <dd class="col-sm-10">@Model.Notes</dd>
</dl>

<a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-secondary">Edit</a>
<a asp-action="Index" class="btn btn-outline-secondary">Back to List</a>
```

### Critical technical rules

**`type="date"` IS REQUIRED on the input — do not omit it:**
- Without it, ASP.NET Core's `asp-for` tag helper renders `DateTime?` as `type="datetime-local"` — wrong behavior
- With explicit `type="date"`, the InputTagHelper formats the bound DateTime? value as `yyyy-MM-dd` for the HTML value attribute
- HTML `<input type="date">` posts `yyyy-MM-dd` format, which the model binder correctly parses to `DateTime?`
- When the field is cleared, the browser posts an empty string → model binder produces `null` for `DateTime?` → correct per AC5

**No `[DataType(DataType.Date)]` on the model:**
- The architecture spine and project-context.md deliberately chose NOT to add display attributes to the model (Story 1.1 dev notes, architecture AD-1)
- The explicit `type="date"` in the view IS the workaround — no model change needed

**No `[Required]` on DateOfBirth:**
- The field is optional — ModelState.IsValid must pass with no DOB
- No validation span needed for DateOfBirth in forms (unlike Name which has `<span asp-validation-for="Name">`)

**No controller changes needed:**
- Create POST: `_db.Contacts.Add(contact)` + `_db.SaveChanges()` — already correct; model binding sets `DateOfBirth` from form (null or value)
- Edit POST: `_db.Contacts.Update(contact)` + `_db.SaveChanges()` — already correct; posts the full entity including `DateOfBirth`
- Details GET: `_db.Contacts.Find(id)` — already returns the full Contact including `DateOfBirth`

**CRITICAL — Edit form must post DateOfBirth to prevent data loss:**
- The Edit controller uses `_db.Contacts.Update(contact)` — a full-entity disconnected update that sets ALL columns
- If the Edit form did NOT include the DateOfBirth field, the model binder would set `contact.DateOfBirth = null`, and Update() would zero out any previously saved DOB
- Story 1.2 fixes this by adding the DateOfBirth input to Edit.cshtml — the field is visible, pre-populated by asp-for, and always included in the POST

**Details.cshtml — conditional rendering:**
- The DOB row must only appear when `Model.DateOfBirth.HasValue` — do NOT render an empty row when null
- This satisfies AC3 ("Detail page shows no DOB value") and AC5 (after clearing, "no DOB value shown")
- Use `Model.DateOfBirth.Value.ToString("d")` — the `.Value` is safe inside the HasValue guard

**Edit.cshtml — preserve the hidden Id field:**
- `<input type="hidden" asp-for="Id" />` at line 13 is CRITICAL — it posts the Contact Id for the Edit POST action's `int id` parameter
- Do NOT remove or reorder this — it must stay in the form

### Architecture constraints (must follow)

- **AD-1** (Layered MVC): Views receive data from the controller and render only — no DB calls, no business logic in `.cshtml`
- **AD-2** (Direct DbContext): No new DI or service layer — controller already has `_db`; this story does NOT touch the controller
- **Bootstrap pattern**: Use `class="mb-3"` wrapper div, `class="form-label"` on label, `class="form-control"` on input — exactly matching existing field pattern

### What NOT to do

- Do NOT add `[DataType(DataType.Date)]` to `Contact.cs` — deliberately excluded from Story 1.1; use explicit `type="date"` in view instead
- Do NOT add `[Display(Name = "Date of Birth")]` to `Contact.cs` — `asp-for` renders the label from the property name; "DateOfBirth" → "Date Of Birth" (auto-split by tag helper)
- Do NOT add a validation span for DateOfBirth — it's optional, no server-side or client-side validation needed for this story
- Do NOT change the controller — Create POST, Edit POST, and Details GET already handle DateOfBirth correctly via model binding
- Do NOT change the model or AppDbContext — Story 1.1 is done; schema exists
- Do NOT change the `@model` directive in any view — it stays `@model MyContacts.Models.Contact`
- Do NOT run `dotnet ef migrations add` — the project uses EnsureCreated (AD-3)
- Do NOT add `DateOfBirth` display to `Views/Contacts/Delete.cshtml` — out of scope for this story

### Learnings from Story 1.1

- Story 1.1 is complete (status: done). `DateTime? DateOfBirth` is in `Models/Contact.cs` as the last property.
- The `contacts.db` was recreated by EnsureCreated — the `DateOfBirth TEXT NULL` column exists and is ready.
- Code review confirmed: keep `DateTime?` (not `DateOnly?`); `type="date"` input + `.Month`/`.Day` extraction in Story 1.3 is the safe approach.
- Code review deferred a note: "Edit form silently zeroes DateOfBirth" — **this story fixes that** by including the DateOfBirth field in Edit.cshtml.
- Code review deferred for Story 1.3: Feb 29 birthday handling must be explicit.

### References

- [Source: Views/Contacts/Create.cshtml] — file to modify (add DateOfBirth input)
- [Source: Views/Contacts/Edit.cshtml] — file to modify (add DateOfBirth input, preserves hidden Id)
- [Source: Views/Contacts/Details.cshtml] — file to modify (add conditional DOB display)
- [Source: _bmad-output/planning-artifacts/epics.md#Story-1.2] — acceptance criteria origin
- [Source: _bmad-output/project-context.md#Critical-Don't-Miss-Rules] — nullable, type="date" pattern
- [Source: _bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-1] — views render only

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `dotnet build` → 0 errors (1 pre-existing NU1903 warning on SQLitePCLRaw)
- Build failed first attempt (MSB3027) — old process PID 30840 locking binary; killed via PowerShell, rebuilt cleanly
- First curl of Create page confirmed `<input type="date">` rendered with `type="date"` attribute ✅
- Note: `asp-for` with empty label body renders "DateOfBirth" not "Date of Birth" in .NET 10 — fixed by setting explicit label text `>Date of Birth</label>`
- AC1: Create page — `<label...>Date of Birth</label>` + `<input type="date">` confirmed in HTML ✅
- AC2: Contact ID 1 (Alice Smith, DOB 1990-01-15) created via form POST, Details showed `15-01-1990` ✅
- AC3: Contact ID 2 (Bob NoDOB, empty DateOfBirth) created; Details page searched for "Date of Birth" → False ✅
- AC4: Edit page for Contact 1 — `value="1990-01-15"` found in HTML ✅
- AC5: Edit POST for Contact 1 with empty DateOfBirth → 302; Details searched for "Date of Birth" → False ✅

### Completion Notes List

- Added `DateOfBirth` date input to `Views/Contacts/Create.cshtml` and `Views/Contacts/Edit.cshtml` (after Notes block, before Save button)
- Used explicit label text `>Date of Birth</label>` instead of empty body — `asp-for` for `DateTime?` without `[Display]` renders "DateOfBirth" (no PascalCase split in .NET 10)
- Edit.cshtml `<input type="hidden" asp-for="Id" />` preserved — not touched
- Added conditional `@if (Model.DateOfBirth.HasValue)` block to `Views/Contacts/Details.cshtml`
- `ToString("d")` renders locale-aware short date (server locale produced `15-01-1990` for 1990-01-15)
- No controller changes — Create POST, Edit POST, and Details GET already handle `DateOfBirth` via model binding
- All 5 ACs verified via automated HTTP tests (PowerShell `Invoke-WebRequest` with CSRF token)
- `contacts.db` now has 2 test contacts: ID 1 (Alice, DOB cleared), ID 2 (Bob, no DOB)

### File List

- Views/Contacts/Create.cshtml (modified — added DateOfBirth date input after Notes)
- Views/Contacts/Edit.cshtml (modified — added DateOfBirth date input after Notes)
- Views/Contacts/Details.cshtml (modified — added conditional DateOfBirth display row)
