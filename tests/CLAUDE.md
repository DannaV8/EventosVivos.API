# CLAUDE.md — EventosVivos Tests

Read the root CLAUDE.md before working here.

## Two test projects, two purposes

```
tests/
├── EventosVivos.Domain.Tests/          ← Unit tests, zero infrastructure
│   ├── EventTests.cs
│   ├── ReservationTests.cs
│   ├── ReservationCapacitySpecTests.cs
│   ├── ReservationValidationServiceTests.cs
│   └── TestData.cs
│
└── EventosVivos.Application.Tests/     ← Integration tests, EF in-memory
    ├── HandlerTestBase.cs
    ├── TestData.cs
    ├── Reservations/
    │   ├── CreateReservationHandlerTests.cs
    │   ├── ConfirmPaymentHandlerTests.cs
    │   └── CancelReservationHandlerTests.cs
    └── Events/
        └── CreateEventHandlerTests.cs
```

---

## Domain.Tests — unit tests

No database. No mocks for domain types. Test the rules directly.

Every business rule from the root CLAUDE.md must have at least one test.

---

## Application.Tests — integration tests

Use `EF Core InMemory` provider + real handlers. No mocks for repositories —
use the real implementations against an in-memory DB.

Each test class gets a fresh database instance via `HandlerTestBase`
(new `Guid.NewGuid()` database name per test class).

### Key scenarios covered

- Successful reservation creates a `PendingPayment` record
- Cannot exceed event capacity
- Cannot reserve less than 1h before event start (RN-04)
- Lost tickets do not count as available (RN-07)
- 24h rule has priority over price rule (RF-03)
- Payment confirmation generates a code matching `EV-\d{6}`
- Cannot confirm an already confirmed reservation
- Venue conflict is detected when two events overlap (RN-02)
- Capacity greater than venue capacity is rejected

---

## Naming convention

`{Subject}_{Condition}_{ExpectedResult}`

Examples:
- `Cancel_Within48hWindow_IsLost`
- `Status_WithPastEndDate_IsCompleted`
- `HasConflict_OverlappingRanges_ReturnsTrue`

---

## Hard rules for this layer

- One assertion focus per test — if a test has 5 unrelated asserts, split it
- No `Thread.Sleep` or real `DateTime.Now` dependencies in unit tests — pass `eventStart` explicitly so tests are deterministic
- Domain.Tests must have zero infrastructure dependencies
- Each Application.Tests test gets its own in-memory database instance — never share state between tests
