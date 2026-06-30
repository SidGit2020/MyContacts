# Technical Research Recommendations

## Implementation Roadmap

```
Week 1–2   Phase 0: Restructure
           ├── Create modular solution structure
           ├── Add Identity + JWT auth
           ├── Add DPDP consent schema (EF Core migration)
           └── Add Testcontainers integration test harness

Week 3–6   Phase 1: Core Life OS (backend)
           ├── Minimal API endpoints for contacts + groups
           ├── Claude API integration (verse reflection + contact insight)
           ├── Bhagavad Gita JSON self-hosted
           ├── Hangfire for async AI jobs
           └── SQLite → PostgreSQL migration (staging first)

Week 7–12  Phase 1: Mobile (Flutter)
           ├── Flutter app: contacts + groups screens
           ├── WhatsApp Cloud API integration
           ├── Push notifications (Firebase/Azure Notification Hubs)
           └── Deploy to Azure Central India

Month 4–6  Phase 2: Intelligence Layer
           ├── Whisper microservice (voice-to-notes)
           ├── Claude persistent memory (Dreaming API when available)
           ├── Razorpay UPI (premium subscriptions)
           ├── Community: Satsang group management
           └── Interaction timeline + AI relationship health

Month 7–12 Phase 3: Ecosystem
           ├── Travel booking (IRCTC aggregator)
           ├── LinkedIn API sync (career network)
           ├── Financial knowledge groups
           └── B2B: corporate wellness / temple admin portals
```

## Technology Stack Recommendations

| Layer | Technology | Rationale |
|---|---|---|
| Backend API | ASP.NET Core .NET 10 Minimal API | Existing team skill; high performance; .NET ecosystem |
| Architecture | Modular Monolith + Clean Architecture + CQRS (MediatR) | Right-sized for 1–5 engineers; no distributed system complexity |
| Database | PostgreSQL 16 (production) / SQLite (dev) | Full EF Core support; RLS for data isolation; scales to 1M+ users |
| Cache | Redis (Azure Cache) | Session, API response, group member lists |
| Background jobs | Hangfire + PostgreSQL storage | AI async processing, notifications, DPDP deletion |
| Mobile | Flutter 3.x | Best India developer community; Material 3 design |
| AI | Anthropic `Anthropic` NuGet SDK (official) | Official SDK; streaming; IChatClient integration |
| Voice | Faster-Whisper + FastAPI (self-hosted) | DPDP compliant; multilingual; no external data transfer |
| Payments | Razorpay (REST API + HttpClient) | RBI-authorised; 0% MDR UPI; PCI DSS L1 |
| Messaging | WhatsApp Business Cloud API (NuGet wrapper) | 500M India users; free Meta Cloud hosting |
| Auth | ASP.NET Core Identity + JWT | Standard .NET approach; DPDP-aligned consent management |
| Cloud | Azure App Service + Azure DB for PostgreSQL (Central India) | DPDP data localisation; $3B India investment; INR billing |
| CI/CD | GitHub Actions + Azure Developer CLI | Free for public repos; first-class Azure integration |
| Observability | Serilog + Azure Application Insights | Structured logging; DPDP audit trail; alerting |

## Skill Development Requirements

1. **Flutter/Dart** — highest priority new skill; 2–4 weeks to build first screens
2. **MediatR + CQRS patterns** — 1 week; transforms code organisation quality
3. **Claude API prompt engineering** — 1 week; determines AI feature quality
4. **Testcontainers** — 2 days; replaces manual database testing
5. **Azure CLI + azd** — 1 day; deploy to Azure Central India from terminal

## Success Metrics and KPIs

| Metric | Phase 1 Target | Phase 2 Target |
|---|---|---|
| API response time (P95) | < 200ms | < 150ms |
| AI insight generation time | < 5s (async) | < 3s |
| Voice transcription latency | < 5s per minute of audio | < 3s |
| DPDP consent coverage | 100% (all data access gated) | 100% |
| Test coverage (integration) | > 80% of API endpoints | > 90% |
| Monthly infra cost | < ₹10,000 | < ₹20,000 |
| Mobile crash rate | < 0.1% | < 0.05% |

_Source: [ASP.NET Core Migration Minimal API — DevelopersVoice](https://developersvoice.com/blog/dotnet/choosing-minimal-api-or-controllers/), [EF Core Migration Bundles — Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)_

---
