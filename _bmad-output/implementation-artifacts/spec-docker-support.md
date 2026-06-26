---
title: 'Docker Support for MyContacts'
type: 'feature'
created: '2026-06-26'
status: 'done'
baseline_commit: 'c6442f9'
context: []
---

<frozen-after-approval reason="human-owned intent — do not modify unless human renegotiates">

## Intent

**Problem:** The MyContacts app runs only on the local machine; there is no way to containerize or host it portably.

**Approach:** Add a multi-stage Dockerfile (SDK build → ASP.NET runtime), a docker-compose.yml that maps port 8080 and mounts a named volume for SQLite persistence, and a .dockerignore to keep the build context lean.

## Boundaries & Constraints

**Always:**
- .NET 10.0 base images (`mcr.microsoft.com/dotnet/sdk:10.0` for build, `mcr.microsoft.com/dotnet/aspnet:10.0` for runtime)
- App listens on port 8080 inside the container (ASP.NET Core Docker default)
- SQLite file stored at `/app/data/contacts.db` inside the container
- Named Docker volume `mycontacts-data` mounted at `/app/data` for persistence
- Connection string overridden via environment variable in docker-compose: `ConnectionStrings__DefaultConnection=Data Source=/app/data/contacts.db`
- Local dev (`dotnet run`) continues using `contacts.db` in CWD — no change to `appsettings.json`

**Ask First:**
- Any change to the exposed port other than 8080
- Publishing to a container registry

**Never:**
- Multi-container orchestration (no database server, no nginx reverse proxy)
- HTTPS termination inside the container
- Changes to application source code

## I/O & Edge-Case Matrix

| Scenario | Input / State | Expected Output / Behavior | Error Handling |
|----------|--------------|---------------------------|----------------|
| Build image | `docker build -t mycontacts .` | Image builds successfully, no errors | Build fails fast with clear layer error |
| Start with compose | `docker-compose up` | App accessible at http://localhost:8080/Contacts | Container logs show startup error if port in use |
| Data persists | Create a contact, restart container | Contact still present after `docker-compose restart` | N/A |
| Clean build context | Docker build | `bin/`, `obj/`, `.git/` excluded via .dockerignore | N/A |

</frozen-after-approval>

## Code Map

- `Dockerfile` -- multi-stage: sdk:10.0 builds, aspnet:10.0 runs; exposes 8080
- `docker-compose.yml` -- service definition: port 8080:8080, env var for connection string, named volume
- `.dockerignore` -- excludes bin/, obj/, .git/, _bmad-output/, *.db

## Tasks & Acceptance

**Execution:**
- [x] `Dockerfile` -- create multi-stage build: stage 1 restores+publishes with sdk:10.0, stage 2 copies publish output into aspnet:10.0, sets WORKDIR /app, EXPOSE 8080, ENTRYPOINT dotnet MyContacts.dll
- [x] `docker-compose.yml` -- create service `web` using image built from Dockerfile; ports 8080:8080; environment variable `ConnectionStrings__DefaultConnection=Data Source=/app/data/contacts.db`; volume `mycontacts-data:/app/data`; restart: unless-stopped
- [x] `.dockerignore` -- exclude bin/, obj/, .git/, _bmad-output/, *.db, .vs/, .claude/

**Acceptance Criteria:**
- Given the project root, when `docker build -t mycontacts .` runs, then it completes with exit code 0
- Given `docker-compose up`, when app starts, then `http://localhost:8080/Contacts` returns HTTP 200 with the contacts list
- Given a contact created while the container runs, when `docker-compose restart` is run, then the contact is still present
- Given `.dockerignore` in place, when Docker sends build context, then bin/ obj/ .git/ are not included

## Spec Change Log

## Design Notes

- ASP.NET Core 8+ automatically listens on port 8080 (not 5000) when running inside a container via the `ASPNETCORE_HTTP_PORTS` default. No `ENV` override needed.
- The `--no-restore` flag is used in the publish stage since restore already ran in the build stage, keeping layers efficient.
- Volume is named (`mycontacts-data`) not bind-mounted so it works cross-platform without path issues.

## Verification

**Commands:**
- `docker build -t mycontacts .` -- expected: `Successfully built` with no errors
- `docker-compose up -d` -- expected: container starts, `docker ps` shows it running
- `curl http://localhost:8080/Contacts` (or browser) -- expected: HTML response with contacts page

## Suggested Review Order

**Entry Point — Multi-Stage Build**

- Build stage: restore then publish in two cached layers
  [`Dockerfile:2`](../../Dockerfile#L2)

- Runtime stage: explicit port env, data dir creation, entrypoint
  [`Dockerfile:11`](../../Dockerfile#L11)

**Compose — Service Wiring**

- Port mapping, connection string env override, named volume mount
  [`docker-compose.yml:4`](../../docker-compose.yml#L4)

- Named volume declaration for SQLite persistence
  [`docker-compose.yml:13`](../../docker-compose.yml#L13)

**Build Context — What Gets Excluded**

- .dockerignore: excludes bin/obj/.git, dev config, and Dockerfile itself
  [`.dockerignore:1`](../../.dockerignore#L1)
