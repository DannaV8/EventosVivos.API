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

**ReservationValidationService** orchestrates capacity and transaction-limit specs in the correct priority order (see root CLAUDE.md RF-03 section).

**Entities cannot be constructed in an invalid state** — use static factory methods, never public constructors.

---

## Hard rules

- No `using Microsoft.EntityFrameworkCore`
- No async/await — domain is synchronous
- No `ILogger` or any logging
- No DTOs or ViewModels
- No `[Required]`, `[MaxLength]` or data annotation attributes
- Always `DateTime.UtcNow`, never `DateTime.Now`
- User-facing error messages (exception messages) stay in Spanish
- All other code (identifiers, comments) in English
