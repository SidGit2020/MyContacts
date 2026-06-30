# Implementation Approaches and Technology Adoption

## Migration Strategy: MyContacts MVC → Life OS Modular Monolith

**The golden rule: surgical, not sweeping. Migrate one module at a time.**

The existing MyContacts app (ASP.NET Core MVC, SQLite, single `Contact` entity) is the foundation. The migration path preserves all working code while incrementally extracting modules and adding the Life OS architecture.

**Phase 0 — Restructure in place (Week 1–2, no feature changes)**

```
Before:                          After:
MyContacts/                      MyLifeOS/
├── Controllers/                 ├── MyLifeOS.Api/          ← entry point
│   └── ContactsController.cs   ├── MyLifeOS.Contacts/     ← moved here
├── Models/Contact.cs           ├── MyLifeOS.Identity/     ← new: auth + DPDP
├── Views/                      ├── MyLifeOS.Shared/       ← new: base entities
└── Data/AppDbContext.cs        └── MyLifeOS.Api/Views/     ← keep MVC views
```

Steps:
1. Create solution with 3 initial projects: `.Api`, `.Contacts`, `.Shared`
2. Move `Contact.cs` and `AppDbContext` into `.Contacts` module
3. Keep MVC controllers and Views working — no behaviour change
4. Add Identity module with ASP.NET Core Identity + JWT for mobile API auth
5. Add DPDP consent tables (additive-only migration — existing data untouched)

**Phase 1 — Parallel Minimal API layer (Week 3–6)**

ASP.NET Core supports running MVC and Minimal APIs in the same app simultaneously:

```csharp
// Program.cs — both patterns active at the same time
app.MapControllers();          // existing MVC routes still work
app.MapContactsEndpoints();    // new minimal API for Flutter mobile client
```

Strategy:
- New mobile API endpoints are Minimal APIs from Day 1
- Existing MVC web views stay as MVC until a web rewrite is warranted
- Run integration tests on both routes to verify identical behaviour before deprecating MVC endpoint

**Rule:** Never delete MVC controller until integration test confirms Minimal API endpoint produces identical response.

_Source: [Minimal APIs vs MVC Controllers ASP.NET Core — DevelopersVoice](https://developersvoice.com/blog/dotnet/choosing-minimal-api-or-controllers/), [Beyond MVC 5 — Devox Software](https://devoxsoftware.com/blog/beyond-mvc-5-the-2025-playbook-for-elevating-asp-net-applications/)_

---

## Database Migration: SQLite → PostgreSQL

**Zero-downtime migration using EF Core Migration Bundles**

The migration is a two-step process: schema migration (EF Core Bundles) + data migration (one-time export/import):

**Step 1 — Add PostgreSQL provider alongside SQLite (development)**

```csharp
// appsettings.Development.json
{ "Database": "sqlite" }

// appsettings.Production.json
{ "Database": "postgresql" }

// Program.cs
if (config["Database"] == "postgresql")
    options.UseNpgsql(config.GetConnectionString("PostgreSQL"));
else
    options.UseSqlite("Data Source=mycontacts.db");
```

**Step 2 — Generate Migration Bundle for CI/CD**

```bash
# Generate a self-contained executable — no .NET SDK needed on deployment server
dotnet ef migrations bundle --configuration Release --output ./migrate-bundle

# Deploy: run bundle before app starts
./migrate-bundle --connection "Host=...;Database=...;Username=...;Password=..."
```

**EF Core Migration Bundles** (introduced v6, standard in v10) are the recommended production deployment approach — a self-contained executable applies pending migrations with no SDK or source code on the server.

**Zero-downtime rules:**
- Migrations must be **additive only** in the same deployment as code changes — never drop a column in the same PR as the code that stops reading it
- PostgreSQL supports `CREATE INDEX CONCURRENTLY` — no table lock; use this for adding indexes to large tables
- Add nullable columns first, populate with defaults, then add NOT NULL constraint in a later migration

**Data migration (one-time, from SQLite to PostgreSQL):**

```bash
# Export SQLite data
sqlite3 mycontacts.db .dump > contacts_dump.sql

# Import to PostgreSQL (after schema migration)
psql $PG_CONN < contacts_dump.sql
```

_Source: [Zero-Downtime Database Migrations EF Core — Medium](https://medium.com/@kittikawin_ball/zero-downtime-database-migrations-with-ef-core-d9fcff7e74aa), [Running Migrations EF Core 10 — CodeWithMukesh](https://codewithmukesh.com/blog/running-migrations-efcore/)_

---

## Testing and Quality Assurance

**Three-layer test strategy:**

| Layer | Tool | What It Tests | Run When |
|---|---|---|---|
| **Unit tests** | xUnit + FluentAssertions | Domain logic, consent validation, result patterns | Every PR |
| **Integration tests** | xUnit + Testcontainers + `WebApplicationFactory` | API endpoints + real PostgreSQL | Every PR |
| **Contract tests** | xUnit + HTTP client | API response shape matches Flutter expectations | Before mobile release |

**Integration test pattern (Testcontainers + real PostgreSQL):**

```csharp
public class ContactsApiTests : IAsyncLifetime {
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder().Build();
    private HttpClient _client;

    public async Task InitializeAsync() {
        await _db.StartAsync();
        // WebApplicationFactory boots real app against real PostgreSQL container
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.ConfigureServices(svc =>
                svc.AddDbContext<AppDbContext>(o =>
                    o.UseNpgsql(_db.GetConnectionString()))));
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateContact_WithValidConsent_Returns201() {
        var response = await _client.PostAsJsonAsync("/api/contacts",
            new { Name = "Raj", Phone = "+91-9876543210", ConsentGranted = true });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

**No database mocking** — all integration tests use Testcontainers with a real PostgreSQL container. This catches EF Core query issues, RLS policy problems, and migration failures that mocks would miss.

**Flutter testing (separate concern):**
- Flutter unit tests: `flutter test` for widget logic, state management, API model parsing
- Flutter integration tests: `flutter drive` against a local test backend
- API contract: generate OpenAPI spec from .NET (`dotnet openapi`) → generate Flutter client models with `openapi-generator`

_Source: [ASP.NET Core Testcontainers — testcontainers.com](https://testcontainers.com/guides/testing-an-aspnet-core-web-app/), [Integration Testing ASP.NET Core — dotnet.testcontainers.org](https://dotnet.testcontainers.org/examples/aspnet/)_

---

## Development Workflows and Tooling

**CI/CD pipeline (GitHub Actions → Azure):**

```yaml
# .github/workflows/deploy.yml
jobs:
  test:
    steps:
      - dotnet test --configuration Release    # xUnit + Testcontainers
  
  build:
    needs: test
    steps:
      - docker build -t mylifeos-api .
      - docker push $ACR/mylifeos-api:$SHA     # Azure Container Registry

  deploy:
    needs: build
    steps:
      - ./migrate-bundle --connection ${{ secrets.PG_CONN }}   # EF Core bundle
      - az webapp deploy --image $ACR/mylifeos-api:$SHA        # Azure App Service slot swap
```

**Local development setup (Docker Compose):**

```yaml
# docker-compose.yml
services:
  db:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: dev_password
      POSTGRES_DB: mylifeos_dev
  redis:
    image: redis:7-alpine
  whisper:
    build: ./whisper-service     # Faster-Whisper FastAPI container
  api:
    build: .
    depends_on: [db, redis, whisper]
    environment:
      Database: postgresql
      ConnectionStrings__PostgreSQL: "Host=db;Database=mylifeos_dev;Username=postgres;Password=dev_password"
```

Single `docker compose up` starts the full local stack including real PostgreSQL, Redis, and Whisper service.

---

## Team Organisation and Skills

**Phase 1 team (solo / 2-person):**

| Role | Skills Required | Gap from Current MyContacts |
|---|---|---|
| Backend | C# .NET 10, EF Core, minimal APIs, Hangfire | Module refactoring, consent architecture |
| Mobile | Flutter/Dart | New skill — 2–4 weeks onboarding for .NET developer |
| AI integration | Anthropic .NET SDK, prompt engineering | New — 1 week to build first Claude integration |
| DevOps | GitHub Actions, Azure CLI, Docker | Docker + Azure setup is 1–2 days |
| DPDP compliance | Consent schema design | Covered by architecture decisions in this document |

**Skill development priority:**
1. Flutter basics (most impactful new skill — unlocks mobile distribution)
2. Modular architecture patterns (MediatR, CQRS with MediatR)
3. Claude API prompt engineering (the quality of AI features depends on this)
4. Testcontainers integration testing (replaces manual testing)

---

## Cost Optimisation and Resource Management

**Phase 1 infrastructure cost (~10K MAU):**

| Service | SKU | Monthly Cost (INR) |
|---|---|---|
| Azure App Service | B2 (2 vCPU, 3.5GB) | ~₹3,500 |
| PostgreSQL Flexible Server | Burstable B2ms | ~₹2,800 |
| Azure Cache for Redis | C0 (Basic, 250MB) | ~₹1,400 |
| Azure Blob Storage | 100GB LRS | ~₹200 |
| Azure Application Insights | Free tier (5GB/month) | ₹0 |
| Azure Container Registry | Basic | ~₹400 |
| **Total Phase 1** | | **~₹8,300/month (~$100)** |

**Scale trigger points:**
- 10K MAU → current setup handles comfortably
- 50K MAU → upgrade to B4ms PostgreSQL + Standard Redis; ~₹18,000/month
- 200K MAU → Add read replica + PgBouncer; ~₹35,000/month
- 1M MAU → Extract AI module to separate App Service; ~₹80,000/month

**Cost optimisation levers:**
- Self-host Whisper (no AssemblyAI cost) — saves ~₹3/hour of transcription at scale
- Use Bhagavad Gita static JSON (no API call cost) — saves API latency + potential cost
- Redis caching reduces PostgreSQL query load — extend cache TTL as data size grows

---

## Risk Assessment and Mitigation

| Risk | Severity | Mitigation |
|---|---|---|
| Flutter learning curve delays mobile launch | 🟡 Medium | Start with PWA (Progressive Web App) via Blazor/MAUI Hybrid as interim; ship mobile later |
| Claude API quota / rate limiting | 🟡 Medium | Cache common coaching responses; implement circuit breaker; queue non-urgent AI jobs |
| DPDP Phase 3 enforcement (May 2027) catches consent gaps | 🔴 High | Build consent schema in Phase 0 (Week 1); never ship personal data features without consent gate |
| EF Core migration failure in production | 🟡 Medium | Test migration bundle in staging with production data copy before deploying to prod |
| Whisper latency unacceptable on low-spec server | 🟡 Medium | Use `small` model first (faster); upgrade to `medium` when server specs allow; AssemblyAI as fallback |
| SQLite data loss during PostgreSQL migration | 🔴 High | Always backup SQLite file; run migration on copy; keep SQLite as emergency fallback for 30 days |
| WhatsApp template message rejection | 🟡 Medium | Pre-submit templates for review 2 weeks before launch; start with free-form messages during 24h session window |
