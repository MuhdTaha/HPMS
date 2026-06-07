# Local Development Guide

How to run the HPMS backend and frontend on your machine.

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 8.0.x | See `global.json` |
| SQL Server | 2019+ | LocalDB, Express, or full instance |
| Node.js | 20+ | For `hpms-ui` |
| npm | 11.x | Bundled with Node or use version in `hpms-ui/package.json` |

Optional: [SQL Server Management Studio](https://learn.microsoft.com/en-us/sql/ssms/) or Azure Data Studio for inspecting the database.

## Backend (`HPMS.Web`)

### 1. Configure the database

Edit `HPMS.Web/appsettings.json` and set `ConnectionStrings:DefaultConnection` to your SQL Server instance:

```json
"DefaultConnection": "Server=YOUR_SERVER\\SQLEXPRESS;Database=HPMS_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
```

Examples:

- **LocalDB:** `Server=(localdb)\\mssqllocaldb;Database=HPMS_Dev;Trusted_Connection=True;TrustServerCertificate=True;`
- **SQL Express:** `Server=.\\SQLEXPRESS;Database=HPMS_Dev;Trusted_Connection=True;TrustServerCertificate=True;`

The checked-in connection string targets a specific machine name and must be changed for other environments.

### 2. Configure JWT (optional for local dev)

Defaults in `appsettings.json` are sufficient for local development:

```json
"Jwt": {
  "Key": "A_Very_Long_Secret_Key_At_Least_32_Chars_Long!",
  "Issuer": "HPMS.Api",
  "Audience": "HPMS.Users"
}
```

For production, move secrets to user secrets, environment variables, or a vault — never commit real keys.

### 3. Run the API

From the repository root:

```powershell
dotnet run --project HPMS.Web --launch-profile http
```

Or open `HPMS.sln` in Visual Studio / Rider and run **HPMS.Web** with the **http** profile.

On startup, the app applies EF Core migrations for all three contexts (Identity, Scheduling, Billing).

| Resource | URL |
|----------|-----|
| API base | http://localhost:5260 |
| Swagger UI | http://localhost:5260/swagger |

**Note:** Only `HPMS.Web` needs to run. The standalone `Program.cs` files in `HPMS.Modules.Identity` and `HPMS.Scheduling` are scaffolds and are not used in normal development.

### 4. Verify the backend

1. Open Swagger at http://localhost:5260/swagger
2. `POST /identity/tenants?name=TestClinic` — create a tenant
3. `POST /identity/users` — register a user (use the tenant ID from step 2)
4. `POST /identity/login` — obtain a JWT
5. Click **Authorize** in Swagger and enter `Bearer {your-token}`
6. Call a protected route such as `GET /scheduling/patients`

## Frontend (`hpms-ui`)

### 1. Install dependencies

```powershell
cd hpms-ui
npm install
```

### 2. Configure API URLs

`hpms-ui/.env` (local only — do not commit secrets):

```
apiUrl=http://localhost:5260/identity
apiBaseUrl=http://localhost:5260
```

The app reads these via `src/app/core/config/api.config.ts`. If `.env` is not injected at build time, defaults match the values above.

### 3. Run the dev server

```powershell
npm start
```

Open http://localhost:4200

CORS on the backend allows `http://localhost:4200`. The HTTP interceptor attaches `Authorization: Bearer {token}` and `X-Tenant-Id` headers from `localStorage` after login.

### 4. Build for production

```powershell
npm run build
```

Output: `hpms-ui/dist/hpms-ui/`

## Running both together

Use two terminals:

```powershell
# Terminal 1 — backend
dotnet run --project HPMS.Web --launch-profile http

# Terminal 2 — frontend
cd hpms-ui
npm start
```

Ensure SQL Server is running before starting the backend.

## Tests

```powershell
# All backend tests (requires SQL Server for integration tests)
dotnet test HPMS.sln

# Frontend unit tests
cd hpms-ui
npm test
```

Integration tests use `WebApplicationFactory<Program>` and expect the connection string from environment variables in CI, or your local `appsettings.json` when run locally.

## Troubleshooting

| Problem | Likely cause | Fix |
|---------|--------------|-----|
| SQL connection failed on startup | Wrong server name or SQL not running | Update `appsettings.json`; start SQL Server service |
| `dotnet ef` not found | EF tools not installed | `dotnet tool install --global dotnet-ef` |
| CORS error in browser | Backend not on port 5260 or frontend not on 4200 | Match ports in launch profile and `api.config.ts` |
| 401 on scheduling/billing routes | Missing or expired JWT | Log in again; use Swagger Authorize to test |
| Frontend build: `environment` error | Stale reference | Use `api.config.ts` (see `src/app/core/config/`) |
| Login succeeds but returns to login page | `/dashboard` route not registered | See [PLAN.md](PLAN.md) frontend gaps |

## EF Core migrations

Migrations are applied automatically on app startup. To add a new migration manually:

```powershell
# Identity
dotnet ef migrations add MigrationName --project HPMS.Modules.Identity --startup-project HPMS.Web --context IdentityDbContext

# Scheduling
dotnet ef migrations add MigrationName --project HPMS.Scheduling --startup-project HPMS.Web --context SchedulingDbContext

# Billing
dotnet ef migrations add MigrationName --project HPMS.Modules.Billing --startup-project HPMS.Web --context BillingDbContext
```

Apply without running the app:

```powershell
dotnet ef database update --project HPMS.Modules.Identity --startup-project HPMS.Web --context IdentityDbContext
```
