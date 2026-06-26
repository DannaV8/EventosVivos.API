# CLAUDE.md — EventosVivos.API (Presentation layer)

Read the root CLAUDE.md before working here.

## What this layer is

HTTP entry point only. Controllers dispatch to MediatR. No business logic here.
References: all other layers (it's the composition root).

---

## Structure

```
EventosVivos.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── EventsController.cs
│   └── ReservationsController.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── appsettings.json
├── appsettings.Development.json       ← gitignored
└── Program.cs
```

---

## Endpoints

### AuthController — `/api/auth`
```
POST  /api/auth/register   → RegisterUserCommand   → 200 + { id }
POST  /api/auth/login      → LoginCommand          → 200 + { token }
```

### EventsController — `/api/events`
```
POST  /api/events                  → CreateEventCommand     → 201 + { id }        [AdminOnly]
GET   /api/events                  → ListEventsQuery        → 200 + PagedResult<EventDto>  [public]
GET   /api/events/{id}             → GetEventByIdQuery      → 200 + EventDto / 404 [public]
GET   /api/events/{id}/report      → EventOccupancyQuery   → 200 + EventOccupancyDto
```

`GET /api/events` is paginated: query params `page` (default 1) and `pageSize` (default 9),
wrapped in `PagedResult<T>` (`{ items, totalCount, page, pageSize }`).
`GET /api/events/{id}` is `[AllowAnonymous]` — the UI's reservation page shows the event before requiring login.

Query params for `GET /api/events` (all optional):
- `type` — Conference | Workshop | Concert
- `startDate` — ISO 8601
- `endDate` — ISO 8601
- `venueId` — int
- `status` — Active | Cancelled | Completed
- `title` — partial match, case-insensitive

### ReservationsController — `/api/reservations`
```
GET   /api/reservations                    → ListReservationsQuery      → 200 + ReservationDto[]
POST  /api/reservations                    → CreateReservationCommand   → 201 + { id }
PUT   /api/reservations/{id}/confirm       → ConfirmPaymentCommand      → 200 + { reservationCode }  [AdminOnly]
PUT   /api/reservations/{id}/cancel        → CancelReservationCommand   → 200
```

---

## Auth

All endpoints require JWT Bearer except `GET /api/events` (public).
Admin-only endpoints use `[Authorize(Policy = "AdminOnly")]` which requires claim `rol: admin`.

**`GetUserId()`** helper in `ReservationsController` extracts `sub` from the JWT token using `JwtRegisteredClaimNames.Sub`. Returns `Guid` or throws if the claim is missing.

---

## Rate limiting

Built-in ASP.NET Core rate limiting (`Microsoft.AspNetCore.RateLimiting` — no NuGet package needed).
Configured in `Program.cs` via `AddRateLimiter(...)` + `app.UseRateLimiter()` (placed after `UseCors`, before `UseAuthentication`).

| Scope | Limit | Partition |
|---|---|---|
| **Global** (all endpoints) | `RateLimiting:Global` — 100 req / 60s | per client IP |
| **`auth` policy** (`AuthController`) | `RateLimiting:Auth` — 5 req / 60s | per client IP |

- Both are **fixed-window** limiters, partitioned by `HttpContext.Connection.RemoteIpAddress`.
- The `auth` policy is applied with `[EnableRateLimiting("auth")]` on `AuthController` (brute-force protection on `/login` and `/register`). It **stacks on top of** the global limiter.
- Limits live in `appsettings.json` under `RateLimiting:{Global,Auth}:{PermitLimit,WindowSeconds}`; `Program.cs` reads them with `GetValue(...)` and falls back to the same defaults (100/60, 5/60) if absent. Override per environment in `appsettings.Development.json`.
- On rejection, `OnRejected` writes the **same JSON error shape** as `ExceptionHandlingMiddleware` (`{ error, message }`) with `error = "RATE_LIMIT_EXCEEDED"` and a `Retry-After` header. The 429 does **not** pass through `ExceptionHandlingMiddleware` (it is not an exception), so the consistent shape is produced here instead.

---

## ExceptionHandlingMiddleware

Maps all exceptions to a structured JSON response:

```json
{
  "error": "CAPACITY_EXCEEDED",
  "message": "Not enough tickets available.",
  "detail": "..."
}
```

| Exception | HTTP status |
|---|---|
| `NotFoundException` | 404 |
| `ValidationException` (FluentValidation) | 400 |
| `CapacityExceededException` | 409 |
| `VenueConflictException` | 409 |
| `InvalidReservationStateException` | 422 |
| `InvalidCredentialsException` | 401 |
| `DomainException` (base) | 400 |
| `DbUpdateConcurrencyException` | 409 |
| Unhandled `Exception` | 500 (detail hidden in production) |

---

## Hard rules

- No business logic in controllers — ever
- No direct repository or DbContext injection into controllers
- No domain entities in controller responses — only DTOs from Application layer
- Swagger must include JWT bearer configuration so the API is testable from the UI
- All user-facing error messages in English; all identifiers and comments in English
