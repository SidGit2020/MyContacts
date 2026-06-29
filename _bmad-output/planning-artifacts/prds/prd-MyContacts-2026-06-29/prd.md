---
title: MyContacts — Birthday Reminder (v1)
status: final
created: 2026-06-29
updated: 2026-06-29
---

# PRD: MyContacts Birthday Reminder (v1)

## 0. Document Purpose

This PRD defines the first incremental feature addition to the MyContacts ASP.NET Core MVC web application: in-app birthday reminders. It is scoped for personal use by a single user (SIDDI) and is intentionally minimal — one feature, shipped fast, as the first step in a longer journey toward a Personal Life OS. Downstream work (additional reminders, notifications, multi-user support) will be addressed in future PRD increments.

---

## 1. Vision

MyContacts currently stores contact names, phone numbers, emails, and addresses. It does nothing with that data — it is a static rolodex.

The birthday reminder feature changes that. Every time the owner opens the web app, they see a glanceable banner showing contacts with birthdays today, tomorrow, or within the next week. No setup, no navigation — the information surfaces automatically. Over time, never missing a close contact's birthday becomes one of the quiet, reliable things the app does for you.

This is the first step: the app starts working *for* you, not just storing data.

---

## 2. Target User

### 2.1 Jobs To Be Done

- Remember upcoming birthdays without manually checking or relying on calendar entries scattered across other apps.
- Feel more attentive and connected to the people I care about.
- Keep my contacts data in one place that also does something useful with it.

### 2.2 Key User Journeys

**UJ-1. SIDDI opens the app and is reminded it's someone's birthday today.**
- **Persona + context:** SIDDI, personal use, opens MyContacts on a browser at any time during the day.
- **Entry state:** Logged in (or unauthenticated — single-user personal app). Lands on the Contacts List page.
- **Path:** Opens the web app → Contacts List page loads → a birthday banner appears at the top of the page showing contacts whose birthday is today, tomorrow, or within 7 days → SIDDI sees the name(s) and how many days away.
- **Climax:** SIDDI reads "🎂 Raj's birthday is today!" and remembers to call or send a message.
- **Resolution:** Banner is visible for the session. SIDDI continues using the app normally.
- **Edge case:** If no birthdays are upcoming in the next 7 days, no banner is shown (silence is correct — no noise).

---

## 3. Glossary

- **Contact** — A person record in MyContacts with fields: Id, Name, Phone, Email, Address, Notes. *After this feature: also DateOfBirth.*
- **DateOfBirth (DOB)** — An optional date field on a Contact. Only the month and day are used for birthday matching; year is stored but not required.
- **Birthday Window** — The rolling 7-day lookahead period used to determine which birthdays surface in the reminder banner. Today + 6 days.
- **Birthday Banner** — The UI element displayed at the top of the Contacts List page when one or more contacts have a birthday within the Birthday Window.
- **Upcoming Birthday** — A contact whose birthday (month + day) falls within the Birthday Window relative to today's date.

---

## 4. Features

### 4.1 Date of Birth on Contact

**Description:** The Contact record gains an optional `DateOfBirth` field. Users can set, edit, or clear it on the Create and Edit contact screens. It is not required — existing contacts without a DOB are unaffected. The year component is optional; if omitted, only month and day are stored.

**Functional Requirements:**

#### FR-1: Add DateOfBirth field to Contact

The Contact form (Create and Edit) includes an optional Date of Birth input. The field accepts a full date or month/day only.

**Consequences (testable):**
- Saving a contact with a DOB stores it in the database and displays it on the Contact Detail view.
- Saving a contact without a DOB stores `null`; no error is shown.
- Editing an existing contact allows adding, changing, or clearing the DOB.

**Out of Scope:**
- DOB is not displayed on the Contacts List page (only the birthday banner uses it there).

---

### 4.2 Birthday Reminder Banner

**Description:** When the Contacts List page loads, the app queries for any contacts whose birthday (month + day) falls within the next 7 days including today. If any are found, a banner is shown at the top of the page. If none are found, no banner is shown. Realizes UJ-1.

**Functional Requirements:**

#### FR-2: Birthday banner on Contacts List page

When the Contacts List page loads, the system checks for Upcoming Birthdays and displays a banner if any exist.

**Consequences (testable):**
- If one or more contacts have a birthday today: banner shows "🎂 [Name]'s birthday is today!" (one line per contact).
- If one or more contacts have a birthday in 1–6 days: banner shows "🎂 [Name]'s birthday is in X day(s)."
- If multiple contacts have birthdays in the window, all are listed in the banner, ordered by soonest first.
- If no contacts have a birthday in the next 7 days, no banner is rendered.
- Contacts without a DOB are excluded from the check — no errors, no noise.

**Out of Scope:**
- No persistence of "dismissed" state — banner reappears on every page load while birthdays are upcoming.
- No email, WhatsApp, or push notifications in this version.
- No sound or animation — plain Bootstrap alert only.

---

## 5. Non-Goals (Explicit)

- **No push or email notifications** — in-app banner on page load only for v1.
- **No multi-user support** — single owner only; no shared contact access.
- **No birthday age calculation displayed** — the year is stored but not surfaced in v1.
- **No "snooze" or "dismiss" persistence** — banner always shows if birthdays are upcoming.
- **No recurring reminders or escalation logic** — one banner, one window, done.
- **No anniversary or custom reminder types** — birthdays only in v1.

---

## 6. MVP Scope

### 6.1 In Scope

- Add optional `DateOfBirth` field to the `Contact` model and EF Core migration.
- Update Create Contact and Edit Contact forms to include DOB input.
- Display DOB on Contact Detail view.
- Birthday banner on the Contacts List page, showing contacts with birthdays in the next 7 days.
- Banner ordered by soonest birthday first.
- No banner shown when no upcoming birthdays exist.

### 6.2 Out of Scope for MVP

- Push notifications (deferred to v2 — requires mobile client or service worker).
- Email or WhatsApp reminders (deferred to v2).
- Recurring or scheduled background checks (deferred — banner is page-load only).
- Multi-user or shared contacts (deferred — personal use only).
- Anniversary or custom event reminders (deferred to v2).
- Age display or year-of-birth calculations (deferred).

---

## 7. Success Metrics

**Primary**
- **SM-1:** SIDDI catches at least one birthday they would otherwise have missed within the first month of use. *(Validates FR-2 — qualitative, self-assessed.)*

**Secondary**
- **SM-2:** All existing contacts can be updated with a DOB without data loss or errors. *(Validates FR-1.)*

**Counter-metrics (do not optimise)**
- **SM-C1:** Banner must not show false positives — a contact with no DOB set must never appear in the banner. Correctness over coverage.

---

## 8. Open Questions

None — all open questions resolved during PRD review.

---

## 9. Decisions Log

- **DOB input:** Single HTML date picker; year is required. Month/day-only entry is a v2 consideration.
- **Banner placement:** Top of the Contacts List page, above the contacts table. Styled as a Bootstrap `alert-info` component. No dismiss button, no persistence — reappears on every page load while birthdays are upcoming.
- **Birthday window:** 7 days (today + 6). Server local date used; no timezone handling in v1.
- **Schema change:** The `DateOfBirth` column is nullable and added via a standard EF Core migration. No existing data is affected.

---

## Constraints and Guardrails

**Technical:** Changes are additive only — the `DateOfBirth` column is nullable, and no existing Contact data is modified or at risk.

**Scope:** This PRD covers exactly one increment. Future features (WhatsApp reminders, multi-user, additional event types) are explicitly deferred and will be covered in separate PRD increments.
