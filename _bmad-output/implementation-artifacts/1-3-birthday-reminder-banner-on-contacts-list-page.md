---
baseline_commit: 47f5b565b40862d1ca53c67aab9b11e2e3986457
---

# Story 1.3: Birthday Reminder Banner on Contacts List Page

Status: done

## Story

As the app owner (SIDDI),
I want to see a banner at the top of the Contacts List whenever a contact has a birthday within the next 7 days,
So that I never miss wishing someone on their birthday.

## Acceptance Criteria

1. **Given** a contact has a birthday today (month and day match today's server date), **When** the Contacts List page loads, **Then** a Bootstrap `alert-info` banner appears above the contacts table showing "🎂 [Name]'s birthday is today!"

2. **Given** a contact has a birthday in 1–6 days from today, **When** the Contacts List page loads, **Then** the banner shows "🎂 [Name]'s birthday is in X day(s)"

3. **Given** multiple contacts have birthdays within the 7-day window, **When** the Contacts List page loads, **Then** all are listed in the banner ordered by soonest birthday first

4. **Given** no contacts have a birthday within the next 7 days, **When** the Contacts List page loads, **Then** no banner is rendered (no empty alert, no placeholder text)

5. **Given** one or more contacts have no Date of Birth set (null DOB), **When** the Contacts List page loads, **Then** those contacts are excluded from the birthday check and do not appear in the banner

6. **Given** the 7-day window spans a year boundary (e.g., today is 28 Dec and a contact's birthday is 2 Jan), **When** the Contacts List page loads, **Then** the contact appears in the banner correctly showing the number of days away

## Tasks / Subtasks

- [x] Task 1: Add `GetUpcomingBirthdays()` private method to `ContactsController` (AC: 1, 2, 3, 4, 5, 6)
  - [x] Added private method after `DeleteConfirmed`, before closing `}` of class
  - [x] Method signature: `private List<(Contact contact, int daysUntil)> GetUpcomingBirthdays(IEnumerable<Contact> contacts)`
  - [x] Null-guard: `if (!contact.DateOfBirth.HasValue) continue;` — excludes null-DOB contacts (AC5)
  - [x] Feb 29 guard: separate block checks `DateTime.IsLeapYear(today.Year)` before constructing Feb 29 date
  - [x] General path: `new DateTime(today.Year, dob.Month, dob.Day)`, advance if past, include if `days >= 0 && days <= 6`
  - [x] Returns `result.OrderBy(x => x.daysUntil).ToList()` (AC3)

- [x] Task 2: Modify `Index()` action to call the method and assign `ViewBag.UpcomingBirthdays` (AC: 1–6)
  - [x] Added `ViewBag.UpcomingBirthdays = GetUpcomingBirthdays(contacts);` after `ToList()`, before `return View()`
  - [x] `GetUpcomingBirthdays` always returns non-null list — ViewBag always set
  - [x] No `_db.SaveChanges()` called — GET action, read-only

- [x] Task 3: Add birthday banner to `Views/Contacts/Index.cshtml` (AC: 1, 2, 3, 4)
  - [x] Cast added to `@{ }` block: `var upcomingBirthdays = (List<(MyContacts.Models.Contact contact, int daysUntil)>)ViewBag.UpcomingBirthdays;`
  - [x] `@if (upcomingBirthdays.Count > 0)` guard — no empty alert rendered (AC4)
  - [x] `<div class="alert alert-info" role="alert">` — both Bootstrap classes present
  - [x] No dismiss button — confirmed in HTML output
  - [x] Entry format: `🎂 @contact.Name's birthday @(days == 0 ? "is today!" : $"is in {days} day(s)")`
  - [x] Banner placed between `<h1>` and `<p><a asp-action="Create">` — above table

- [x] Task 4: Build and verify all ACs
  - [x] `dotnet build --no-restore` → 0 errors, 1 pre-existing NU1903 warning ✅
  - [x] AC4/AC5 baseline: Index page loaded with existing contacts (Alice, Bob — no upcoming DOBs) → no banner in HTML ✅
  - [x] AC1: "Birthday Today" (DOB 1990-06-29) → "🎂 Birthday Today's birthday is today!" ✅
  - [x] AC2: "Birthday Soon" (DOB 1985-07-02) → "🎂 Birthday Soon's birthday is in 3 day(s)" ✅
  - [x] AC2: "Birthday Edge" (DOB 2000-07-05) → "🎂 Birthday Edge's birthday is in 6 day(s)" ✅
  - [x] AC3: Banner order: Birthday Today (0) → Birthday Soon (3) → Birthday Edge (6) — soonest first ✅
  - [x] AC4: "Birthday Outside" (DOB 1995-07-07, 8 days away) absent from banner ✅
  - [x] AC5: Bob NoDOB (null DOB) absent from banner ✅
  - [x] AC6: Year-boundary handled by algorithm — `thisYear.AddYears(1)` for Dec 28 → Jan 2 case; verified by code logic per AD-6 ✅

## Dev Notes

### Architecture Rules That MUST Be Followed

- **AD-4**: Birthday logic lives ONLY in `GetUpcomingBirthdays()` — a private method on `ContactsController`. No birthday calculations in the view.
- **AD-5**: Result passes to the view via `ViewBag.UpcomingBirthdays` as `List<(Contact contact, int daysUntil)>`. The view's `@model` STAYS `IEnumerable<Contact>` — do NOT change it.
- **AD-6**: Year-boundary-safe arithmetic — construct thisYear, advance if past, compute days. Contacts with null DOB excluded before any date calculation.

### Files Being Modified

#### 1. `Controllers/ContactsController.cs`

**Current `Index()` action (lines 18–24):**
```csharp
// GET: /Contacts
public IActionResult Index()
{
    var contacts = _db.Contacts.ToList();
    return View(contacts);
    //Test Git Update to remote
    //Test commit to Git 2
}
```

**What changes:**
- Add `ViewBag.UpcomingBirthdays = GetUpcomingBirthdays(contacts);` before `return View(contacts);`
- The two stale comments (`//Test Git Update...`) may be left as-is — do not remove them

**New private method goes AFTER `DeleteConfirmed` (currently the last method, line 87–96):**
```csharp
// POST: /Contacts/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public IActionResult DeleteConfirmed(int id)
{
    var contact = _db.Contacts.Find(id);
    if (contact != null)
    {
        _db.Contacts.Remove(contact);
        _db.SaveChanges();
    }
    return RedirectToAction(nameof(Index));
}
// ← ADD GetUpcomingBirthdays HERE, before the closing `}` of the class
```

#### 2. `Views/Contacts/Index.cshtml`

**Current full file:**
```razor
@model IEnumerable<MyContacts.Models.Contact>

@{
    ViewData["Title"] = "Contacts";
}

<h1>Contacts</h1>

<p>
    <a asp-action="Create" class="btn btn-primary">Create New</a>
</p>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Name</th>
            <th>Phone</th>
            <th>Email</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var contact in Model)
        {
            <tr>
                <td>@contact.Name</td>
                <td>@contact.Phone</td>
                <td>@contact.Email</td>
                <td>
                    <a asp-action="Edit" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-secondary">Edit</a>
                    <a asp-action="Details" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-info">Details</a>
                    <a asp-action="Delete" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-danger">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

**What changes:**
- Add the ViewBag cast to the `@{ }` block
- Insert the banner div AFTER `<h1>Contacts</h1>` and BEFORE `<p><a asp-action="Create">...</a></p>`

### Exact Code to Write

#### Modified `Index()` action:
```csharp
// GET: /Contacts
public IActionResult Index()
{
    var contacts = _db.Contacts.ToList();
    ViewBag.UpcomingBirthdays = GetUpcomingBirthdays(contacts);
    return View(contacts);
    //Test Git Update to remote
    //Test commit to Git 2
}
```

#### New `GetUpcomingBirthdays` private method (append before closing `}` of class):
```csharp
private List<(Contact contact, int daysUntil)> GetUpcomingBirthdays(IEnumerable<Contact> contacts)
{
    var today = DateTime.Today;
    var result = new List<(Contact contact, int daysUntil)>();

    foreach (var contact in contacts)
    {
        if (!contact.DateOfBirth.HasValue) continue;

        var dob = contact.DateOfBirth.Value;

        if (dob.Month == 2 && dob.Day == 29)
        {
            if (DateTime.IsLeapYear(today.Year))
            {
                var leapBirthday = new DateTime(today.Year, 2, 29);
                var leapDays = (leapBirthday - today).Days;
                if (leapDays >= 0 && leapDays <= 6)
                    result.Add((contact, leapDays));
            }
            continue;
        }

        var thisYear = new DateTime(today.Year, dob.Month, dob.Day);
        if (thisYear < today) thisYear = thisYear.AddYears(1);
        var days = (thisYear - today).Days;

        if (days >= 0 && days <= 6)
            result.Add((contact, days));
    }

    return result.OrderBy(x => x.daysUntil).ToList();
}
```

#### Modified `Views/Contacts/Index.cshtml` (complete file):
```razor
@model IEnumerable<MyContacts.Models.Contact>

@{
    ViewData["Title"] = "Contacts";
    var upcomingBirthdays = (List<(MyContacts.Models.Contact contact, int daysUntil)>)ViewBag.UpcomingBirthdays;
}

<h1>Contacts</h1>

@if (upcomingBirthdays.Count > 0)
{
    <div class="alert alert-info" role="alert">
        @foreach (var (contact, days) in upcomingBirthdays)
        {
            <div>🎂 @contact.Name's birthday @(days == 0 ? "is today!" : $"is in {days} day(s)")</div>
        }
    </div>
}

<p>
    <a asp-action="Create" class="btn btn-primary">Create New</a>
</p>

<table class="table table-striped">
    <thead>
        <tr>
            <th>Name</th>
            <th>Phone</th>
            <th>Email</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var contact in Model)
        {
            <tr>
                <td>@contact.Name</td>
                <td>@contact.Phone</td>
                <td>@contact.Email</td>
                <td>
                    <a asp-action="Edit" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-secondary">Edit</a>
                    <a asp-action="Details" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-info">Details</a>
                    <a asp-action="Delete" asp-route-id="@contact.Id" class="btn btn-sm btn-outline-danger">Delete</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Critical Technical Rules

**1. Use `DateTime.Today` not `DateTime.Now`**
`Today` is date-only (midnight). `Now` includes a time component that would cause `(thisYear - DateTime.Now).Days` to return -1 instead of 0 for today's birthday.

**2. NEVER leave `ViewBag.UpcomingBirthdays` null**
The view casts it directly: `(List<...>)ViewBag.UpcomingBirthdays`. If it's null, the cast produces null and `.Count` throws `NullReferenceException`. The `GetUpcomingBirthdays` method always returns a non-null `List<>` (empty when no birthdays), so as long as `Index()` calls it, this is safe.

**3. ViewBag cast in Razor — WHY this is required**
`ViewBag` properties are `dynamic`. Named tuple fields (`contact`, `daysUntil`) are compile-time aliases — at runtime they're `Item1`/`Item2`. If you iterate over `dynamic` items and try `item.contact`, it throws a `RuntimeBinderException`. The cast to the concrete type restores the named fields and enables `var (contact, days) in upcomingBirthdays` tuple deconstruction.

**4. Feb 29 birthday handling — CRITICAL**
`new DateTime(nonLeapYear, 2, 29)` throws `ArgumentOutOfRangeException`. This would crash the entire Index page for ALL users whenever anyone has a Feb 29 birthday and the current year is not a leap year.

The exact fix:
```csharp
if (dob.Month == 2 && dob.Day == 29)
{
    if (DateTime.IsLeapYear(today.Year))
    {
        var leapBirthday = new DateTime(today.Year, 2, 29);
        var leapDays = (leapBirthday - today).Days;
        if (leapDays >= 0 && leapDays <= 6)
            result.Add((contact, leapDays));
    }
    continue; // skip regardless — already handled or intentionally skipped
}
```
Feb 29 contacts are only shown in the banner when the CURRENT year is a leap year AND the birthday is within the next 7 days.

**5. Bootstrap banner classes — BOTH required**
`<div class="alert alert-info">` — `alert` alone does nothing in Bootstrap 5; `alert-info` alone does nothing either. Both are required for the blue info banner styling.

**6. NO dismiss button**
The banner is informational, always visible on page load, no `×` close button. Do NOT add `<button class="btn-close" data-bs-dismiss="alert">`.

**7. `_db.SaveChanges()` never in GET actions**
`Index()` is a GET action. No `_db.SaveChanges()` call. No DB writes. Only `_db.Contacts.ToList()` (read) and `GetUpcomingBirthdays()` (in-memory LINQ on already-fetched list).

**8. NFR-4 compliance: no separate query**
`GetUpcomingBirthdays(contacts)` receives the already-fetched contact list and filters it in memory. Do NOT run a second database query (no `_db.Contacts.Where(...)` in `GetUpcomingBirthdays`). The single `_db.Contacts.ToList()` call in `Index()` is sufficient.

### Year-Boundary Logic Walkthrough

For today = Dec 28, DOB = Jan 2 (any year):
```
thisYear = new DateTime(today.Year, 1, 2)  → Jan 2, 2026
thisYear (Jan 2) < today (Dec 28)          → TRUE → advance
thisYear = thisYear.AddYears(1)            → Jan 2, 2027
days = (Jan 2, 2027 - Dec 28, 2026).Days  → 5 → INCLUDED ✅
```

For today = Dec 28, DOB = Dec 28 (any year):
```
thisYear = new DateTime(today.Year, 12, 28) → Dec 28, 2026
thisYear (Dec 28) < today (Dec 28)          → FALSE (equal, not less)
days = (Dec 28, 2026 - Dec 28, 2026).Days  → 0 → "is today!" ✅
```

**Note on `thisYear < today`:** This uses strict less-than. When `thisYear == today`, the condition is false, so no advance happens. `days = 0` correctly.

### Test Contacts for Verification

Today is June 29, 2026. Create these contacts to verify the ACs:

| Contact | DOB to enter | Expected banner text | AC |
|---------|-------------|---------------------|-----|
| Birthday Today | 1990-06-29 | "🎂 Birthday Today's birthday is today!" | AC1 |
| Birthday Soon | 1985-07-02 | "🎂 Birthday Soon's birthday is in 3 day(s)" | AC2 |
| Birthday Edge | 2000-07-05 | "🎂 Birthday Edge's birthday is in 6 day(s)" | AC2, AC3 boundary |
| Birthday Outside | 1995-07-07 | (does NOT appear — 8 days away) | AC4 implicitly |
| Bob NoDOB | (no DOB) | (does NOT appear — null DOB) | AC5 |

AC3 order: Birthday Today (0) → Birthday Soon (3) → Birthday Edge (6)

Use PowerShell `Invoke-WebRequest` with CSRF token (same pattern as Story 1.2) to POST these contacts.

### What NOT to Do

- Do NOT change `@model IEnumerable<Contact>` in `Index.cshtml` — stays as-is (AD-5)
- Do NOT create a ViewModel class — ViewBag is the correct mechanism for v1 (AD-5)
- Do NOT run a second DB query in `GetUpcomingBirthdays` — it receives the already-fetched list (NFR-4)
- Do NOT add birthday logic directly in the Razor view (AD-4)
- Do NOT use `DateTime.Now` — use `DateTime.Today` (project-context rule 1)
- Do NOT add `[ValidateAntiForgeryToken]` to `Index()` — it's a GET action; AVFT is for POST only
- Do NOT call `_db.SaveChanges()` anywhere in this story — no mutations
- Do NOT change the Delete, Create, Edit, or Details actions — they are out of scope
- Do NOT delete `contacts.db` — the schema already has `DateOfBirth` from Story 1.1; no schema changes needed
- Do NOT add `role="alert"` as a mandatory requirement but include it for accessibility — `<div class="alert alert-info" role="alert">`

### Learnings from Stories 1.1 and 1.2

- Build fails if a MyContacts.exe process is still running and locking the binary (MSB3027). Kill with `Stop-Process -Name "MyContacts" -Force` before building.
- Use `Stop-Process -Name "MyContacts" -Force` AND kill any dotnet processes to free port 5244 before starting a new instance.
- `dotnet build --no-restore` is faster than `dotnet build` — use it after confirming NuGet packages are present.
- PowerShell `Invoke-WebRequest` with `$session` cookie for CSRF token — pattern established in Story 1.2 dev agent record.
- The test contacts from Story 1.2 (Alice Smith with DOB cleared, Bob NoDOB) are already in the database. Neither has an upcoming birthday in June/July, which satisfies AC4/AC5 baseline.
- Port 5244 is the default from `Properties/launchSettings.json`.

### References

- [Source: Controllers/ContactsController.cs] — file to modify (Index + new GetUpcomingBirthdays method)
- [Source: Views/Contacts/Index.cshtml] — file to modify (add birthday banner)
- [Source: _bmad-output/planning-artifacts/epics.md#Story-1.3] — acceptance criteria origin
- [Source: _bmad-output/project-context.md#Critical-Don't-Miss-Rules] — DateTime.Today, null guard, year-boundary arithmetic, ViewBag rules
- [Source: _bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-4] — birthday logic in controller private method
- [Source: _bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-5] — ViewBag.UpcomingBirthdays, @model stays IEnumerable<Contact>
- [Source: _bmad-output/planning-artifacts/architecture/architecture-MyContacts-2026-06-29/ARCHITECTURE-SPINE.md#AD-6] — year-boundary arithmetic
- [Source: _bmad-output/implementation-artifacts/deferred-work.md#Code-Review-Findings-Story-1.1] — Feb 29 crash fix requirement

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- `dotnet build --no-restore` → 0 errors (1 pre-existing NU1903 SQLitePCLRaw warning)
- First app start: Index page confirmed no banner with existing contacts (AC4 baseline) ✅
- Port 5244 conflict on second start — existing process still running; solved by using `Start-Process` (hidden window) with `Start-Sleep -Seconds 15` and response check
- Created 4 test contacts via PowerShell CSRF pattern (same as Story 1.2):
  - "Birthday Today" DOB 1990-06-29 → 302 redirect ✅
  - "Birthday Soon" DOB 1985-07-02 → 302 redirect ✅
  - "Birthday Edge" DOB 2000-07-05 → 302 redirect ✅
  - "Birthday Outside" DOB 1995-07-07 → 302 redirect ✅
- Index page HTML confirmed: `<div class="alert alert-info" role="alert">` with 3 birthday entries ✅
- AC1: "🎂 Birthday Today's birthday is today!" ✅
- AC2: "🎂 Birthday Soon's birthday is in 3 day(s)" ✅  
- AC2: "🎂 Birthday Edge's birthday is in 6 day(s)" ✅
- AC3: Ordered Today(0) → Soon(3) → Edge(6) ✅
- AC4/AC5: Birthday Outside (8d) and Bob NoDOB absent from banner ✅
- Banner positioned correctly: after `<h1>Contacts</h1>`, before `<p><a asp-action="Create">` ✅

### Completion Notes List

- Added `GetUpcomingBirthdays(IEnumerable<Contact> contacts)` private method to `ContactsController` (after `DeleteConfirmed`)
- Feb 29 birthday safety: separate block checks `DateTime.IsLeapYear(today.Year)` before constructing Feb 29 date — prevents `ArgumentOutOfRangeException` in non-leap years
- Modified `Index()` to call `GetUpcomingBirthdays(contacts)` and assign to `ViewBag.UpcomingBirthdays` — always non-null (empty list when no upcoming birthdays)
- Added birthday banner to `Views/Contacts/Index.cshtml`: cast `ViewBag.UpcomingBirthdays` to concrete type in `@{ }` block, `@if (.Count > 0)` guard, `alert alert-info` Bootstrap classes, no dismiss button
- View's `@model IEnumerable<Contact>` unchanged (AD-5 compliance)
- No second DB query — in-memory filter on already-fetched contact list (NFR-4 compliance)
- `contacts.db` now has 6 contacts: ID1 Alice (DOB cleared), ID2 Bob (no DOB), ID3 Birthday Today (06-29), ID4 Birthday Soon (07-02), ID5 Birthday Edge (07-05), ID6 Birthday Outside (07-07)

### File List

- Controllers/ContactsController.cs (modified — added ViewBag assignment to Index(), added GetUpcomingBirthdays() private method)
- Views/Contacts/Index.cshtml (modified — added birthday banner with ViewBag cast and conditional rendering)

## Senior Developer Review (AI)

**Review Date:** 2026-06-29
**Outcome:** Approved with 1 patch — no blockers
**Layers:** Blind Hunter ✅, Edge Case Hunter ✅, Acceptance Auditor ✅

### Action Items

- [x] [Review][Patch] View self-protection: `ViewBag.UpcomingBirthdays` direct cast has no null guard — `upcomingBirthdays.Count` throws NullReferenceException if any future action renders Index.cshtml without setting ViewBag `[Views/Contacts/Index.cshtml:5]` — FIXED: added `?? new()` null-coalescing guard
- [x] [Review][Defer] Feb 29 contacts silently suppressed in non-leap years (3 of 4 years) — deferred; by design per deferred-work.md "skip Feb 29 contacts"; acceptable for v1
- [x] [Review][Defer] `contact.Name` null renders `'s birthday` — deferred; `[Required]` + `= string.Empty` prevent via forms; all views share this pattern; app-wide concern not banner-specific
- [x] [Review][Defer] `days >= 0` is dead code after advance logic — deferred; invariant is correct and harmless; defensive clarity acceptable
