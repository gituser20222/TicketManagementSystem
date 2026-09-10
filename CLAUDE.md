# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Full-stack ticket management app: ASP.NET Core (.NET 10) Web API + EF Core/PostgreSQL backend, React/TypeScript (Vite) frontend, JWT auth. Portfolio/learning project — see `README.md` for the full feature list, Docker Compose usage, and the `kind`+Helm local Kubernetes deployment.

## Commands

### Backend (`TicketManagement.Api`)

```bash
dotnet restore
dotnet build
dotnet test                                    # run all backend tests (TicketManagement.Api.Tests)
dotnet test --filter FullyQualifiedName~TicketServiceTests   # single test class
dotnet ef database update                      # apply EF Core migrations
```

Solution file is `TicketManagement.slnx` (the newer XML solution format, not `.sln`).

### Frontend (`TicketManagement.Ui`)

```bash
npm ci
npm run dev       # Vite dev server
npm run build     # tsc -b && vite build
npm run lint
npm test          # vitest, watch mode
npm test -- --run # vitest, single run (used in CI)
```

### Full stack (Docker Compose)

```bash
docker compose up -d
docker compose logs -f api
docker compose up -d --force-recreate api   # after rebuilding the API image
```

Requires `.env` (copy from `.env.example`) with `POSTGRES_PASSWORD` and `JWT_KEY`. Never commit `.env`.

### Local Kubernetes (kind + Helm)

See the "Kubernetes (kind + Helm)" section in `README.md` for the full cluster create / build+load images / `helm install` / port-forward / teardown sequence. Chart lives at `helm/ticket-management/`; cluster config at `kind-config.yaml`.

## Architecture

**Backend namespace layout doesn't mirror folder structure for auth**: `Services/AuthService.cs` and `Services/IAuthService.cs` are namespaced `TicketManagement.Api.Services.Auth` (not `TicketManagement.Api.Services`), matching `DTOs/Auth/*` → `TicketManagement.Api.DTOs.Auth`. Ticket-related services (`TicketService`, `ITicketService`) stay in the plain `TicketManagement.Api.Services` namespace. Keep this convention when adding new services rather than "fixing" the folder/namespace mismatch.

**Auth flow**: JWT bearer auth configured in `Program.cs` (issuer/audience/signing key from config → `Jwt:Issuer`/`Jwt:Audience`/`Jwt:Key`, backed by `JWT_KEY` env var). `AuthController` + `AuthService` issue tokens; `TicketsController` reads the caller's identity via `ClaimsPrincipal` for ownership/role checks (Admin role required to edit/delete tickets — see `TicketsController.cs`).

**Data layer**: single `TicketManagementDbContext` (`Data/`), EF Core + Npgsql provider, connection string from `ConnectionStrings:DefaultConnection`. Migrations live in `Migrations/`; the DB host resolves differently per environment (`postgres` service name in Docker Compose / Helm's `<release>-postgres` in Kubernetes — see `README.md` and `helm/ticket-management/templates/api-deployment.yaml`).

**Ticket status enum**: `TicketStatus.InProgress` serializes to JSON as `"In Progress"` via `JsonStringEnumMemberName` — keep the C# identifier and the wire value intentionally different when touching this enum.

**Deployment configs are layered, not redundant** — `docker-compose.yml` (local dev, `.env`-driven) and `helm/ticket-management/` (kind cluster, `values.yaml`/`--set`-driven) both stand up the same three services (api, ui, postgres) independently; there's no shared templating between them, so a config change (e.g. a new env var) needs updating in both places.

**CI/CD** (`.github/workflows/ci.yml`, single file, two jobs): `build-and-test` runs on every push to `main`. `deploy-to-kind` runs only on manual `workflow_dispatch` and only after `build-and-test` passes (`needs:`) — it spins up an ephemeral `kind` cluster on the runner and `helm upgrade --install`s the chart, reading `POSTGRES_PASSWORD`/`JWT_KEY` from GitHub Actions repository secrets (never from `.env`, which the runner never sees).
