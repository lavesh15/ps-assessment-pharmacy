# ABC Pharmacy — Medicine Tracker

Single-page inventory app for ABC Pharmacy: list, search, add, and sell medicines. The API is ASP.NET Core 10 with layered architecture and JSON file storage. The UI is React 19 with Redux Toolkit and Axios.

## Run locally

Use two terminals.

### 1. API (`http://localhost:5001`)

```bash
dotnet run --project backend/src/Pharmacy.Api
```

Swagger UI (Development only): http://localhost:5001/swagger

### 2. Web UI (`http://localhost:5173`)

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173 and sign in with:

- Username: `admin`
- Password: `Admin@123`

Architecture, class/sequence diagrams, security, and debug steps: [DOCUMENTATION.md](DOCUMENTATION.md).

## Assignment features

- Medicine grid (name, brand, expiry, quantity, price — notes are omitted)
- Red row when expiry is less than 30 days
- Yellow row when quantity is less than 10 (red wins if both apply)
- Search by medicine name
- Add medicine
- Sell reduces stock and appends an audit record in `sales.json`

## Demo data

Seed inventory lives in [`backend/src/Pharmacy.Api/data/medicines.json`](backend/src/Pharmacy.Api/data/medicines.json). Sales and idempotency keys are stored beside it.

## Architecture

```
backend/src/Pharmacy.Api            HTTP, middleware, versioned controllers
backend/src/Pharmacy.Application    DTOs, FluentValidation, services, Options
backend/src/Pharmacy.Domain         Entities and repository contracts
backend/src/Pharmacy.Infrastructure JSON file persistence
frontend                           React 19 + Redux + Axios
```

API versioning uses URL segments, for example `/api/v1/medicines`.

## Security notes (demo)

These controls are implemented for the assignment, not as a production identity system.

- Demo cookie login (hardcoded user in configuration)
- API key delivered in an HttpOnly cookie after login
- CORS allowlist and Origin check on mutating requests
- Double-submit CSRF (`pharmacy.csrf` cookie + `X-CSRF-TOKEN` header)
- Correlation id on every request (`X-Correlation-ID`)
- Rate limiting (global, API, tighter login policy)
- Concurrency token (`version`) on sell → HTTP 409 on mismatch
- `Idempotency-Key` required on create and sell
- Serilog + global exception handler (stack traces only in Development)
- HTTPS redirection and HSTS in Production only
- XSS hardening: JSON responses, security headers, HTML rejected in text fields

Do not reuse the demo password or API key outside this project.
