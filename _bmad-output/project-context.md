---
project_name: 'MyContacts'
user_name: 'SIDDI'
date: '2026-06-29'
sections_completed: ['technology_stack', 'language_rules', 'framework_rules', 'testing_rules', 'quality_rules', 'workflow_rules', 'anti_patterns']
status: 'complete'
rule_count: 20
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

| Technology | Version |
|---|---|
| .NET / C# | 10.0 / 13 |
| ASP.NET Core MVC | 10.0 |
| EF Core (SQLite provider) | 10.0.9 |
| Bootstrap | 5.3.3 (bundled — `wwwroot/lib/bootstrap/`) |
| SQLite | embedded (no separate install) |

---

## Critical Implementation Rules

### Language-Specific Rules

- Nullable reference types are **ENABLED** (`<Nullable>enable</Nullable>`) — every `string?` / `DateTime?` requires a null guard before use
- Implicit usings are **ENABLED** — do not add redundant `using System;` or other common namespaces
- Use `nameof(ActionName)` in redirects and route references — never string literals

### Framework-Specific Rules (ASP.NET Core MVC)

- Every POST action **MUST** have `[ValidateAntiForgeryToken]` — no exceptions
- POST actions **MUST** check `if (!ModelState.IsValid) return View(model)` **before** any DB write
- Single-entity lookup: use `_db.Contacts.Find(id)` (not LINQ) → immediately null-check → `return NotFound()`
- After any successful mutation: `return RedirectToAction(nameof(Index))`
- The injected `AppDbContext` field is always named `_db`
- Use Tag Helpers (`asp-action`, `asp-for`, `asp-route-id`) — never `Html.ActionLink` or raw hrefs
- The Contacts Index view `@model` stays `IEnumerable<Contact>` — birthday data passes via `ViewBag.UpcomingBirthdays`

### Testing Rules

- No automated test framework is configured for v1 — verify features manually in the browser

### Code Quality & Style Rules

- **PascalCase**: classes, methods, properties, filenames
- **camelCase**: local variables, parameters
- **`_camelCase`**: private fields (e.g., `_db`)
- One class per file; filename must match class name
- No inline comments unless the *why* is truly non-obvious — do not explain what the code does

### Development Workflow Rules

- **Schema changes**: delete `contacts.db` and restart the app — `EnsureCreated()` recreates the database with the updated schema
- **Do NOT run** `dotnet ef migrations add` — no EF Core migrations in v1
- All static assets are in `wwwroot/lib/` — no npm, no CDN references, no external links

### Critical Don't-Miss Rules

1. **Use `DateTime.Today` not `DateTime.Now`** for birthday comparisons — `Today` is date-only; `Now` includes a time component that breaks date equality

2. **Null-guard DOB before any date math** — always check `contact.DateOfBirth.HasValue` first; contacts without a DOB must never appear in the birthday banner

3. **Year-boundary birthday arithmetic** — construct the birthday for this calendar year; if already past, advance to next year:
   ```csharp
   var dob = contact.DateOfBirth!.Value;
   var thisYear = new DateTime(DateTime.Today.Year, dob.Month, dob.Day);
   if (thisYear < DateTime.Today) thisYear = thisYear.AddYears(1);
   var days = (thisYear - DateTime.Today).Days;
   ```

4. **Birthday window is days 0–6** — 0 = today, 6 = six days out; include contact when `days >= 0 && days <= 6`

5. **Always set `ViewBag.UpcomingBirthdays`** — assign an empty list when no birthdays are upcoming; never leave it null; the view guards on `.Count`

6. **Bootstrap alert class is `alert alert-info`** — both classes required; `alert-info` alone does not render correctly

7. **No dismiss button on the birthday banner** — no `btn-close`, no `data-bs-dismiss`; the banner is plain Bootstrap alert only

8. **`_db.SaveChanges()` in POST actions only** — never call it from a GET action or from a view

---

## Usage Guidelines

**For AI Agents:** Read this file before implementing any code. Follow all rules exactly. When in doubt, prefer the more restrictive option.

**For Humans:** Keep this lean. Update when the stack changes. Remove rules that become obvious over time.

_Last Updated: 2026-06-29_
