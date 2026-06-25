# CLAUDE.md — EventosVivos.Application

Read the root CLAUDE.md before working here.

## What this layer is

Use cases. One handler per operation. This layer orchestrates the domain
and repositories but contains no business logic itself — rules live in Domain.

Dependencies allowed: `EventosVivos.Domain` only.
Infrastructure types come in via interfaces defined here.

---

## Structure

```
EventosVivos.Application/
├── Common/
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs       ← runs FluentValidation before every handler
│   ├── Exceptions/
│   │   └── NotFoundException.cs
│   └── Interfaces/
│       ├── EventFilter.cs
│       ├── IEventRepository.cs
│       ├── IReservationRepository.cs
│       ├── IUserRepository.cs
│       ├── IVenueRepository.cs
│       ├── ITokenGenerator.cs
│       └── IUnitOfWork.cs
├── Auth/
│   └── Commands/
│       ├── Login/
│       └── RegisterUser/
├── Events/
│   ├── Commands/CreateEvent/
│   └── Queries/ListEvents/
├── Reservations/
│   ├── Commands/
│   │   ├── CreateReservation/
│   │   ├── ConfirmPayment/
│   │   └── CancelReservation/
│   └── Queries/ListReservations/
└── Reports/
    └── Queries/EventOccupancy/
```

---

## Key design decisions

**Validators run before handlers** via `ValidationBehavior<TRequest, TResponse>`.
They validate shape and format only — business rules (capacity, timing, limits) belong in handlers.

**`CreateReservationHandler`** is the most complex handler — it delegates all business rule evaluation to `ReservationValidationService` (Domain layer), then calls `Reservation.Create`.

**`ConfirmPaymentHandler`** generates the `EV-{6digits}` code with a retry loop on collision before calling `reservation.ConfirmPayment(code)`.

**Queries never mutate state.** Handlers for queries only read and project to DTOs.

**Commands return only IDs or simple scalars** — never full entities or domain objects.

---

## Hard rules

- No direct EF Core queries — always go through repository interfaces
- No domain exceptions swallowed silently — let them propagate to middleware
- Always `DateTime.UtcNow`, never `DateTime.Now`
- Queries return DTOs, never domain entities
- User-facing error messages stay in Spanish; all identifiers and comments in English
