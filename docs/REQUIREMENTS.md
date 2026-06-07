# Project Requirements: HPMS

## 1. User Roles & Personas

| Role | Responsibilities |
|------|------------------|
| **System Admin** | Tenant creation, global configuration, system monitoring |
| **Clinic Admin** | Clinic settings, staff onboarding, financial dashboards |
| **Provider** | Personal schedule, patient demographics, completing visits |
| **Billing Manager** | Invoices, payments, ledger reconciliation |
| **Front Desk** | Booking appointments, check-in, copay collection |

Roles are seeded in the database (`HPMS.Modules.Identity`). JWT role claims and endpoint authorization policies are enforced on Identity, Scheduling, and Billing routes.

## 2. Core Use Cases

| ID | Use case | Primary actor |
|----|----------|---------------|
| **UC-01** | Schedule an appointment — select provider, patient, and time slot; validate availability and prevent double-booking | Front Desk |
| **UC-02** | Complete a visit — provider marks appointment **Completed**; system generates a draft invoice from the appointment rate | Provider |
| **UC-03** | Process payment — billing manager applies payment; invoice becomes **Paid** and a credit is recorded in the ledger | Billing Manager |

## 3. Functional Requirements (FR)

### Module: Scheduling

| ID | Requirement | Status |
|----|-------------|--------|
| **FR-S01** | Prevent double-booking for a single provider unless explicitly overridden by a Clinic Admin | Done |
| **FR-S02** | Appointments transition through `Scheduled` → `Arrived` → `InSession` → `Completed` or `NoShow` (also `Canceled`) | Done |

### Module: Billing

| ID | Requirement | Status |
|----|-------------|--------|
| **FR-B01** | Automatically generate a draft invoice when `AppointmentCompletedEvent` is detected | Done (default fee $150; no AppointmentType rates yet) |
| **FR-B02** | Maintain an immutable financial ledger; payments cannot be deleted, only offset by refund/void | Partial — ledger append-only in practice; refund/void not implemented |

### Module: Multi-Tenancy

| ID | Requirement | Status |
|----|-------------|--------|
| **FR-M01** | Associate every User, Patient, Appointment, and Invoice with a `TenantId` | Done — EF global query filters + `IHasTenant` |

## 4. Non-Functional Requirements (NFR)

| ID | Requirement | Status |
|----|-------------|--------|
| **NFR-01** | **Security & HIPAA:** JWT on API endpoints; BCrypt passwords; PHI encrypted at rest (AES-256); TLS in transit; audit log on clinical mutations | Partial — JWT/BCrypt/PHI encryption/RBAC done; audit log not implemented; encryption key in source |
| **NFR-02** | **Tenant isolation:** Clinic A cannot access Clinic B data, enforced at query level | Done — global filters + integration test |
| **NFR-03** | **Performance:** 99.9% uptime SLA; GET queries under 300ms | Not validated |

### NFR-01 detail

* All API endpoints **shall** require a valid JWT — *exception: identity onboarding/login endpoints today*
* Passwords **shall** be hashed (BCrypt/Argon2) — *BCrypt implemented*
* PHI **shall** be encrypted at rest (AES-256) and in transit (TLS 1.2+) — *PHI blob encrypted; TLS depends on deployment*
* Every clinical mutation **shall** produce an immutable audit log — *not implemented (Phase 5)*

## 5. Implementation Status

Summary as of June 2026. See [PLAN.md](PLAN.md) for phase-level detail.

| Area | Done | Remaining |
|------|------|-----------|
| Backend API | 18 endpoints across Identity, Scheduling, Billing | Audit logs, refund/void |
| Frontend | Login, signup, auth interceptor, dashboard, patients, appointments | Billing feature pages, auth guards on all private routes |
| Tests | Unit (conflict, tenant filters) + 3 integration tests | Broader coverage, Testcontainers |
| Docs | Requirements, design, ERM, API reference | Keep in sync with code changes |

Known gaps that affect requirement traceability:

1. Visit fee is hardcoded; no `AppointmentType` pricing table
2. Frontend dashboard not connected to scheduling/billing summary APIs
3. Audit log interceptor not implemented (Phase 5)
