# Technology Stack Analysis

## Programming Languages and Runtime

**Primary: C# / .NET 10**

The existing MyContacts codebase is ASP.NET Core MVC (.NET 10 / `net10.0`). C# and .NET 10 remain the correct choice for the Life OS backend:
- .NET 10 is the current LTS-track release with major performance improvements (HybridCache, minimal API enhancements, OpenAPI 3.1 support)
- C# 13 brings primary constructors, collection expressions, and improved async patterns — reducing boilerplate for entity models, request/response DTOs, and service classes
- The existing ASP.NET Core skillset transfers directly; no language migration risk

**Mobile Frontend Decision: Flutter (Recommended over .NET MAUI)**

| Framework | Pros | Cons | India Suitability |
|---|---|---|---|
| **.NET MAUI** | Native Azure/ASP.NET Core integration; first-party .NET support | Smaller community; fewer India developers; enterprise-focused UX | 🟡 Good for .NET shops |
| **Flutter** | 🔴 Best India developer pool; Google-backed; excellent Material Design; Firebase integration | Dart language learning curve | 🔴 **Recommended for India consumer app** |
| **React Native** | Largest JS ecosystem; web team reuse | Performance gap vs native; Meta dependency | 🟡 Good for web teams |

**Recommendation: Flutter** for the mobile front-end. India has the second-largest Flutter developer community globally. Flutter's Material Design 3 components match user expectations for Indian consumer apps. The ASP.NET Core backend exposes REST APIs consumed by Flutter — no tight coupling to the frontend framework.

_Source: [.NET MAUI vs Flutter 2026 — DigiSoft](https://www.digisoftsolution.com/blog/dotnet-maui-vs-flutter), [Cross-Platform Frameworks 2025 — Appisto](https://appisto.app/blog/cross-platform-mobile-frameworks)_

---

## Development Frameworks and Architecture Pattern

**Backend: Modular Monolith + Clean Architecture + CQRS**

The 2026 .NET consensus has moved decisively away from "microservices for everything" toward the **modular monolith** as the correct default for product teams:

- Fewer than 3 independent deployment teams → microservices are not warranted
- Running 15 microservices costs **5–10× more** than one modular monolith at equivalent traffic
- Modular monolith: strong module boundaries enforced at compile time, single deployment, single database connection, no distributed system complexity
- Boundaries can be extracted into microservices later if/when specific modules (e.g., AI coaching, payment processing) need independent scaling

**Recommended architecture for the Life OS backend:**

```
MyContacts.sln
├── MyLifeOS.Api          ← ASP.NET Core minimal API (entry point, no business logic)
├── MyLifeOS.Contacts     ← Contacts module (domain + infrastructure)
├── MyLifeOS.Community    ← Groups, Satsang, spiritual community module
├── MyLifeOS.Growth       ← Career, financial, wellness dimensions module
├── MyLifeOS.AI           ← Claude API, Whisper integration module
├── MyLifeOS.Notifications← WhatsApp, push, reminders module
├── MyLifeOS.Identity     ← Auth, consent management, DPDP compliance module
└── MyLifeOS.Shared       ← Shared kernel (domain events, base entities, Result types)
```

**Key patterns:**
- **CQRS with MediatR**: Commands mutate state; queries read state — keeps API controllers thin (1-2 line handlers)
- **FluentValidation**: Declarative input validation on commands — critical for DPDP consent validation
- **Domain Events**: Modules communicate via events (ContactCreated → AI analyses relationship context → Notification schedules reminder)
- **Result<T> pattern**: No exceptions for expected failures (consent declined, API quota exceeded)

_Source: [Modular Monolith Architecture .NET 2026 — Milan Jovanović](https://www.milanjovanovic.tech/blog/modular-monolith-architecture-dotnet), [Modular Architecture ASP.NET Core — CodeWithMukesh](https://codewithmukesh.com/blog/modular-architecture-in-aspnet-core/)_

---

## Database and Storage Technologies

**Migration path: SQLite (dev) → PostgreSQL (production)**

| Stage | Database | Rationale |
|---|---|---|
| **Development** | SQLite (current) | Keep as-is; zero configuration; fast local iteration |
| **Production** | PostgreSQL 16 | Handles terabytes; connection pooling; JSONB for flexible schema |
| **Cache layer** | Redis (Azure Cache for Redis) | Session data, API response caching, rate limiting counters |
| **File storage** | Azure Blob Storage (India region) | Contact profile photos, voice recording files |

**EF Core migration: one configuration line change**

```csharp
// Development
optionsBuilder.UseSqlite("Data Source=mycontacts.db");

// Production (swap provider, keep all migrations)
optionsBuilder.UseNpgsql(connectionString);
```

All existing EF Core migrations work unchanged. The switch requires only the `Npgsql.EntityFrameworkCore.PostgreSQL` NuGet package and connection string update.

**PostgreSQL scaling path (when needed):**
1. Optimize single server (indexes, query tuning) — handles up to ~1M active users
2. Add connection pooling (PgBouncer) — handles connection storm from mobile clients
3. Read replicas — separate heavy read queries (AI analysis, reports) from writes
4. Table partitioning — contacts/interaction_logs tables partitioned by user_id

**Schema evolution considerations for Life OS:**

The existing `Contact` model (Id, Name, Phone, Email, Address, Notes) requires significant schema additions:
- `ContactCategory` (enum: Family, Friend, Colleague, CloseFriend, ChildhoodFriend, etc.)
- `ContactGroup` (many-to-many: contacts ↔ groups)
- `InteractionLog` (timestamp, channel, duration, notes, AI summary)
- `SatsangSession` (group, date, scripture_reference, reflection_notes)
- `ConsentRecord` (user_id, consent_type, granted_at, revoked_at, ip_address) — DPDP requirement
- `GrowthDimension` (user_id, dimension_type, goal, progress)

_Source: [Switching EF Core SQLite to PostgreSQL — Medium](https://didourebai.medium.com/switching-ef-core-from-sqlite-to-postgresql-a-complete-guide-for-net-developers-e4b3174243bf), [7 Ways to Scale PostgreSQL 2026 — VeloDB](https://www.velodb.io/glossary/ways-to-scale-postgresql)_

---

## Development Tools and Platforms

**Recommended toolchain:**

| Tool | Purpose | Notes |
|---|---|---|
| Visual Studio 2022 / VS Code | IDE | VS 2022 for full .NET debugging; VS Code + C# Dev Kit for lightweight editing |
| Docker Desktop | Local containerisation | Run PostgreSQL + Redis locally; matches production |
| Azure Developer CLI (azd) | Cloud deployment | One-command deploy to Azure India; built for .NET apps |
| GitHub Actions | CI/CD | Free for public repos; integrates with Azure deployment |
| Postman / Scalar | API testing | Scalar is the new .NET 10 default for OpenAPI UI |
| FluentMigrator / EF Core Migrations | DB versioning | EF Core Migrations sufficient for team of 1-3 |
| Serilog + Azure Application Insights | Logging/observability | Structured logging; critical for DPDP audit trail |

**Testing stack:**
- xUnit (unit tests), Testcontainers (integration tests with real PostgreSQL), Playwright (E2E for web)
- No mocking of the database — use Testcontainers for real DB integration tests

---

## Cloud Infrastructure and Deployment

**Primary recommendation: Azure India (Central India region, Pune)**

Azure is the natural choice given the ASP.NET Core + .NET ecosystem:

**Azure India infrastructure:**
- **Central India (Pune)**: Primary production region — Availability Zones for resilience; $3B Microsoft investment in 2026; 8–15ms latency to Mumbai
- **South India (Chennai)**: Paired region for geo-redundant disaster recovery
- **New Hyderabad region**: Coming 2026 — monitor for lower-cost options
- **DPDP compliance**: Azure India Pvt Ltd contracts with customers in INR; data residency in India by default — satisfies DPDP data localisation requirement out of the box

**Recommended Azure services for Life OS:**

```
Production Architecture
─────────────────────────────────────────────────────
Azure App Service (Central India)
  └── ASP.NET Core minimal API (containerised)
Azure Database for PostgreSQL Flexible Server (Central India)
  └── Automatic backups, read replica support
Azure Cache for Redis (Central India)
  └── Session cache, API response cache
Azure Blob Storage (Central India)
  └── Voice recordings, profile photos (DPDP: India-only)
Azure Notification Hubs
  └── Push notifications (Android FCM + iOS APNs)
Azure Application Insights
  └── Telemetry, performance monitoring, audit log
Azure API Management (optional, Phase 2)
  └── Rate limiting, API versioning, developer portal
─────────────────────────────────────────────────────
```

**Cost estimate (Phase 1, ~10K MAU):**
- App Service B2 (2 vCPU, 3.5GB): ~₹3,500/month
- PostgreSQL Flexible Server Burstable B2: ~₹2,800/month
- Redis Cache C0: ~₹1,400/month
- Blob Storage 100GB: ~₹200/month
- **Total Phase 1: ~₹8,000/month (~$95/month)**

_Source: [Azure India Deployment Guide 2026 — SiriusStar](https://siriusstar.in/cloud-solutions/microsoft-azure/), [Azure India Regions — Opsio](https://opsiocloud.com/in/knowledge-base/is-azure-available-in-india/)_

---

## Technology Adoption Trends

**1. Modular Monolith is the 2026 default** — microservices are no longer the default recommendation for new products. Start modular, extract when scale demands it.

**2. Minimal APIs replacing MVC for API endpoints** — ASP.NET Core minimal APIs are faster, less ceremony, and OpenAPI-native. The existing MVC views stay for any web interface; new mobile API endpoints use minimal API.

**3. Flutter dominates India mobile** — largest non-Google Flutter developer community; ideal hiring pool for Hyderabad/Bangalore/Pune engineering teams.

**4. PostgreSQL + EF Core is the standard .NET ORM stack** — Dapper for read-heavy queries where EF overhead matters; EF Core for CRUD and migrations.

**5. DPDP-ready architecture is a differentiator** — consent management, audit logging, and automated deletion are now architectural requirements for India-deployed apps, not afterthoughts. Building with these from Day 1 avoids costly rewrites.

_Source: [.NET Developer Roadmap 2026 — CodeWithMukesh](https://codewithmukesh.com/blog/dotnet-developer-roadmap/), [Monolith vs Modular vs Microservices .NET 2026 — CodingDroplets](https://codingdroplets.com/monolith-vs-modular-monolith-vs-microservices-dotnet-2026)_
