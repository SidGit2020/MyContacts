# Deferred Work — MyContacts

Items surfaced during review that are explicitly out of v1 scope.

## For v2+

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
