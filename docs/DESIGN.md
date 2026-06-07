# Project Design & Architecture: HPMS

## 1. Architectural Style: Modular Monolith

HPMS is a **modular monolith**: one deployable ASP.NET Core application (`HPMS.Web`) with clear logical boundaries. Modules are separate projects but share a single process and database.

```
HPMS.Web (host)
├── HPMS.Modules.Identity   → /identity/*
├── HPMS.Scheduling         → /scheduling/*
├── HPMS.Modules.Billing    → /billing/*
└── HPMS.SharedKernel       → shared interfaces, events, encryption
```

**Rule:** Modules do not query another module's tables directly. Cross-module communication uses **MediatR domain events** (e.g., Scheduling → Billing via `AppointmentCompletedEvent`).

## 2. Solution Structure

| Project | Responsibility |
|---------|----------------|
| `HPMS.Web` | Host: DI, JWT, CORS, Swagger, endpoint registration, auto-migration |
| `HPMS.Modules.Identity` | Tenants, users, roles, login; `IdentityDbContext` |
| `HPMS.Scheduling` | Patients, appointments, conflict service; `SchedulingDbContext` |
| `HPMS.Modules.Billing` | Invoices, ledger, payment handler; `BillingDbContext` |
| `HPMS.SharedKernel` | `IHasTenant`, `ISoftDelete`, `ITenantProvider`, `EncryptionHelper`, EF extensions |
| `hpms-ui` | Angular SPA (separate folder, not in `HPMS.sln`) |

Each module owns its **DbContext and migrations**. All contexts use the same SQL Server connection string but maintain separate migration histories.

## 3. Key Design Patterns

### Event-driven communication (MediatR)

When Scheduling marks an appointment **Completed**, it publishes `AppointmentCompletedEvent`. Billing registers `CreateInvoiceOnAppointmentCompleted` as an `INotificationHandler` to create an invoice and debit ledger entry without Scheduling referencing Billing types directly.

### Multi-tenancy (global query filters)

Tenant isolation uses EF Core **global query filters** on `IHasTenant` entities:

1. `ClaimsTenantProvider` reads `TenantId` from the JWT
2. Each `DbContext` applies `WHERE TenantId = @currentTenant`
3. `StampTenantIds()` on `SaveChangesAsync` sets `TenantId` on new entities

Login uses `IgnoreQueryFilters()` because the tenant is not known until the user is found.

### Soft deletes

Entities implementing `ISoftDelete` set `IsDeleted = true` instead of physical deletion. Global filters exclude deleted rows from queries.

### PHI encryption

Patient contact/medical details are serialized to JSON in `PHI_Data`. A value converter in `SchedulingDbContext` encrypts/decrypts via `EncryptionHelper` (AES-256) at the EF layer.

## 4. Technology Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Angular 21, TypeScript, RxJS, SCSS, PrimeNG, PrimeIcons |
| **Backend** | C# / .NET 8, ASP.NET Core Minimal APIs |
| **Database** | Microsoft SQL Server |
| **ORM** | Entity Framework Core 8 |
| **Auth** | JWT Bearer, BCrypt password hashing |
| **Events** | MediatR |
| **API docs** | Swashbuckle (Swagger UI) |
| **Testing** | xUnit, Moq, FluentAssertions, EF InMemory; `WebApplicationFactory` for integration tests |

## 5. Frontend Architecture

```
hpms-ui/src/app/
├── core/
│   ├── config/api.config.ts      # API base URLs
│   ├── interceptors/             # JWT + X-Tenant-Id headers
│   └── services/                 # Auth, dashboard, toast
├── features/
│   ├── auth/                     # Login, signup
│   └── dashboard/                # Role-aware widgets (WIP)
└── app.routes.ts                 # Router config
```

API URLs default to `http://localhost:5260` with identity routes under `/identity`. CORS on the backend allows `http://localhost:4200`.

## 6. Tradeoffs & Decisions

### Monolith vs. microservices

**Decision:** Modular monolith.

**Rationale:** Microservices add network latency, distributed transactions, and operational complexity. A well-structured monolith delivers module separation with simpler deployment—appropriate for an MVP B2B SaaS.

### Angular vs. React

**Decision:** Angular.

**Rationale:** Angular's structure (modules, DI, RxJS) aligns with enterprise .NET patterns and suits data-heavy healthcare UIs.

### Three DbContexts vs. one

**Decision:** Separate contexts per module.

**Rationale:** Each module owns its schema evolution (independent migrations) while sharing one database. Avoids a single giant `DbContext` and keeps module boundaries explicit.

### Soft deletes vs. hard deletes

**Decision:** Soft deletes (`IsDeleted = true`).

**Rationale:** Healthcare and billing records require audit trails; physical deletion violates compliance expectations.

## 7. Known design debt

| Item | Location | Notes |
|------|----------|-------|
| Hardcoded JWT role | `IdentityEndpoints.cs` | Should read from `User.Role` |
| Hardcoded visit fee | `AppointmentCompletedEvent` | Replace with AppointmentType rates |
| Hardcoded AES key | `EncryptionHelper.cs` | Move to secure configuration |
| Open onboarding endpoints | `/identity/tenants`, `/identity/users` | Require System Admin auth |
| Module scaffold `Program.cs` files | Identity, Scheduling | Not used; host is `HPMS.Web` |

See [PLAN.md](PLAN.md) for remediation timeline.
