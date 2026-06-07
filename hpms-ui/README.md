# HPMS UI

Angular 21 frontend for the Healthcare Practice Management System. Uses PrimeNG for components and a custom dark theme defined in `src/styles.scss`.

## Prerequisites

- Node.js 20+
- Backend running at http://localhost:5260 (see [../docs/DEVELOPMENT.md](../docs/DEVELOPMENT.md))

## Setup

```bash
npm install
```

### Environment

Create or edit `.env` in this directory (local development):

```
apiUrl=http://localhost:5260/identity
apiBaseUrl=http://localhost:5260
```

Values are read in `src/app/core/config/api.config.ts`. Defaults match the above if `.env` is not loaded.

## Development

```bash
npm start
```

Open http://localhost:4200

## Build & test

```bash
npm run build    # output: dist/hpms-ui/
npm test         # Vitest via Angular CLI
```

## Project structure

```
src/app/
├── core/
│   ├── config/api.config.ts       # API URLs
│   ├── interceptors/api.interceptor.ts  # JWT + X-Tenant-Id
│   └── services/
│       ├── auth/                  # Login, signup API calls
│       ├── dashboard.service.ts   # Scheduling/billing summaries
│       └── toast/                 # PrimeNG notifications
├── features/
│   ├── auth/login/                # Login page
│   ├── auth/signup/               # Tenant + user registration
│   └── dashboard/                 # Role-aware dashboard (WIP)
├── app.routes.ts
└── app.config.ts
```

## Current status

| Feature | Status |
|---------|--------|
| Login | Done |
| Signup (tenant + user) | Done |
| Dashboard route | Not registered — login redirects to `/dashboard` which falls through to login |
| Dashboard API integration | `DashboardService` exists but component does not call it |
| Auth guards | Not implemented |
| Scheduling / billing / patients UI | Not started |

See [../docs/PLAN.md](../docs/PLAN.md) for the frontend roadmap.

## Styling

- Global tokens and PrimeNG overrides: `src/styles.scss`
- Auth pages: scoped SCSS in `features/auth/*/`
- PrimeIcons loaded via `angular.json`
- PrimeFlex is in `package.json` but not yet added to global styles — dashboard layout classes may not apply until configured

## Related docs

- [../docs/DEVELOPMENT.md](../docs/DEVELOPMENT.md) — full stack local setup
- [../docs/API.md](../docs/API.md) — backend endpoints consumed by services
