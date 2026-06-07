# HPMS Documentation

This folder contains the product and technical documentation for the Healthcare Practice Management System.

## Reading order

1. **[DESCRIPTION.md](DESCRIPTION.md)** — What HPMS is, business value, and scope
2. **[REQUIREMENTS.md](REQUIREMENTS.md)** — Roles, use cases, functional/non-functional requirements
3. **[PLAN.md](PLAN.md)** — Five-phase roadmap and current implementation progress
4. **[DESIGN.md](DESIGN.md)** — Modular monolith architecture and design decisions
5. **[ERM.md](ERM.md)** — Database schema (as implemented; planned tables marked)
6. **[API.md](API.md)** — REST endpoint reference
7. **[DEVELOPMENT.md](DEVELOPMENT.md)** — How to run backend and frontend locally

## Document maintenance

When you change the codebase, update the relevant doc:

| Change type | Update |
|-------------|--------|
| New API endpoint | [API.md](API.md) |
| Schema / migration | [ERM.md](ERM.md) |
| Requirement met or deferred | [REQUIREMENTS.md](REQUIREMENTS.md), [PLAN.md](PLAN.md) |
| Architecture decision | [DESIGN.md](DESIGN.md) |
| Local setup steps | [DEVELOPMENT.md](DEVELOPMENT.md), root [README.md](../README.md) |

Last reviewed against codebase: June 2026.
