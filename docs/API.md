# API Reference

Base URL (local development): `http://localhost:5260`

Interactive documentation: **Swagger UI** at `/swagger` (Development environment only).

## Authentication

Most endpoints require a JWT in the `Authorization` header:

```
Authorization: Bearer {token}
```

Obtain a token via `POST /identity/login`. The token includes claims:

| Claim | Description |
|-------|-------------|
| `sub` | User ID |
| `unique_name` | Username |
| `TenantId` | Clinic tenant GUID |
| `role` | User role from database (`SystemAdmin`, `ClinicAdmin`, `Provider`, `BillingManager`, `FrontDesk`) |

The frontend also sends `X-Tenant-Id` from `localStorage`; tenant scoping is enforced server-side via JWT claims and EF global query filters.

### Public endpoints (no JWT required)

- `POST /identity/login`

### Role-protected identity endpoints

- `POST /identity/tenants` — **SystemAdmin** only
- `POST /identity/users` — **ClinicAdmin** or **SystemAdmin** (Clinic Admins may only create users in their own tenant and cannot assign the SystemAdmin role)
- `GET /identity/users` — **ClinicAdmin** or **SystemAdmin**

---

## Identity — `/identity`

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/identity/tenants?name={name}` | SystemAdmin | Create a new clinic tenant |
| POST | `/identity/users` | ClinicAdmin, SystemAdmin | Register a user (BCrypt password hash) |
| GET | `/identity/users` | ClinicAdmin, SystemAdmin | List users for current tenant |
| POST | `/identity/login` | No | Authenticate; returns `{ "token": "..." }` |

### POST /identity/users — request body

```json
{
  "tenantId": "guid",
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "password": "string",
  "roleId": 1
}
```

Seeded roles (`RoleId`): 1 SystemAdmin, 2 ClinicAdmin, 3 Provider, 4 BillingManager, 5 FrontDesk.

### POST /identity/login — request body

```json
{
  "username": "string",
  "password": "string",
  "rememberMe": false
}
```

Token lifetime: 8 hours (default) or ~30 days when `rememberMe` is true.

---

## Scheduling — `/scheduling`

All routes require JWT.

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/scheduling/appointments` | Create appointment (conflict check) |
| PATCH | `/scheduling/appointments/{id}/status` | Update status (state machine) |
| DELETE | `/scheduling/appointments/{id}` | Soft-delete appointment |
| GET | `/scheduling/appointments/{id}` | Get appointment by ID |
| GET | `/scheduling/summary/today` | Today's stats and check-in queue |
| POST | `/scheduling/patients` | Create patient (PHI encrypted at rest) |
| GET | `/scheduling/patients` | List patients (tenant-scoped) |
| DELETE | `/scheduling/patients/{id}` | Soft-delete patient |

### Appointment status transitions

Valid flow: `Scheduled` → `Arrived` → `InSession` → `Completed`

Also allowed: `Scheduled` → `NoShow`, `Scheduled` → `Canceled`

When status becomes `Completed`, the API publishes `AppointmentCompletedEvent` (MediatR), which triggers invoice creation in Billing.

### GET /scheduling/summary/today — response

```json
{
  "totalCount": 0,
  "arrivedCount": 0,
  "inSessionCount": 0,
  "completedCount": 0,
  "noShowCount": 0,
  "canceledCount": 0,
  "queue": [
    { "id": "guid", "patientId": "guid", "startTime": "2026-06-06T09:00:00Z", "status": 2 }
  ]
}
```

---

## Billing — `/billing`

All routes require JWT.

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/billing/invoices` | List invoices |
| GET | `/billing/ledger` | List ledger entries (newest first) |
| POST | `/billing/invoices/{id}/pay` | Mark invoice paid; adds credit ledger entry |
| GET | `/billing/summary/revenue` | Last 7 days revenue chart data |

### GET /billing/summary/revenue — response

```json
{
  "todayRevenue": 150.00,
  "chartLabels": ["06-01", "06-02"],
  "chartValues": [150.00, 300.00]
}
```

### Event-driven invoice creation

When an appointment is marked `Completed`:

1. `AppointmentCompletedEvent` is published with a default `VisitFee` of **$150.00** (hardcoded until AppointmentType pricing exists)
2. `CreateInvoiceOnAppointmentCompleted` handler creates a `Pending` invoice and a **Debit** ledger entry

---

## Error responses

| Status | Meaning |
|--------|---------|
| 401 | Missing or invalid JWT |
| 404 | Resource not found |
| 400 | Validation failure (e.g., scheduling conflict, invoice already paid) |
| 409 | Optimistic concurrency conflict on appointments (`RowVersion`) |

---

## Source files

| Module | Endpoint definitions |
|--------|---------------------|
| Identity | `HPMS.Modules.Identity/Endpoints/IdentityEndpoints.cs` |
| Scheduling | `HPMS.Scheduling/SchedulingModule.cs` |
| Billing | `HPMS.Modules.Billing/BillingModule.cs` |
| Host composition | `HPMS.Web/Program.cs` |
