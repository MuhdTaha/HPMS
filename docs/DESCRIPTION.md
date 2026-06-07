# Project Description: Healthcare Practice Management System (HPMS)

## 1. Executive Summary

The Healthcare Practice Management System (HPMS) is a multi-tenant, cloud-based B2B SaaS application designed for independent medical and behavioral health clinics. It consolidates patient scheduling, clinical workflows, and financial billing into a single, unified platform.

The system uses a **modular monolith** architecture: one deployable ASP.NET Core API (`HPMS.Web`) with logical module boundaries for **Identity**, **Scheduling**, and **Billing**, plus an **Angular** single-page application (`hpms-ui`). The stack is C# / .NET 8, SQL Server, and Entity Framework Core, with design goals aligned to HIPAA compliance (encryption, auditability, tenant isolation).

## 2. Purpose & Business Value

Mid-sized clinics often rely on fragmented systems—one product for scheduling, another for billing, and spreadsheets for reporting. That fragmentation increases administrative overhead, missed revenue from unbilled appointments, and compliance risk.

HPMS addresses this with **event-driven integration** between clinical and financial workflows. When an appointment is marked **Completed**, the system publishes an `AppointmentCompletedEvent` that automatically creates a pending invoice in the billing module, reducing manual data entry and improving financial accuracy.

## 3. Project Scope

### In-Scope (Core MVP)

* **Tenant management:** Secure onboarding for independent clinics with strict data isolation (multi-tenancy).
* **Patient scheduling:** Appointment booking, conflict detection, and status tracking (`Scheduled`, `Arrived`, `InSession`, `Completed`, `NoShow`, `Canceled`).
* **Billing & invoicing:** Event-driven invoice generation, payment recording, and a financial ledger.
* **Role-based access control (RBAC):** Distinct roles for System Admin, Clinic Admin, Provider, Billing Manager, and Front Desk.
* **Web UI:** Authentication (login/signup), role-aware dashboard, and module-specific views (in progress).

### Out-of-Scope (Future Iterations)

* Direct telehealth video streaming integration.
* E-prescribing routing to external pharmacies.
* Automated insurance clearinghouse integrations (EDI 837/835).

## 4. Constraints & Assumptions

* **Regulatory:** The system must support foundational HIPAA requirements: encryption at rest and in transit, audit logging, and access controls. Full compliance hardening is planned for Phase 5 (see [PLAN.md](PLAN.md)).
* **Technical:** Backend is C# / .NET 8 with EF Core and SQL Server. Frontend is Angular 21 with PrimeNG.
* **Architecture:** Modular monolith for the MVP—clear module boundaries without the operational overhead of microservices.

## 5. Related Documentation

| Document | Contents |
|----------|----------|
| [REQUIREMENTS.md](REQUIREMENTS.md) | Functional and non-functional requirements |
| [PLAN.md](PLAN.md) | Development phases and current progress |
| [DESIGN.md](DESIGN.md) | Architecture and design patterns |
| [DEVELOPMENT.md](DEVELOPMENT.md) | Local setup instructions |
