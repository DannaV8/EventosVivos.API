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
GET   /api/events                  → ListEventsQuery        → 200 + EventDto[]     [public]
GET   /api/events/{id}/report      → EventOccupancyQuery   → 200 + EventOccupancyDto
```

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

---

## ExceptionHandlingMiddleware

Maps all exceptions to a structured JSON response:

```json
{
  "error": "CAPACITY_EXCEEDED",
  "message": "No hay suficientes entradas disponibles.",
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
- User-facing error messages stay in Spanish; all identifiers and comments in English
