# Technical Research Synthesis

## Cross-Domain Technical Insights

Seven architectural decisions that only become clear when all five research areas are read together:

**1. The modular monolith solves three problems simultaneously**
It gives module isolation (each domain owns its schema prefix and DbContext), async decoupling (MediatR domain events replace synchronous cross-module calls), and a clear microservices extraction path (when one module genuinely needs independent scaling). The cost advantage (5–10× cheaper than microservices at Phase 1 scale) is a bonus, not the reason to choose it.

**2. DPDP compliance and good architecture are the same thing here**
The consent middleware, append-only audit log, RLS data isolation, and Hangfire deletion workflow are not compliance overhead bolted onto the architecture — they are the architecture. A system designed correctly for DPDP is also a system designed correctly for multi-user data security in general.

**3. The official Claude .NET SDK removes the last integration risk**
Until 2026, integrating Claude into a .NET backend required either a community SDK or raw HTTP calls. The official `Anthropic` NuGet package (2026) changes this — first-class .NET support, streaming, IChatClient compatibility with `Microsoft.Extensions.AI`, and built-in retry/timeout handling. There is no longer a technical gap between "Claude is available" and "Claude works cleanly in ASP.NET Core."

**4. Flutter + ASP.NET Core minimal APIs = clean separation that scales**
The backend becomes entirely framework-agnostic for the frontend. OpenAPI 3.1 (built into .NET 10) generates the spec; `openapi-generator` builds Flutter client models. When the product evolves — web app, desktop, TV app — the backend never changes. This separation also enables independent mobile release cycles from backend deployments.

**5. `SET LOCAL` is the single most dangerous line of code in the system**
Using `SET` instead of `SET LOCAL` in the RLS middleware causes User A's PostgreSQL session variable to persist across connection pool reuse — meaning User B's next request reads User A's data. This is a data breach in a single wrong keyword. The research surfaced this as a production incident pattern; the fix is always `SET LOCAL`.

**6. EF Core Migration Bundles eliminate deployment environment dependencies**
The old approach (run `dotnet ef database update` on the server) requires the .NET SDK on production. Migration Bundles are a self-contained executable — no SDK, no source code, no CLI tools. This simplifies the CI/CD pipeline, reduces attack surface on production servers, and makes rollback reasoning cleaner.

**7. Self-hosting Whisper is the only DPDP-compliant voice transcription path**
Every cloud transcription service (AssemblyAI, Azure Speech, Google Speech) processes audio on external servers outside India. Under DPDP, voice recordings of personal conversations are sensitive personal data. Faster-Whisper running as a local FastAPI microservice means audio never leaves the Azure Central India boundary. The 1–5s latency for post-call transcription is acceptable; this is not a real-time feature.

---

## Architecture Decision Record Summary

| # | Decision | Chosen | Rejected | Rationale |
|---|---|---|---|---|
| ADR-1 | Backend architecture | Modular Monolith | Microservices | 5–10× cheaper; right-sized for 1–5 engineer team |
| ADR-2 | Mobile frontend | Flutter | .NET MAUI / React Native | Best India developer pool; Material 3; Google-backed |
| ADR-3 | Database (prod) | PostgreSQL 16 | SQL Server / MySQL | RLS support; open source; EF Core first-class |
| ADR-4 | AI SDK | Official `Anthropic` NuGet | Raw HTTP / community SDK | Official support; streaming; IChatClient integration |
| ADR-5 | Voice transcription | Faster-Whisper (self-hosted) | AssemblyAI / Azure Speech | DPDP compliance; audio stays in India; multilingual |
| ADR-6 | Background jobs | Hangfire + PostgreSQL storage | Azure Functions / Quartz | Simpler ops; dashboard UI; reuse existing DB |
| ADR-7 | Cloud region | Azure Central India (Pune) | AWS Mumbai | DPDP data residency; $3B investment; INR billing |
| ADR-8 | Consent design | Append-only table + SHA-256 hash | Updatable consent flags | DPDP tamper-proof audit requirement; legal defensibility |
| ADR-9 | API migration | Parallel MVC + Minimal API | Full rewrite | Zero risk; test equivalence before deprecating MVC |
| ADR-10 | DB migration | EF Core Migration Bundles | `dotnet ef` on server | No SDK on prod; CI/CD friendly; self-contained |

---

## Full Technology Reference Stack

```
MyLifeOS Technical Stack — Verified 2026-06-29
═══════════════════════════════════════════════════════════════

RUNTIME & LANGUAGE
  C# 13 / .NET 10                  — Backend language and runtime
  Dart / Flutter 3.x               — Mobile frontend

BACKEND FRAMEWORK
  ASP.NET Core .NET 10             — Web framework
  Minimal APIs                     — New mobile API endpoints
  MVC (retained)                   — Existing web views
  CQRS via MediatR                 — Command/query separation
  FluentValidation                 — Input validation

DATABASE & STORAGE
  PostgreSQL 16                    — Production database (Azure Flexible Server)
  SQLite                           — Development database
  EF Core (Npgsql provider)        — ORM + migrations
  EF Core Migration Bundles        — CI/CD deployment
  Redis (Azure Cache for Redis)    — Session + API response cache
  Azure Blob Storage               — Voice files, profile photos

AI & INTELLIGENCE
  Anthropic NuGet (official SDK)   — Claude API integration
  Faster-Whisper + FastAPI         — Voice-to-text microservice (self-hosted)
  Bhagavad Gita REST API           — Self-hosted JSON (open-source)

INTEGRATIONS
  WhatsappBusinessCloudApi NuGet   — WhatsApp Cloud API wrapper
  Razorpay REST API (HttpClient)   — UPI payment processing
  Google Calendar API              — Event sync (Phase 2)
  LinkedIn API                     — Career network sync (Phase 3)

ASYNC & BACKGROUND
  Hangfire + PostgreSQL storage    — Background job queue
  IHttpClientFactory + Polly       — Resilient outbound HTTP

SECURITY & COMPLIANCE
  ASP.NET Core Identity + JWT      — Authentication
  PostgreSQL Row-Level Security    — Database-level user isolation
  Azure Key Vault                  — Secret management
  ConsentMiddleware (custom)       — DPDP consent gate
  Serilog + Application Insights   — Audit logging + monitoring

CLOUD INFRASTRUCTURE (Azure Central India)
  Azure App Service                — Application hosting
  Azure Database for PostgreSQL    — Managed database
  Azure Cache for Redis            — Managed Redis
  Azure Blob Storage               — File storage
  Azure Application Insights       — Observability
  Azure Container Registry         — Docker image registry
  Azure Notification Hubs          — Push notifications

CI/CD & TOOLING
  GitHub Actions                   — CI/CD pipeline
  Docker + Docker Compose          — Local dev environment
  Testcontainers + xUnit           — Integration testing
  Azure Developer CLI (azd)        — Cloud deployment
```

---

## Technical Research Methodology and Sources

**Research methodology:** 12 parallel web searches across technology stack, integration patterns, architectural patterns, and implementation approaches. All critical claims multi-source verified. Sources dated 2025–2026 prioritised.

**Primary sources:**
- [ASP.NET Core Development Trends 2026 — NioTech](https://niotechone.com/blog/aspnet-core-development-trends-2026/)
- [Modular Monolith Architecture .NET — Milan Jovanović](https://www.milanjovanovic.tech/blog/modular-monolith-architecture-dotnet)
- [Official Claude C# SDK — platform.claude.com](https://platform.claude.com/docs/en/api/sdks/csharp)
- [WhatsApp Business Cloud API .NET — GitHub gabrieldwight](https://github.com/gabrieldwight/Whatsapp-Business-Cloud-Api-Net)
- [Switching EF Core SQLite to PostgreSQL — Medium](https://didourebai.medium.com/switching-ef-core-from-sqlite-to-postgresql-a-complete-guide-for-net-developers-e4b3174243bf)
- [PostgreSQL RLS Multi-Tenant — TechBuddies](https://www.techbuddies.io/2026/01/01/how-to-implement-postgresql-row-level-security-for-multi-tenant-saas/)
- [DPDP Consent Management 2026 — Digio](https://www.digio.in/blog/dpdp-consent-management-what-every-data-fiduciary-must-know-in-2026/)
- [Faster-Whisper — GitHub SYSTRAN](https://github.com/SYSTRAN/faster-whisper)
- [Razorpay UPI 2026 — Razorpay](https://razorpay.com/upi-payment-gateway-india/)
- [Azure India Regions 2026 — Opsio](https://opsiocloud.com/in/knowledge-base/is-azure-available-in-india/)
- [Zero-Downtime EF Core Migrations — Medium](https://medium.com/@kittikawin_ball/zero-downtime-database-migrations-with-ef-core-d9fcff7e74aa)
- [ASP.NET Core Testcontainers — testcontainers.com](https://testcontainers.com/guides/testing-an-aspnet-core-web-app/)

**Research limitations:**
- Claude "Dreaming" persistent memory API is in preview (May 2026) — GA timeline unconfirmed; architecture designed to add it without rework
- Flutter India developer community size is qualitative assessment; no hard survey data found for 2026
- Razorpay has no official .NET SDK; REST API approach verified as production-viable

---
