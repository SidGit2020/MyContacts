---
baseline_commit: 75bfb69
---

# Story 2.2: Category Dropdown on Contact Create, Edit, and Detail Views

Status: ready-for-dev

## Story

As the app owner (SIDDI),
I want to assign a category to a contact when creating or editing, and see it on the Detail page,
So that I can classify contacts from day one and review their category at a glance.

## Acceptance Criteria

1. **Given** I navigate to the Create Contact page, **When** the page loads, **Then** a "Category" `<select>` dropdown is visible with a blank default option and the five options: Family, Colleague, Friends, Close Friends, Childhood Friends; the Notes textarea is absent.

2. **Given** I am on the Create Contact page, **When** I choose a category and click Save, **Then** the contact saves successfully and the chosen category is displayed on the Contact Detail page.

3. **Given** I am on the Create Contact page, **When** I leave the Category dropdown on the blank default and click Save, **Then** the contact saves successfully with no error and the Detail page shows no category value.

4. **Given** I am on the Edit Contact page for a contact with an existing Category, **When** I change the category and click Save, **Then** the new category is saved and displayed on the Detail page.

5. **Given** I am on the Edit Contact page for a contact with an existing Category, **When** I reset the dropdown to the blank default and click Save, **Then** the Category is stored as `null` and the Detail page shows no category.

6. **Given** the Notes field previously existed in Create and Edit views, **When** I open either form after this story is implemented, **Then** the Notes textarea is absent from both forms (confirmed completed in Story 2.1).

## Tasks / Subtasks

- [ ] Task 1: Add Category dropdown to `Views/Contacts/Create.cshtml` (AC: 1, 2, 3)
  - [ ] Insert the `<select asp-for="Category">` block between the Address and DateOfBirth `<div class="mb-3">` blocks
  - [ ] Include a blank default `<option value="">` and the five category options in order
  - [ ] Use `class="form-select"` (Bootstrap 5 select class) — NOT `form-control`
  - [ ] Use `<label asp-for="Category" class="form-label"></label>` (Tag Helper renders "Category" automatically)
- [ ] Task 2: Add Category dropdown to `Views/Contacts/Edit.cshtml` (AC: 4, 5)
  - [ ] Insert identical `<select asp-for="Category">` block between Address and DateOfBirth
  - [ ] `asp-for="Category"` automatically pre-selects the contact's existing category value on Edit
  - [ ] Same option list and classes as Create
- [ ] Task 3: Display Category on `Views/Contacts/Details.cshtml` (AC: 2, 4)
  - [ ] Add a conditional Category row after the Address row, before the DateOfBirth block
  - [ ] Show only when `!string.IsNullOrEmpty(Model.Category)` — same pattern as DateOfBirth
- [ ] Task 4: Normalize empty-string Category to null in `Controllers/ContactsController.cs` (AC: 3, 5)
  - [ ] In `Create` POST action: add `if (string.IsNullOrEmpty(contact.Category)) contact.Category = null;` before the `ModelState.IsValid` check
  - [ ] In `Edit` POST action: add the same line after the `id != contact.Id` guard and before `ModelState.IsValid`
  - [ ] Do NOT modify any other controller logic
- [ ] Task 5: Build and verify (AC: 1–5)
  - [ ] Run `dotnet build` — confirm 0 errors
  - [ ] Manually create a contact with a category → verify it appears on Detail page (AC: 2)
  - [ ] Manually create a contact with no category → verify Detail shows no category row (AC: 3)
  - [ ] Edit a contact to change category → verify Detail shows new value (AC: 4)
  - [ ] Edit a contact to clear category (blank option) → verify Detail shows no category row (AC: 5)

## Dev Notes

### Why the controller needs a change (empty string vs null)

When a `<select>` has `<option value="">` and the user submits without choosing, the form POST sends `Category=""` (empty string). ASP.NET Core model binding maps `""` to `string?` as `""`, not `null`. The EF Core schema stores `""` as a non-null TEXT value, which violates AC 3 and AC 5 requiring `null` storage.

Fix: add one line to each POST action before the DB save:
```csharp
if (string.IsNullOrEmpty(contact.Category))
    contact.Category = null;
```

This normalizes both `""` and `null` (defensive) to `null` before persistence.

### What to change — exact diffs

**File 1: `Views/Contacts/Create.cshtml`**

Insert this block between the Address `</div>` and DateOfBirth `<div>`:
```html
            <div class="mb-3">
                <label asp-for="Category" class="form-label"></label>
                <select asp-for="Category" class="form-select">
                    <option value="">-- Select Category --</option>
                    <option value="Family">Family</option>
                    <option value="Colleague">Colleague</option>
                    <option value="Friends">Friends</option>
                    <option value="Close Friends">Close Friends</option>
                    <option value="Childhood Friends">Childhood Friends</option>
                </select>
            </div>
```

Full resulting form body (between `<form>` and `<button>`):
```html
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>

            <div class="mb-3">
                <label asp-for="Name" class="form-label"></label>
                <input asp-for="Name" class="form-control" />
                <span asp-validation-for="Name" class="text-danger"></span>
            </div>
            <div class="mb-3">
                <label asp-for="Phone" class="form-label"></label>
                <input asp-for="Phone" class="form-control" />
            </div>
            <div class="mb-3">
                <label asp-for="Email" class="form-label"></label>
                <input asp-for="Email" class="form-control" />
            </div>
            <div class="mb-3">
                <label asp-for="Address" class="form-label"></label>
                <input asp-for="Address" class="form-control" />
            </div>
            <div class="mb-3">
                <label asp-for="Category" class="form-label"></label>
                <select asp-for="Category" class="form-select">
                    <option value="">-- Select Category --</option>
                    <option value="Family">Family</option>
                    <option value="Colleague">Colleague</option>
                    <option value="Friends">Friends</option>
                    <option value="Close Friends">Close Friends</option>
                    <option value="Childhood Friends">Childhood Friends</option>
                </select>
            </div>
            <div class="mb-3">
                <label asp-for="DateOfBirth" class="form-label">Date of Birth</label>
                <input asp-for="DateOfBirth" type="date" class="form-control" />
            </div>
```

---

**File 2: `Views/Contacts/Edit.cshtml`**

Insert the identical Category block between Address and DateOfBirth (same HTML as Create). The `asp-for="Category"` Tag Helper automatically pre-selects the contact's saved category when the Edit page loads.

---

**File 3: `Views/Contacts/Details.cshtml`**

Insert after the Address `<dd>` row and before the `@if (Model.DateOfBirth.HasValue)` block:
```html
    @if (!string.IsNullOrEmpty(Model.Category))
    {
        <dt class="col-sm-2">Category</dt>
        <dd class="col-sm-10">@Model.Category</dd>
    }
```

---

**File 4: `Controllers/ContactsController.cs`**

Create POST — add normalization line:
```csharp
// POST: /Contacts/Create
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Contact contact)
{
    if (string.IsNullOrEmpty(contact.Category))
        contact.Category = null;

    if (!ModelState.IsValid)
        return View(contact);

    _db.Contacts.Add(contact);
    _db.SaveChanges();
    return RedirectToAction(nameof(Index));
}
```

Edit POST — add normalization line after the id guard:
```csharp
// POST: /Contacts/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Edit(int id, Contact contact)
{
    if (id != contact.Id) return NotFound();

    if (string.IsNullOrEmpty(contact.Category))
        contact.Category = null;

    if (!ModelState.IsValid)
        return View(contact);

    _db.Contacts.Update(contact);
    _db.SaveChanges();
    return RedirectToAction(nameof(Index));
}
```

### Tag Helper behaviour — asp-for on select

`asp-for="Category"` on a `<select>` element:
- On GET (load form): inspects `Model.Category` and adds `selected` attribute to the matching `<option>` automatically
- On POST + model error (return View(contact)): same — reflects the posted (now null-normalized) value
- No `asp-items` needed — inline `<option>` elements are the correct approach here (no ViewBag required)

### Bootstrap class for selects

Use `class="form-select"` — NOT `class="form-control"`. Bootstrap 5 uses `form-select` for `<select>` elements. `form-control` is for text inputs.

### What NOT to do

- Do NOT use `asp-items` or `ViewBag.Categories` — inline `<option>` elements are correct for a fixed, small list
- Do NOT use `form-control` on the `<select>` — use `form-select`
- Do NOT use `@Html.DropDownListFor` — Tag Helpers are the project standard (AD-1 Consistency Conventions)
- Do NOT add `[Required]` to Category — it remains optional
- Do NOT skip the controller normalization — `""` and `null` are not the same in SQLite storage
- Do NOT modify `GetUpcomingBirthdays`, `Delete`, `Index`, or `Details` controller actions — only `Create` and `Edit` POST need the null-normalization line

### Project Structure Notes

- Files modified (this story): `Views/Contacts/Create.cshtml`, `Views/Contacts/Edit.cshtml`, `Views/Contacts/Details.cshtml`, `Controllers/ContactsController.cs`
- Files NOT modified: `Models/Contact.cs`, `Views/Contacts/Index.cshtml`, `Views/Contacts/Delete.cshtml`
- No new files created in this story
- Story 2.3 will modify: `Views/Contacts/Index.cshtml`, `Controllers/ContactsController.cs` (add Category column + filter to list)

### Previous story intelligence (Story 2.1)

- Notes textarea was removed from Create (lines 31-34), Edit (lines 32-35), and Details (lines 22-23) — confirmed absent in current file state
- Category (`string?`) is now in the model and DB — no further model changes needed
- Controller has no `[Bind]` attribute — all Contact properties bind automatically from form POST
- Pre-existing NU1903 SQLite warning in build output — ignore, not introduced by this work

### References

- [Source: `Controllers/ContactsController.cs`] — Create and Edit POST actions to modify
- [Source: `Views/Contacts/Create.cshtml`] — add Category dropdown
- [Source: `Views/Contacts/Edit.cshtml`] — add Category dropdown
- [Source: `Views/Contacts/Details.cshtml`] — add conditional Category display
- [Source: `_bmad-output/planning-artifacts/epics.md#Story-2.2`] — acceptance criteria origin
- [Source: `_bmad-output/project-context.md#Framework-Specific-Rules`] — Tag Helpers, ValidateAntiForgeryToken, ModelState pattern

### Referenced Planning Artifacts

Specific sections relevant to this story. Use `Read` with `offset`/`limit` to jump directly.

**Architecture** — `_bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md`
- Line 32 `### AD-1 — Layered MVC Paradigm [ADOPTED]` — null normalization belongs in controller (business logic), not in views
- Line 68 `## Consistency Conventions` — use Tag Helpers (`asp-for`, `form-select`); no `Html.DropDownListFor` or raw HTML

**PRD** — `_bmad-output/planning-artifacts/prds/prd-MyContacts-2026-06-29/prd.md`
- Line 154 `## 9. Decisions Log` — optional field display pattern: show row only when value exists (same as DateOfBirth)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

### Completion Notes List

### File List
