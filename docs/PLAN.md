# Project Development Plan: HPMS

## Executive Summary

HPMS development is organized into five phases: foundation and multi-tenancy first, then clinical workflows, event-driven billing, financial features, and finally compliance hardening. This ordering ensures tenant isolation and authentication exist before building on top of them.

---

## Phase 1: Foundation & Multi-Tenancy

**Goal:** Core infrastructure, tenant isolation, and identity management.

**Description:** Implement `HPMS.Modules.Identity` — tenants table, users, roles, and automated `TenantId` filtering.

**Key deliverables:**

* Database migrations for Tenants, Users, and Roles
* JWT authentication middleware
* EF Core global query filters for data isolation
* Tenant onboarding API (System Admin only)

**Progress:** Complete

| Deliverable | Status |
|-------------|--------|
| Migrations + seed data | Done |
| JWT middleware + Swagger | Done |
| Global query filters | Done |
| `ClaimsTenantProvider` | Done |
| Secure tenant onboarding (admin-only) | Done |
| RBAC on endpoints | Done |
| Correct role in JWT claims | Done |

---

## Phase 2: Scheduling & Patient Management

**Goal:** Primary clinical workflow for front-desk and provider staff.

**Description:** Implement `HPMS.Scheduling` — patient CRUD, appointment booking, status state machine.

**Key deliverables:**

* Patient demographics with AES-256 encryption for PHI
* Appointment booking with conflict detection (FR-S01)
* State machine: `Scheduled` → `Arrived` → `InSession` → `Completed`
* Soft-delete for clinical records

**Progress:** Complete

| Deliverable | Status |
|-------------|--------|
| Patient CRUD + PHI encryption | Done |
| Conflict detection service | Done |
| Status state machine + `Canceled`/`NoShow` | Done |
| Soft-delete | Done |
| Today's summary endpoint | Done |
| Appointment list/calendar API | Done |
| Admin double-booking override | Done |
| Provider linked to Identity `User` | Done |
| Frontend scheduling UI | Done |

---

## Phase 3: Event-Driven Billing Integration

**Goal:** Automate clinical-to-financial handoff.

**Description:** Connect Scheduling and Billing via MediatR; auto-generate invoices on completed visits.

**Key deliverables:**

* `AppointmentCompletedEvent` definition and publishing
* Billing handler for completed visits
* Invoice generation based on AppointmentType rates

**Progress:** ~70% complete

| Deliverable | Status |
|-------------|--------|
| Event + MediatR handler | Done |
| Auto invoice + debit ledger | Done |
| Integration test (appointment → invoice) | Done |
| AppointmentType pricing | Not done (hardcoded $150 fee) |

---

## Phase 4: Financial Ledger & Payments

**Goal:** Compliant financial system for clinic revenue.

**Description:** Double-entry ledger, payment processing, reporting.

**Key deliverables:**

* Immutable `FinancialLedger` table
* Payment logic (invoice → `Paid`)
* Refund/void workflow (FR-B02)
* Revenue reporting dashboard

**Progress:** ~50% complete

| Deliverable | Status |
|-------------|--------|
| Ledger + invoice entities | Done |
| `POST /billing/invoices/{id}/pay` | Done |
| Revenue summary API | Done |
| Refund/void workflow | Not done |
| Ledger immutability enforcement | Not done |
| Billing UI + dashboard wired to API | Not done |

---

## Phase 5: Compliance, Auditing & Hardening

**Goal:** Meet HIPAA-level non-functional requirements before MVP launch.

**Description:** Audit logging, security hardening, test infrastructure.

**Key deliverables:**

* EF Core `AuditLog` interceptor (old vs. new values)
* Indexing on `TenantId` and `IsDeleted`
* Testcontainers for integration tests
* Swagger/OpenAPI polish + RBAC validation

**Progress:** ~10% complete

| Deliverable | Status |
|-------------|--------|
| Swagger in Development | Done |
| Basic integration tests (SQL Server) | Done |
| CI pipeline | Done |
| Audit log table + interceptor | Not done |
| Testcontainers | Not done |
| Secrets in vault/env (not source) | Not done |
| Full RBAC validation | Not done |

---

## Frontend Roadmap (parallel track)

| Milestone | Status |
|-----------|--------|
| Login / signup pages | Done |
| HTTP interceptor (JWT + tenant header) | Done |
| Shared API config | Done |
| Dashboard route + auth guard | Done |
| Dashboard wired to summary APIs | Done |
| App shell / navigation | Done |
| Scheduling, patients, billing UIs | Scheduling + patients done; billing UI deferred to Phase 4 |

---

## Summary of Phases

| Phase | Focus | Primary stakeholder |
| --- | --- | --- |
| **1** | Multi-tenancy & auth | System Admin |
| **2** | Patient workflows | Front Desk / Provider |
| **3** | Automation (events) | System reliability |
| **4** | Revenue management | Billing Manager |
| **5** | Security & audit | Compliance officer |

---

## Recommended next steps

1. **Phase 3:** AppointmentType pricing instead of hardcoded visit fee
2. **Phase 4:** Billing UI + refund/void workflow
3. **Phase 5:** Audit log interceptor and move encryption/JWT secrets to configuration

See [DEVELOPMENT.md](DEVELOPMENT.md) for running the stack locally and [API.md](API.md) for endpoint details.
