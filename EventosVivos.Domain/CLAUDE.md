# CLAUDE.md — EventosVivos.Domain

Read the root CLAUDE.md before working here.

## What this layer is

The core of the system. **Zero dependencies on any other project layer.**
No EF Core. No HTTP. No MediatR. Just C# and business rules.

If you find yourself importing from Infrastructure or Application, stop — the design is wrong.

---

## Structure

```
EventosVivos.Domain/
├── Entities/
│   ├── Event.cs
│   ├── Reservation.cs
│   ├── User.cs
│   └── Venue.cs
├── Enums/
│   ├── EventType.cs          (Conference, Workshop, Concert)
│   ├── EventStatus.cs        (Active, Cancelled, Completed)
│   └── ReservationStatus.cs  (PendingPayment, Confirmed, Cancelled)
├── Specifications/
│   ├── ReservationCapacitySpec.cs
│   ├── ReservationTransactionLimitSpec.cs
│   └── VenueAvailabilitySpec.cs
├── Services/
│   └── ReservationValidationService.cs
└── Exceptions/
    ├── DomainException.cs                  ← base class
    ├── CapacityExceededException.cs
    ├── VenueConflictException.cs
    ├── NightScheduleException.cs
    ├── InvalidReservationStateException.cs
    ├── EventSoonException.cs
    ├── InvalidEventException.cs
    ├── InvalidCredentialsException.cs
    └── InvalidUserException.cs
```

---

## Key design decisions

**EventStatus is computed, never persisted.**
Only `_cancelled` (bool) is stored. `Completed` is always derived from `EndDateTime` vs `UtcNow`.

**Specifications never throw** — they return a result tuple `(bool isValid, string? code, string? message)`.
The caller (handler or service) decides whether to throw based on the result.

**ReservationValidationService** orchestrates capacity and transaction-limit specs plus quantity validation in the correct priority order per RF-03 (see root CLAUDE.md RF-03 section). The `quantity < 1` check is the last priority, evaluated only after capacity and transaction limits pass. The service is synchronous — it receives a pre-loaded `Event` (with reservations) and the requested quantity, and throws the first applicable `DomainException`.

**Entities cannot be constructed in an invalid state** — use static factory methods (`Event.Create`, `Reservation.Create`, `User.Create`) with inline validation, never public constructors.

**Inline validations in factory methods** (no Value Objects, no EF converters needed):
- `Event.Create` — throws if `price < 0`
- `Reservation.Create` — validates `buyerEmail` against `^[^@\s]+@[^@\s]+\.[^@\s]+$`
- `Reservation.ConfirmPayment` — validates code against `^EV-\d{6}$`
- `User.Create` — validates `email` against `^[^@\s]+@[^@\s]+\.[^@\s]+$`

---

## Hard rules

- No `using Microsoft.EntityFrameworkCore`
- No async/await — domain is synchronous
- No `ILogger` or any logging
- No DTOs or ViewModels
- No `[Required]`, `[MaxLength]` or data annotation attributes
- Always `DateTime.UtcNow`, never `DateTime.Now`
- All user-facing error messages (exception messages) in English
- All other code (identifiers, comments) in English
