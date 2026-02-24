# Project Design & Architecture: HPMS

## 1. Architectural Style: Modular Monolith
The system is designed as a **Modular Monolith**. Instead of splitting the application into separate deployable microservices (which introduces network latency and complex distributed transactions), the system is separated into logical boundaries (Modules) within a single deployed ASP.NET Core API.



**Core Modules:**
1. `HPMS.Scheduling`
2. `HPMS.Billing`
3. `HPMS.Identity` (Tenant and User Management)

*Rule of thumb:* Modules cannot directly query another module's database tables. They must communicate via public C# interfaces or Domain Events.

## 2. Key Design Patterns
### Event-Driven Communication (MediatR)
To decouple the Scheduling and Billing modules, we utilize the Mediator pattern.
* When Scheduling completes a visit, it publishes an `AppointmentCompletedEvent` to the MediatR bus.
* The Billing module implements an `INotificationHandler<AppointmentCompletedEvent>` which catches the event and generates the invoice.



### Multi-Tenancy (Global Query Filters)
To guarantee strict data isolation without relying on developers to manually type `WHERE TenantId = X` on every query, we utilize **EF Core Global Query Filters**.
* The current user's `TenantId` is extracted from their JWT token via HTTP Context.
* The `AppDbContext` automatically intercepts all LINQ queries and appends the `TenantId` filter before generating the SQL sent to the database.

## 3. Technology Stack
* **Frontend:** Angular (TypeScript, RxJS, SCSS/Tailwind)
* **Backend:** C# / .NET 8 ASP.NET Core Web API
* **Database:** MS SQL Server (Relational data, ACID compliance for billing)
* **ORM:** Entity Framework Core 8
* **Testing:** xUnit, Moq (Unit Testing), Testcontainers (Integration Testing)

## 4. Tradeoffs & Decisions
### Monolith vs. Microservices
* **Decision:** Modular Monolith.
* **Justification:** Microservices require complex infrastructure (Kubernetes, distributed tracing) and handle data consistency poorly (eventual consistency is dangerous for early-stage billing systems). A well-architected monolith provides the separation of concerns of microservices but is much faster to develop, test, and deploy for a mid-sized B2B SaaS.

### Angular vs. React
* **Decision:** Angular.
* **Justification:** While React is highly prevalent in consumer startups, Angular is the standard for complex, data-heavy enterprise applications (particularly in HealthTech and FinTech). Angular's highly opinionated OOP structure, built-in Dependency Injection, and heavy use of RxJS for handling asynchronous data streams perfectly mirror the architecture of the .NET backend. This creates a cohesive full-stack environment where architectural patterns translate seamlessly from server to client.

### Soft Deletes vs. Hard Deletes
* **Decision:** Soft Deletes (`IsDeleted = true`).
* **Justification:** In healthcare software, physical deletion of records (Hard Delete) violates audit and compliance requirements. All core entities implement an `ISoftDelete` interface, and EF Core filters out deleted records automatically.