# Deferred Work — MyContacts

Items surfaced during review that are explicitly out of v1 scope.

## Docker (v2+)

- Switch to non-root user in Dockerfile: `USER app` (base image provides it)
- Add `HEALTHCHECK` instruction to Dockerfile
- Pin base image tags to full patch version (e.g. `sdk:10.0.9`) for reproducible builds
- Move connection string to `.env` file (gitignored) instead of inline in docker-compose.yml
- Set `ASPNETCORE_ENVIRONMENT=Production` explicitly in docker-compose.yml
- Bind port to `127.0.0.1:8080:8080` when behind a reverse proxy
- Add `deploy.resources.limits` (memory/CPU) in docker-compose.yml
- Add `--locked-mode` to `dotnet restore` and commit `packages.lock.json`

## Code Review Findings — Story 1.1 (2026-06-29)

- **Edit form silently zeroes DateOfBirth on save** — Story 1.2 MUST add `DateOfBirth` to Edit.cshtml (full-entity `Update()` in ContactsController.cs overwrites all fields including DateOfBirth with null when the form doesn't post it)
- **Feb 29 birthday crashes Story 1.3 calculation** — `new DateTime(year, 2, 29)` throws `ArgumentOutOfRangeException` in non-leap years; Story 1.3 must handle explicitly (e.g., skip Feb 29 contacts or treat as Mar 1)
- **EnsureCreated() no-op on existing databases** — pre-existing AD-3 constraint; any developer pulling this change must manually delete `contacts.db`
- **No past-date validation on DateOfBirth** — future dates (e.g., 9999-12-31) are accepted; consider adding `[Range]` or custom validation in Story 1.2

## Code Review Findings — Story 1.2 (2026-06-29)

- **ModelOnly validation summary swallows property-level errors for DateOfBirth** — Create.cshtml and Edit.cshtml have no `<span asp-validation-for="DateOfBirth">`. Intentional per spec (optional field, no validation attributes). If future validation is added (e.g., `[Range]`), a validation span will be needed.
- **ToString("d") date display is culture-sensitive** — Details.cshtml renders `@Model.DateOfBirth.Value.ToString("d")` which is locale-aware (e.g., `15-01-1990` on en-IN, `1/15/1990` on en-US). Consider `ToString("MMM d, yyyy")` or `ToString("yyyy-MM-dd")` for unambiguous display.
- **No min/max constraints on DateOfBirth inputs** — Duplicate of Story 1.1 finding; future dates (year 9999) and past extremes (year 0001) are accepted client-side without feedback.

## App (v2+)

- Add `[MaxLength]` / `[StringLength]` on all Contact string fields
- Add `[EmailAddress]` validation on Contact.Email
- Add `[Phone]` or regex validation on Contact.Phone
- Whitespace-only Name rejection (`[Required]` passes `"   "` — add `[RegularExpression]` or trim in controller)
- Add `_ValidationScriptsPartial` to Create/Edit views for inline client-side validation UX
- Convert all EF Core calls to async (`ToListAsync`, `FindAsync`, `SaveChangesAsync`)
- Add pagination to Contact Index (currently loads full table)
- Anchor SQLite file path to `AppContext.BaseDirectory` (currently relative to CWD)
- Add optimistic concurrency (`RowVersion` / `[Timestamp]`) on Contact for multi-user edit conflicts
- Add `[Bind]` attribute or ViewModel to prevent over-posting on Create/Edit
- Add `try/catch` around `EnsureCreated()` for graceful startup failure
- Add `try/catch` around `SaveChanges()` calls for user-friendly DB error messages
- Restrict `AllowedHosts` to actual hostname in production `appsettings.Production.json`
