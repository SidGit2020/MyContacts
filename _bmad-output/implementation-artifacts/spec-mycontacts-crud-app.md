---
title: 'MyContacts CRUD Application'
type: 'feature'
created: '2026-06-26'
status: 'done'
baseline_commit: 'NO_VCS'
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** No contact management system exists; need a simple CRUD app to manage personal contacts and validate the BMad development workflow end-to-end.

**Approach:** Scaffold an ASP.NET Core 8 MVC application backed by EF Core + SQLite, implementing Create, Read (list + detail), Update, and Delete for a Contact entity with five fields.

## Boundaries & Constraints

**Always:**
- ASP.NET Core 8 MVC, EF Core 8, SQLite provider
- `Name` field is Required (server-side validation via DataAnnotations)
- Bootstrap for UI (default MVC template)
- `EnsureCreated()` for DB init (no migrations in v1)
- List page shows: Name, Phone, Email columns + action links

**Ask First:**
- Any schema change beyond the 5 defined fields (Name, Phone, Email, Address, Notes)
- Any change to the routing or project namespace

**Never:**
- Authentication / authorization
- Search, filter, or pagination
- Import / export
- Client-side validation libraries beyond what ships with the MVC template
- Async EF queries (keep synchronous for v1 simplicity)

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| List contacts (empty) | No contacts in DB | Index page renders with empty table | N/A |
| List contacts | Contacts exist in DB | Table shows Name, Phone, Email per row with Edit/Details/Delete links | N/A |
| Create — valid | Name filled, other fields optional | Contact saved, redirect to Index | N/A |
| Create — missing Name | Name empty, form submitted | Form redisplays with ValidationSummary error "The Name field is required." | Server-side model state invalid |
| Edit — valid | Modified fields submitted | Contact updated in DB, redirect to Index | N/A |
| Details / Edit / Delete — bad ID | Non-existent contact ID in URL | Return HTTP 404 | NotFound() |
| Delete — confirmed | User clicks Delete on confirmation page | Contact removed from DB, redirect to Index | N/A |

</frozen-after-approval>

## Code Map

- `MyContacts.csproj` -- project file; target net8.0, add EF Core SQLite + Design packages
- `appsettings.json` -- add `ConnectionStrings.DefaultConnection` for SQLite file path
- `Models/Contact.cs` -- Contact entity: Id, Name (Required), Phone, Email, Address, Notes
- `Data/AppDbContext.cs` -- EF Core DbContext with `DbSet<Contact> Contacts`
- `Program.cs` -- register AppDbContext with SQLite; call `EnsureCreated()` on startup
- `Controllers/ContactsController.cs` -- CRUD actions: Index, Create (GET/POST), Details, Edit (GET/POST), Delete (GET), DeleteConfirmed (POST)
- `Views/Contacts/Index.cshtml` -- contact list table with action links
- `Views/Contacts/Create.cshtml` -- create form with all 5 fields + ValidationSummary
- `Views/Contacts/Edit.cshtml` -- edit form with all 5 fields + ValidationSummary
- `Views/Contacts/Details.cshtml` -- read-only display of all fields
- `Views/Contacts/Delete.cshtml` -- delete confirmation page showing contact Name

## Tasks & Acceptance

**Execution:**
- [x] `MyContacts.csproj` -- run `dotnet new mvc -n MyContacts --no-https` in project root, then add `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design` NuGet packages -- scaffolds the MVC project and pulls in EF Core dependencies
- [x] `Models/Contact.cs` -- create Contact class with properties: `int Id`, `string Name` ([Required]), `string? Phone`, `string? Email`, `string? Address`, `string? Notes` -- defines the entity
- [x] `Data/AppDbContext.cs` -- create AppDbContext inheriting DbContext with `DbSet<Contact> Contacts` and constructor accepting `DbContextOptions<AppDbContext>` -- EF Core data layer
- [x] `appsettings.json` -- add `"ConnectionStrings": { "DefaultConnection": "Data Source=contacts.db" }` -- SQLite file location
- [x] `Program.cs` -- register `builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(...))`, then after `app.Build()` call `db.Database.EnsureCreated()` via scoped service -- wires up DB on startup
- [x] `Controllers/ContactsController.cs` -- implement all CRUD actions injecting AppDbContext; return NotFound() when contact ID not found -- full CRUD controller
- [x] `Views/Contacts/Index.cshtml` -- table with columns Name, Phone, Email; each row has Edit | Details | Delete links -- list view
- [x] `Views/Contacts/Create.cshtml` -- form with label+input for all 5 fields, ValidationSummary, submit button -- create view
- [x] `Views/Contacts/Edit.cshtml` -- same layout as Create with hidden Id field -- edit view
- [x] `Views/Contacts/Details.cshtml` -- display all 5 fields read-only + Edit and Back to List links -- detail view
- [x] `Views/Contacts/Delete.cshtml` -- show contact Name, confirmation message, Delete and Cancel buttons -- delete confirmation view

**Acceptance Criteria:**
- Given app starts, when navigating to `/Contacts`, then an empty contact list renders with a "Create New" link
- Given the Create form, when Name is left empty and submitted, then the form redisplays with "The Name field is required."
- Given valid contact data (Name + any optional fields), when Create is submitted, then the contact appears on the Index page
- Given an existing contact, when Edit form is submitted with a changed Name, then the updated Name appears on the Index page
- Given a contact on the Delete confirmation page, when Delete is confirmed, then the contact is gone from the Index
- Given a URL with a non-existent contact ID (e.g. `/Contacts/Details/9999`), when accessed, then HTTP 404 is returned

## Spec Change Log

## Design Notes

- `EnsureCreated()` is used instead of migrations for v1 simplicity. This means schema changes require deleting `contacts.db` and restarting.
- The Delete action uses two methods: `Delete` (GET — shows confirmation) and `DeleteConfirmed` (POST — performs delete). Anti-forgery token is included via `@Html.AntiForgeryToken()`.
- Namespace: `MyContacts` throughout.

## Verification

**Commands:**
- `dotnet build` -- expected: `Build succeeded. 0 Error(s)`
- `dotnet run` -- expected: app starts on `http://localhost:5000`; navigate to `/Contacts` and see an empty list with "Create New" link

## Suggested Review Order

**Application Wiring**

- DI registration, null-guarded connection string, EnsureCreated on startup
  [`Program.cs:6`](../../Program.cs#L6)

- SQLite connection string anchoring the DB file location
  [`appsettings.json:8`](../../appsettings.json#L8)

**Data Model**

- Contact entity — [Required] on Name, all other fields nullable
  [`Contact.cs:8`](../../Models/Contact.cs#L8)

- AppDbContext with null-forgiving DbSet initializer
  [`AppDbContext.cs:9`](../../Data/AppDbContext.cs#L9)

**CRUD Logic**

- Index and Create — entry points for the two most common user flows
  [`ContactsController.cs:22`](../../Controllers/ContactsController.cs#L22)

- Create POST — ModelState guard, Add + SaveChanges
  [`ContactsController.cs:29`](../../Controllers/ContactsController.cs#L29)

- Edit POST — id mismatch guard, Update + SaveChanges
  [`ContactsController.cs:50`](../../Controllers/ContactsController.cs#L50)

- Delete GET/POST split — confirmation page pattern with anti-forgery
  [`ContactsController.cs:68`](../../Controllers/ContactsController.cs#L68)

**UI — List**

- Index table: Name/Phone/Email columns, action links per row
  [`Index.cshtml:1`](../../Views/Contacts/Index.cshtml#L1)

**UI — Forms**

- Create form: ValidationSummary + asp-validation-for on Name
  [`Create.cshtml:10`](../../Views/Contacts/Create.cshtml#L10)

- Edit form: hidden Id field prevents over-posting the wrong record
  [`Edit.cshtml:11`](../../Views/Contacts/Edit.cshtml#L11)

- Delete confirmation: `method="post"` (patch applied — was missing, broke AC5)
  [`Delete.cshtml:13`](../../Views/Contacts/Delete.cshtml#L13)

**Supporting**

- Details view: read-only dl/dd display of all five fields
  [`Details.cshtml:1`](../../Views/Contacts/Details.cshtml#L1)
