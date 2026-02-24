# Project Requirements: HPMS

## 1. User Roles & Personas
* **System Admin (Superuser):** Manages tenant creation, global billing configurations, and system-wide monitoring.
* **Clinic Admin:** Manages clinic-specific settings, staff onboarding, and views financial dashboards.
* **Provider (Doctor/Therapist):** Manages their personal schedule, views patient demographics, and marks appointments as complete.
* **Billing Manager:** Processes invoices, records patient payments, and reconciles the financial ledger.
* **Front Desk:** Books appointments, checks patients in, and collects copays.

## 2. Core Use Cases


* **UC-01: Schedule Appointment.** A Front Desk user selects a Provider, Patient, and time slot. The system must validate that the Provider is available and the time slot does not overlap with existing appointments.
* **UC-02: Complete Visit & Trigger Billing.** A Provider marks an appointment as "Completed." The system automatically generates a draft invoice based on the appointment type's configured rate.
* **UC-03: Process Patient Payment.** A Billing Manager applies a payment to an open invoice. The system must update the invoice status to "Paid" and record a credit to the clinic's revenue ledger.

## 3. Functional Requirements (FR)
### Module: Scheduling
* **FR-S01:** The system shall prevent double-booking for a single provider unless explicitly overridden by a Clinic Admin.
* **FR-S02:** The system shall allow appointments to transition through states: `Scheduled` -> `Arrived` -> `InSession` -> `Completed` or `NoShow`.

### Module: Billing
* **FR-B01:** The system shall automatically generate a draft invoice when an `AppointmentCompletedEvent` is detected.
* **FR-B02:** The system shall maintain an immutable financial ledger; payments cannot be deleted, only offset by a refund/void transaction.

### Module: Multi-Tenancy
* **FR-M01:** The system shall associate every User, Patient, Appointment, and Invoice with a specific `TenantId`.

## 4. Non-Functional Requirements (NFR)
* **NFR-01 Security & HIPAA Compliance:**
  * All API endpoints must require a valid JWT.
  * Passwords must be hashed using BCrypt/Argon2.
  * Protected Health Information (PHI) must be encrypted at rest (AES-256) and in transit (TLS 1.2+).
  * Every mutation (POST/PUT/DELETE) to a clinical record must generate an immutable audit log detailing *who* made the change and *when*.
* **NFR-02 Data Isolation (Multi-Tenancy):** A user from Clinic A must never be able to query, view, or modify data belonging to Clinic B, enforced at the database query level.
* **NFR-03 Availability & Performance:** The API must maintain a 99.9% uptime SLA and respond to standard GET queries (e.g., loading the daily schedule) in under 300ms.