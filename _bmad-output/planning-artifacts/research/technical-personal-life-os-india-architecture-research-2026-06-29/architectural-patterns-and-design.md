# Architectural Patterns and Design

## System Architecture Pattern: Modular Monolith

The Life OS uses a **Modular Monolith** — a single deployable unit with hard module boundaries enforced at compile time, not network boundaries. This is the correct architecture for a product team of 1–5 engineers at Phase 1.

```
┌─────────────────────────────────────────────────────────────┐
│                    MyLifeOS.Api (Entry Point)                │
│         ASP.NET Core Minimal API — thin HTTP layer only      │
└────────────┬───────────────┬──────────────┬─────────────────┘
             │               │              │
    ┌────────▼───┐  ┌────────▼───┐  ┌───────▼────────┐
    │  Contacts  │  │ Community  │  │    Identity    │
    │   Module   │  │  Module    │  │  (Auth+DPDP)   │
    └────────────┘  └────────────┘  └────────────────┘
             │               │              │
    ┌────────▼───┐  ┌────────▼───┐  ┌───────▼────────┐
    │   Growth   │  │     AI     │  │ Notifications  │
    │   Module   │  │   Module   │  │    Module      │
    └────────────┘  └────────────┘  └────────────────┘
             │               │              │
    ┌────────▼───────────────▼──────────────▼────────┐
    │              MyLifeOS.Shared Kernel             │
    │   Domain Events │ Base Entities │ Result<T>    │
    └──────────────────────────────────────────────── ┘
             │
    ┌────────▼───────────────────────────────────────┐
    │        Infrastructure Layer                     │
    │  PostgreSQL │ Redis │ Hangfire │ Blob Storage   │
    └─────────────────────────────────────────────────┘
```

**Module boundary rules:**
- Modules communicate only via MediatR domain events (not direct method calls)
- No module references another module's `Infrastructure` or `Domain` namespace directly
- Shared Kernel contains only generic building blocks, never business logic
- Each module owns its own EF Core `DbContext` partial or dedicated schema prefix

---

## DPDP-Compliant Data Architecture

**This is the most critical architectural decision for India deployment.**

The consent management system is a foundational layer, not an add-on:

**Consent schema (PostgreSQL):**

```sql
-- consent_records: immutable audit log — NEVER UPDATE, only INSERT
CREATE TABLE consent_records (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         uuid NOT NULL REFERENCES users(id),
    consent_type    text NOT NULL,  -- e.g. 'contacts.storage', 'ai.analysis', 'whatsapp.notify'
    purpose         text NOT NULL,  -- human-readable purpose description
    language_code   text NOT NULL,  -- 'hi', 'te', 'ta', 'en', etc. — language shown to user
    granted_at      timestamptz,
    revoked_at      timestamptz,
    ip_address      inet,
    user_agent      text,
    consent_text_hash text NOT NULL  -- SHA-256 of exact consent notice shown (proof of notice)
);

-- WORM behaviour enforced at application layer:
-- No UPDATE or DELETE permitted on consent_records (Hangfire deletion job uses soft-delete on user data, not consent log)

-- data_deletion_requests: 7-day SLA on withdrawal
CREATE TABLE data_deletion_requests (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         uuid NOT NULL,
    requested_at    timestamptz NOT NULL DEFAULT now(),
    scope           text NOT NULL,   -- 'contacts', 'interaction_logs', 'ai_insights', 'all'
    completed_at    timestamptz,
    status          text NOT NULL DEFAULT 'pending'  -- pending, processing, completed
);
```

**Key architecture decisions:**
- `consent_records` is **append-only** — no UPDATEs, no DELETEs. Withdrawal creates a new row with `revoked_at` set
- `consent_text_hash` stores SHA-256 of the exact consent notice text shown (preserves proof-of-notice if notice wording changes)
- Logs retained **minimum 1 year** (mandatory per DPDP Rules 2025)
- 72-hour breach notification: Application Insights alert + Hangfire job triggers DPBI notification workflow
- Data erasure: Hangfire delayed job queued on withdrawal with 7-day deadline

_Source: [Consent Management Framework DPDP — PrivacyGlobal](https://www.privacyglobal.org/blog/consent-management-framework-dpdp-act), [DPDP Consent Management 2026 — Digio](https://www.digio.in/blog/dpdp-consent-management-what-every-data-fiduciary-must-know-in-2026/)_

---

## Data Isolation: PostgreSQL Row-Level Security

**For a per-user personal data app, RLS is the architectural safety net.**

Every user's contacts, interactions, and AI insights must be invisible to other users at the database level, regardless of application code bugs:

```sql
-- Enable RLS on sensitive tables
ALTER TABLE contacts         ENABLE ROW LEVEL SECURITY;
ALTER TABLE interaction_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_insights      ENABLE ROW LEVEL SECURITY;
ALTER TABLE consent_records  ENABLE ROW LEVEL SECURITY;

-- Policy: user can only see their own rows
CREATE POLICY user_isolation ON contacts
    USING (user_id = current_setting('app.current_user_id')::uuid);

-- Critical: composite index with user_id FIRST (0.3ms at 50M rows)
CREATE INDEX idx_contacts_user_id_name ON contacts (user_id, name);
```

**ASP.NET Core middleware sets session variable per request:**

```csharp
public class RlsMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context, AppDbContext db) {
        var userId = context.User.GetUserId();
        // SET LOCAL (not SET) — scoped to transaction, never leaks across connection pool
        await db.Database.ExecuteSqlRawAsync(
            "SET LOCAL app.current_user_id = {0}", userId.ToString());
        await next(context);
    }
}
```

**Critical warning:** Use `SET LOCAL` only — `SET` persists across sessions in connection pooling and will leak User A's data to User B's subsequent request.

_Source: [Multi-Tenant .NET 9 with RLS — Medium](https://medium.com/@vahidbakhtiaryinfo/building-multi-tenant-net-9-applications-with-row-level-security-and-event-isolation-78cea5f60233), [PostgreSQL RLS Multi-Tenant SaaS — TechBuddies](https://www.techbuddies.io/2026/01/01/how-to-implement-postgresql-row-level-security-for-multi-tenant-saas/)_

---

## Async Processing Architecture: Hangfire

AI analysis, voice transcription, WhatsApp notifications, and data deletion are all **async workloads** — they must not block the HTTP request:

```
HTTP Request (Flutter)
  │
  ├── POST /interactions/log
  │     └── Save interaction (fast, synchronous)
  │     └── Enqueue background job → returns 202 Accepted
  │
Background Queue (Hangfire + PostgreSQL storage)
  ├── Job: TranscribeAudioJob(interactionId)
  │     └── Calls Faster-Whisper microservice
  │     └── Stores transcript
  │     └── Enqueues → AnalyseInteractionJob
  │
  ├── Job: AnalyseInteractionJob(interactionId)
  │     └── Calls Claude API with transcript + relationship history
  │     └── Stores AI insight
  │     └── Enqueues → ScheduleFollowUpReminderJob (if insight suggests follow-up)
  │
  └── Job: DeleteUserDataJob(userId, scope)   ← DPDP deletion workflow
        └── Triggered by consent withdrawal
        └── Executes within 7-day SLA
        └── Marks deletion_request as completed
```

**Hangfire configuration:**
- PostgreSQL as job storage (reuse existing DB — no additional infrastructure)
- Dashboard endpoint protected by `[Authorize(Roles = "Admin")]`
- Job retry: 3 attempts with exponential backoff; dead-letter queue for failed AI jobs
- Recurring jobs: daily reminder scheduling, weekly relationship health digest

_Source: [Hangfire Cookbook ASP.NET Core Azure — DEV Community](https://dev.to/mikaelkrief2/the-hangfire-cookbook-a-practical-guide-to-background-job-processing-in-net-and-azure-4mg0), [ASP.NET Core Background Jobs — BoldSign](https://boldsign.com/blogs/aspnet-core-background-jobs-hosted-services-hangfire-quartz/)_

---

## Scalability and Performance Patterns

**Caching strategy (3-tier):**

| Cache Level | Technology | What It Caches | TTL |
|---|---|---|---|
| In-process | `IMemoryCache` | User session data, consent status | 5 minutes |
| Distributed | Redis (Azure Cache) | Bhagavad Gita verses, group member lists | 1 hour |
| HTTP response | `OutputCache` (.NET 10) | Public API responses (non-personal) | 15 minutes |

**India-specific performance considerations:**
- Mobile networks: 4G average 25 Mbps; 3G Tier 2/3 cities 5 Mbps — compress all API responses (`UseResponseCompression`)
- Paginate all list endpoints (contacts, groups, interaction history) — default page size 20
- Lazy-load AI insights on contact detail view — don't include in contact list response
- Flutter client: implement local SQLite cache (using `sqflite`) for offline contact viewing
- Image optimisation: compress contact photos to 200×200px before storage; serve via Azure CDN

**Database performance:**
- `user_id` as leading column on all composite indexes (matches RLS policy evaluation)
- EF Core: use `AsNoTracking()` for all read-only queries
- Dapper for complex reporting queries (AI insight aggregations, relationship health reports)
- Connection pooling: PgBouncer in transaction mode for high-concurrency scenarios

_Source: [DPDP Compliance Checklist 2026 — DPDPA.com](https://www.dpdpa.com/blogs/dpdpa_compliance_checklist_2026_business_assessment.html), [AWS Multi-Tenant RLS PostgreSQL — AWS Blog](https://aws.amazon.com/blogs/database/multi-tenant-data-isolation-with-postgresql-row-level-security/)_

---

## Security Architecture Patterns

**Defence-in-depth layers:**

1. **Network**: Azure WAF (Web Application Firewall) in front of App Service; HTTPS enforced; HSTS preload
2. **Authentication**: ASP.NET Core Identity + JWT (15-minute access tokens, 7-day refresh tokens)
3. **Authorisation**: Resource-based authorisation (`IAuthorizationHandler`) — users can only access their own contacts
4. **Database**: PostgreSQL RLS — even a SQL injection attack cannot cross user boundaries
5. **Secrets**: Azure Key Vault for all API keys (Claude, WhatsApp, Razorpay); never in `appsettings.json`
6. **Audit**: Serilog → Azure Application Insights — all data access logged with user_id, timestamp, action

**DPDP breach notification architecture:**
```
Application Insights Alert Rule
  → Triggers when: unusual data access volume, auth failures spike, SQL errors
  → Action Group: emails DPO + creates Hangfire job
  → Hangfire job: within 72 hours sends structured report to DPBI notification endpoint
```

---

## Deployment and Operations Architecture

**Container-first deployment:**

```dockerfile
# MyLifeOS.Api/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyLifeOS.Api.dll"]
```

**Azure deployment pipeline (GitHub Actions → Azure):**
```
Push to main
  → Run tests (xUnit + Testcontainers)
  → Build Docker image
  → Push to Azure Container Registry
  → Deploy to Azure App Service (Central India)
  → Run EF Core migrations (via startup migration)
  → Health check endpoint confirms deployment
```

**Zero-downtime deployment:** Azure App Service deployment slots (staging → production swap); EF Core migrations must be backwards-compatible (additive only — never drop columns in same deployment as code change).

_Source: [Hangfire Background Jobs .NET — Hangfire.io](https://www.hangfire.io/), [Azure Background Jobs Guide — DevelopersVoice](https://developersvoice.com/blog/azure/azure-background-jobs-architectural-guide/)_
