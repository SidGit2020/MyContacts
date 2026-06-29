---
stepsCompleted: [1, 2, 3, 4]
inputDocuments:
  - _bmad-output/planning-artifacts/prds/prd-MyContacts-2026-06-29/prd.md
  - _bmad-output/planning-artifacts/research/technical-personal-life-os-india-architecture-research-2026-06-29.md
---

# MyContacts - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for MyContacts Birthday Reminder (v1), decomposing requirements from the PRD and technical architecture research into implementable stories.

## Requirements Inventory

### Functional Requirements

FR-1: The Contact Create and Edit forms include an optional Date of Birth field (HTML date picker, year required). Saving with a DOB stores it in the database and displays it on the Contact Detail view. Saving without a DOB stores null with no error shown. Editing allows adding, changing, or clearing an existing DOB.

FR-2: When the Contacts List page loads, the system queries for contacts whose birthday (month + day) falls within the next 7 days including today. If any are found, a birthday banner is displayed at the top of the page above the contacts table, ordered by soonest birthday first, showing the contact name and days until their birthday. If no upcoming birthdays exist, no banner is rendered.

### NonFunctional Requirements

NFR-1: Schema change must be additive only — the DateOfBirth column is nullable (DateTime?); no existing Contact data is modified or deleted.

NFR-2: Birthday matching uses server local date at page load time. No timezone conversion required in v1.

NFR-3: Correctness over coverage — contacts with no DOB set must never appear in the banner (no false positives).

NFR-4: Birthday check query executes at Contacts List page load; must not introduce a noticeable delay to page rendering.

### Additional Requirements

- AR-1: Use existing ASP.NET Core MVC pattern (controllers + Razor views) — no framework changes.
- AR-2: EF Core migration adds nullable `DateOfBirth` (`DateTime?`) column to the Contacts table. Migration run via `dotnet ef migrations add AddContactDateOfBirth` + `dotnet ef database update`.
- AR-3: Birthday banner styled as Bootstrap `alert-info` component — Bootstrap is already present in the app via the standard MVC template.
- AR-4: No starter template — the existing MyContacts app is the starting point; all changes are modifications to existing files.
- AR-5: Development database is SQLite; migration applies to the existing `mycontacts.db` file.

### UX Design Requirements

N/A — no UX design document exists for this increment. UI decisions are captured in the PRD Decisions Log:
- DOB input: single HTML `<input type="date">` field, year required
- Banner: Bootstrap `alert-info`, top of Contacts List page above the contacts table, no dismiss button

### FR Coverage Map

FR-1: Epic 1 (Stories 1.1 + 1.2) — DateOfBirth model + migration + Create/Edit forms + Detail view display
FR-2: Epic 1 (Story 1.3) — Birthday banner logic and display on Contacts List page
NFR-1: Epic 1 (Story 1.1) — Nullable column, additive migration, no existing data affected
NFR-2: Epic 1 (Story 1.3) — Server local date used at query time, no timezone conversion
NFR-3: Epic 1 (Story 1.3) — Contacts with null DOB excluded from birthday query (no false positives)
NFR-4: Epic 1 (Story 1.3) — Efficient LINQ query; no noticeable page load delay

## Epic List

### Epic 1: Birthday Reminders
SIDDI can record a Date of Birth for any contact and see an automatic birthday reminder banner on the Contacts List page whenever a birthday is coming up within the next 7 days.
**FRs covered:** FR-1, FR-2

## Epic 1: Birthday Reminders

SIDDI can record a Date of Birth for any contact and see an automatic birthday reminder banner on the Contacts List page whenever a birthday is coming up within the next 7 days.

### Story 1.1: Add DateOfBirth to Contact Model and Database

As the app owner (SIDDI),
I want the Contact model and database to support a nullable Date of Birth field,
So that birthday data can be stored per contact as the foundation for reminder features.

**Acceptance Criteria:**

**Given** `DateOfBirth` (`DateTime?`) is added to the `Contact` class in `Contact.cs`
**When** the existing `contacts.db` file is deleted and the app is started
**Then** `EnsureCreated()` recreates the database with a `DateOfBirth` column in the `Contacts` table

**Given** the migration has been applied
**When** a Contact is saved with a `DateOfBirth` value
**Then** the value is persisted to the database and can be read back correctly

**Given** the migration has been applied
**When** a Contact is saved with `DateOfBirth` set to `null`
**Then** the record saves successfully with no validation error

---

### Story 1.2: Date of Birth on Contact Create, Edit, and Detail Views

As the app owner (SIDDI),
I want to enter and view a contact's Date of Birth on the Create, Edit, and Detail pages,
So that I can manage birthday information for my contacts directly in the app.

**Acceptance Criteria:**

**Given** I navigate to the Create Contact page
**When** the page loads
**Then** an optional "Date of Birth" `<input type="date">` field is visible in the form

**Given** I am on the Create Contact page
**When** I enter a date in the Date of Birth field and click Save
**Then** the contact saves successfully and the DOB is displayed on the Contact Detail page

**Given** I am on the Create Contact page
**When** I leave the Date of Birth field empty and click Save
**Then** the contact saves successfully with no error and the Detail page shows no DOB value

**Given** I am on the Edit Contact page for a contact with no existing DOB
**When** I enter a date and click Save
**Then** the DOB is saved and displayed on the Contact Detail page

**Given** I am on the Edit Contact page for a contact with an existing DOB
**When** I clear the Date of Birth field and click Save
**Then** the DOB is removed (stored as null) and no error is shown

---

### Story 1.3: Birthday Reminder Banner on Contacts List Page

As the app owner (SIDDI),
I want to see a banner at the top of the Contacts List whenever a contact has a birthday within the next 7 days,
So that I never miss wishing someone on their birthday.

**Acceptance Criteria:**

**Given** a contact has a birthday today (month and day match today's server date)
**When** the Contacts List page loads
**Then** a Bootstrap `alert-info` banner appears above the contacts table showing "🎂 [Name]'s birthday is today!"

**Given** a contact has a birthday in 1–6 days from today
**When** the Contacts List page loads
**Then** the banner shows "🎂 [Name]'s birthday is in X day(s)"

**Given** multiple contacts have birthdays within the 7-day window
**When** the Contacts List page loads
**Then** all are listed in the banner ordered by soonest birthday first

**Given** no contacts have a birthday within the next 7 days
**When** the Contacts List page loads
**Then** no banner is rendered (no empty alert, no placeholder text)

**Given** one or more contacts have no Date of Birth set (null DOB)
**When** the Contacts List page loads
**Then** those contacts are excluded from the birthday check and do not appear in the banner

**Given** the 7-day window spans a year boundary (e.g., today is 28 Dec and a contact's birthday is 2 Jan)
**When** the Contacts List page loads
**Then** the contact appears in the banner correctly showing the number of days away
