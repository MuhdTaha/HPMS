# HPMS — Healthcare Practice Management System

A multi-tenant B2B SaaS platform for independent medical and behavioral health clinics. HPMS combines patient scheduling, clinical workflows, and billing in a single **modular monolith** (.NET 8 + Angular 21).

## Repository layout

| Path | Description |
|------|-------------|
| `HPMS.Web/` | ASP.NET Core host — composes all modules, JWT, Swagger, CORS |
| `HPMS.Modules.Identity/` | Tenants, users, roles, authentication |
| `HPMS.Scheduling/` | Patients, appointments, conflict detection |
| `HPMS.Modules.Billing/` | Invoices, ledger, event-driven billing |
| `HPMS.SharedKernel/` | Tenant interfaces, encryption, MediatR events, EF extensions |
| `HPMS.Tests.Unit/` | Unit tests (xUnit, Moq, EF InMemory) |
| `HPMS.Tests.Integration/` | Integration tests (`WebApplicationFactory`) |
| `hpms-ui/` | Angular frontend (PrimeNG) |
| `docs/` | Requirements, design, plan, ERM, API reference |

## Quick start

**Prerequisites:** .NET 8 SDK, SQL Server (LocalDB or Express), Node.js 20+

```powershell
# 1. Backend — update the connection string in HPMS.Web/appsettings.json first
dotnet run --project HPMS.Web --launch-profile http

# 2. Frontend (separate terminal)
cd hpms-ui
npm install
npm start
```

| Service | URL |
|---------|-----|
| API | http://localhost:5260 |
| Swagger | http://localhost:5260/swagger |
| Frontend | http://localhost:4200 |

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for full setup, configuration, and troubleshooting.

## Documentation

| Document | Purpose |
|----------|---------|
| [docs/README.md](docs/README.md) | Documentation index |
| [docs/DESCRIPTION.md](docs/DESCRIPTION.md) | Project overview and scope |
| [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md) | Functional and non-functional requirements |
| [docs/PLAN.md](docs/PLAN.md) | Five-phase development plan and progress |
| [docs/DESIGN.md](docs/DESIGN.md) | Architecture, patterns, and tech stack |
| [docs/ERM.md](docs/ERM.md) | Database schema (implemented and planned) |
| [docs/API.md](docs/API.md) | REST API reference |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | Local development guide |
| [hpms-ui/README.md](hpms-ui/README.md) | Frontend-specific notes |

## Current status (summary)

The backend MVP is largely functional: multi-tenant isolation, scheduling, event-driven invoice generation, and payment recording are implemented with integration tests. The Angular frontend has polished auth pages but the dashboard is not yet routed or wired to the API. See [docs/PLAN.md](docs/PLAN.md) for phase-by-phase progress and known gaps.

## CI

GitHub Actions (`.github/workflows/dotnet.yml`) builds the solution, applies Identity and Scheduling migrations against SQL Server, and runs tests on pushes to `main`.
