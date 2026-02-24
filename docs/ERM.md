
---

# Database Design: Healthcare Practice Management System (HPMS)

## 1. ERM Overview

The HPMS database uses a **Shared Database, Shared Schema** multi-tenancy approach. Every table (excluding the global `Tenants` table) includes a `TenantId` to ensure strict data isolation via EF Core Global Query Filters.

---

## 2. Global & Infrastructure Tables

These tables manage the SaaS platform's multi-tenancy and compliance requirements.

### `Tenants`

Stores the identity of the independent clinics using the platform.
| Column | Type | Description |
| :--- | :--- | :--- |
| **Id** (PK) | Guid | Unique identifier for the clinic. |
| **Name** | nvarchar(255) | Legal name of the practice. |
| **ApiKey** | nvarchar(max) | Encrypted key for external integrations. |
| **IsActive** | boolean | Subscription status. |
| **CreatedAt** | datetime | Timestamp of onboarding. |

### `AuditLogs`

Mandatory for HIPAA compliance; tracks every mutation in the system.
| Column | Type | Description |
| :--- | :--- | :--- |
| **Id** (PK) | bigint | Unique log identifier. |
| **TenantId** (FK) | Guid | Links log to a specific clinic. |
| **EntityName** | nvarchar(100) | Name of the table (e.g., "Patients"). |
| **EntityId** | nvarchar(100) | The PK of the modified record. |
| **UserId** (FK) | Guid | The user who performed the action. |
| **Action** | nvarchar(10) | Create, Update, or Delete. |
| **OldValues** | nvarchar(max) | JSON snapshot of data before change. |
| **NewValues** | nvarchar(max) | JSON snapshot of data after change. |
| **Timestamp** | datetime | Date and time of the event. |

---

## 3. Identity Module (`HPMS.Identity`)

Manages RBAC (Role-Based Access Control) and user authentication.

### `Users`

| Column | Type | Description |
| --- | --- | --- |
| **Id** (PK) | Guid | Unique identifier. |
| **TenantId** (FK) | Guid | Clinic ownership. |
| **Username** | nvarchar(100) | Login credential. |
| **PasswordHash** | nvarchar(max) | BCrypt/Argon2 hashed password. |
| **RoleId** (FK) | int | Reference to the Roles table. |
| **IsDeleted** | boolean | Soft delete flag for compliance. |

### `Roles`

Static table defining permissions: `SystemAdmin`, `ClinicAdmin`, `Provider`, `BillingManager`, `FrontDesk`.

---

## 4. Scheduling Module (`HPMS.Scheduling`)

Handles clinical operations and patient demographics.

### `Patients`

| Column | Type | Description |
| --- | --- | --- |
| **Id** (PK) | Guid | Unique identifier. |
| **TenantId** (FK) | Guid | Clinic ownership. |
| **FirstName** | nvarchar(100) | Patient's first name. |
| **LastName** | nvarchar(100) | Patient's last name. |
| **DOB** | date | Date of birth. |
| **PHI_Data** | nvarchar(max) | AES-256 Encrypted contact/medical info. |
| **IsDeleted** | boolean | Soft delete flag. |

### `Appointments`

| Column | Type | Description |
| --- | --- | --- |
| **Id** (PK) | Guid | Unique identifier. |
| **TenantId** (FK) | Guid | Clinic ownership. |
| **PatientId** (FK) | Guid | Reference to Patients. |
| **ProviderId** (FK) | Guid | Reference to Users (Role: Provider). |
| **StartTime** | datetime | Start of appointment. |
| **EndTime** | datetime | End of appointment. |
| **Status** | int | Enum: Scheduled, Arrived, InSession, Completed, NoShow. |
| **TypeId** (FK) | int | Reference to AppointmentTypes. |

---

## 5. Billing Module (`HPMS.Billing`)

Triggered by `AppointmentCompletedEvent` via MediatR.

### `Invoices`

| Column | Type | Description |
| --- | --- | --- |
| **Id** (PK) | Guid | Unique identifier. |
| **TenantId** (FK) | Guid | Clinic ownership. |
| **AppointmentId** (FK) | Guid | The clinical event that triggered this bill. |
| **TotalAmount** | decimal(18,2) | Total cost. |
| **Status** | int | Enum: Draft, Open, Paid, Void. |

### `FinancialLedger`

An immutable table for double-entry accounting.
| Column | Type | Description |
| :--- | :--- | :--- |
| **Id** (PK) | bigint | Unique identifier. |
| **TenantId** (FK) | Guid | Clinic ownership. |
| **InvoiceId** (FK) | Guid | Associated invoice. |
| **Amount** | decimal(18,2) | Transaction value. |
| **Type** | nvarchar(10) | Debit (Charge) or Credit (Payment/Refund). |
| **CreatedAt** | datetime | Immutable timestamp. |

---

## 6. Implementation Notes

1. **Soft Deletes:** All tables implement an `ISoftDelete` interface. Rows are never physically removed from the database to satisfy HIPAA audit requirements.
2. **Concurrency:** The `Appointments` table uses a `RowVersion` (timestamp) column to handle `FR-S01` (preventing double-booking) during high-concurrency scheduling.
3. **Indexing:** Non-clustered indexes are applied to all `TenantId` and `IsDeleted` columns to optimize Global Query Filter performance.