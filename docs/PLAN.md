
---

# Project Development Plan: HPMS

## Executive Summary

The development of HPMS is divided into five distinct phases. This approach ensures that the foundational multi-tenancy and security layers are established before moving into complex clinical and financial workflows.

---

## Phase 1: Foundation & Multi-Tenancy

**Goal:** Establish the core infrastructure, tenant isolation, and identity management.

* **Description:** Implement the `HPMS.Identity` module. This includes the global `Tenants` table and the automated `TenantId` filtering logic.
* **Key Deliverables:**
* Database migrations for Tenants, Users, and Roles.
* JWT Authentication middleware.
* EF Core Global Query Filters for automated data isolation.
* Tenant onboarding API (System Admin only).



---

## Phase 2: Scheduling & Patient Management

**Goal:** Build the primary clinical workflow for front-desk and provider staff.

* **Description:** Implement the `HPMS.Scheduling` module. This phase focuses on patient CRUD operations and the calendar logic required to manage clinic capacity.
* **Key Deliverables:**
* Patient Demographic management with AES-256 encryption for PHI.
* Appointment booking engine with conflict detection (preventing double-booking per `FR-S01`).
* State machine for appointments (`Scheduled`  `Arrived`  `Completed`).
* Soft-delete implementation for all clinical records.



---

## Phase 3: Event-Driven Billing Integration

**Goal:** Automate the transition from clinical activity to financial record-keeping.

* **Description:** Connect the `Scheduling` and `Billing` modules using MediatR. This phase focuses on the "In-Scope" requirement of automatic invoice generation.
* **Key Deliverables:**
* `AppointmentCompletedEvent` definition and publishing logic.
* Billing Module event handler to catch completed visits.
* Automatic "Draft" invoice generation based on `AppointmentType` rates.



---

## Phase 4: Financial Ledger & Payments

**Goal:** Implement a compliant, immutable financial system for clinic revenue.

* **Description:** Finalize the `HPMS.Billing` module by building out the double-entry ledger and payment processing UI/API.
* **Key Deliverables:**
* Immutable `FinancialLedger` table for auditing.
* Payment application logic (updating Invoice status to `Paid`).
* Refund/Void workflow (ensuring no hard deletes of financial data per `FR-B02`).
* Basic revenue reporting dashboard for Clinic Admins.



---

## Phase 5: Compliance, Auditing & Hardening

**Goal:** Ensure the system meets all HIPAA-level non-functional requirements (NFRs).

* **Description:** Implement the comprehensive audit logging system and finalize security hardening before the MVP launch.
* **Key Deliverables:**
* Automated `AuditLog` interceptor in EF Core (captures Old vs. New values).
* Performance indexing on `TenantId` and `IsDeleted` columns.
* Integration tests using Testcontainers to verify tenant data leakage prevention.
* API Documentation (Swagger/OpenAPI) and Role-Based Access Control (RBAC) final validation.



---

## Summary of Phases

| Phase | Focus | Primary Stakeholder |
| --- | --- | --- |
| **1** | Multi-Tenancy & Auth | System Admin |
| **2** | Patient Workflows | Front Desk / Provider |
| **3** | Automation (Events) | System Reliability |
| **4** | Revenue Management | Billing Manager |
| **5** | Security & Audit | Compliance Officer |