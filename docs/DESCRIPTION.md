# Project Description: Healthcare Practice Management System (HPMS)

## 1. Executive Summary
The Healthcare Practice Management System (HPMS) is a multi-tenant, cloud-based B2B SaaS application designed for independent medical and behavioral health clinics. It consolidates patient scheduling, clinical workflows, and financial billing into a single, unified platform. By leveraging a **Modular Monolith** architecture, HPMS ensures rapid development and deployment while maintaining clear separation of concerns across its core modules: Identity, Scheduling, Clinical, and Billing. The system is built with C# / .NET 8 and SQL Server, adhering to HIPAA compliance standards for data security and auditability.

## 2. Purpose & Business Value
Mid-sized clinics often rely on fragmented systems—using one software for scheduling, another for billing, and spreadsheets for reporting. This fragmentation leads to high administrative overhead, missed revenue (unbilled appointments), and compliance risks. 

HPMS solves this by utilizing an **Event-Driven Architecture**. When a clinical appointment is marked "Completed," the system automatically publishes an event to generate a pending invoice in the billing module, eliminating manual data entry and ensuring financial accuracy.

## 3. Project Scope
### In-Scope (Core MVP)
* **Tenant Management:** Secure onboarding for independent clinics, ensuring strict data isolation (Multi-Tenancy).
* **Patient Scheduling:** Calendar management, conflict detection, and appointment state tracking (Booked, Arrived, Completed, No-Show).
* **Billing & Invoicing:** Generation of superbills, payment reconciliation, and a double-entry ledger for clinic revenue.
* **Role-Based Access Control (RBAC):** Granular permissions for Front Desk, Providers, and Billing Managers.

### Out-of-Scope (Future Iterations)
* Direct telehealth video streaming integration.
* E-prescribing routing to external pharmacies.
* Automated insurance clearinghouse integrations (EDI 837/835).

## 4. Constraints & Assumptions
* **Regulatory:** The system must adhere to foundational HIPAA compliance standards, including data encryption (at rest and in transit) and comprehensive audit logging.
* **Technical:** The backend must be built using C# / .NET 8, utilizing Entity Framework Core and SQL Server to align with enterprise HealthTech standards.
* **Architecture:** The system will utilize a **Modular Monolith** architecture to balance separation of concerns with deployment simplicity, avoiding the unnecessary overhead of microservices for the MVP phase.