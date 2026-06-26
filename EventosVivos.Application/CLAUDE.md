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
├── Handlers/                           ← one file per use case
│   ├── Auth/
│   │   ├── LoginHandler.cs
│   │   └── RegisterUserHandler.cs
│   ├── Events/
│   │   ├── CreateEventHandler.cs
│   │   ├── GetEventByIdHandler.cs
│   │   └── ListEventsHandler.cs
│   ├── Reports/
│   │   └── EventOccupancyHandler.cs
│   └── Reservations/
│       ├── CancelReservationHandler.cs
│       ├── ConfirmPaymentHandler.cs
│       ├── CreateReservationHandler.cs
│       └── ListReservationsHandler.cs
├── Models/                             ← commands, queries, DTOs, validators
│   ├── Auth/
│   │   ├── LoginCommand.cs
│   │   ├── LoginValidator.cs
│   │   ├── RegisterUserCommand.cs
│   │   └── RegisterUserValidator.cs
│   ├── Events/
│   │   ├── CreateEventCommand.cs
│   │   ├── CreateEventValidator.cs
│   │   ├── EventDto.cs
│   │   ├── GetEventByIdQuery.cs
│   │   └── ListEventsQuery.cs
│   ├── Reports/
│   │   ├── EventOccupancyDto.cs
│   │   └── EventOccupancyQuery.cs
│   └── Reservations/
│       ├── CancelReservationCommand.cs
│       ├── ConfirmPaymentCommand.cs
│       ├── CreateReservationCommand.cs
│       ├── CreateReservationValidator.cs
│       ├── ListReservationsQuery.cs
│       └── ReservationDto.cs
├── CLAUDE.md
├── DependencyInjection.cs
└── EventosVivos.Application.csproj
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
- All user-facing error messages in English; all identifiers and comments in English
