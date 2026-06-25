# CLAUDE.md — EventosVivos (root)

## What this project is

Backend REST API for **EventosVivos**, a cultural event reservation system.
Core problem: real-time capacity control — preventing overselling, venue
schedule conflicts, and time-dependent reservation lifecycle rules.

Stack: .NET 10 · Clean Architecture · DDD lite · PostgreSQL · JWT auth

---

## How to work on this project

Before touching any file, identify which layer it belongs to and read that layer's CLAUDE.md first.

| Layer | CLAUDE.md | Owns |
|---|---|---|
| Domain | `EventosVivos.Domain/CLAUDE.md` | Entities, specs, exceptions, enums |
| Application | `EventosVivos.Application/CLAUDE.md` | Use cases, handlers, validators, DTOs |
| Infrastructure | `EventosVivos.Infrastructure/CLAUDE.md` | EF Core, repos, JWT, migrations |
| API | `EventosVivos.API/CLAUDE.md` | Controllers, middleware, Program.cs |
| Tests | `tests/CLAUDE.md` | Unit + integration tests |

**Never write business logic outside Domain or Application.**
**Never import Infrastructure types into Domain.**

---

## Solution structure

```
EventosVivos/
├── CLAUDE.md                              ← you are here
├── EventosVivos.slnx
├── EventosVivos.Domain/
├── EventosVivos.Application/
├── EventosVivos.Infrastructure/
├── EventosVivos.API/
└── tests/
    ├── EventosVivos.Domain.Tests/
    └── EventosVivos.Application.Tests/
```

---

## Business rules — single source of truth

### RF-03 — Reservation creation priority

Apply in this exact order — earlier rules take priority:

1. **RN-04** Event starts in < 1h → reject
2. **RN-01** Not enough capacity → reject
3. **RF-03-24h** Starts in < 24h → max 5 tickets (overrides RN-05)
4. **RN-05** Price > $100 → max 10 tickets
5. Quantity < 1 → reject

### RN-01 — Capacity formula

```
available = maxCapacity - confirmedTickets - lostTickets
```

Never store `available` as a column. Always compute it.

### RN-02 — Venue overlap

Two events conflict when they share a venue, both are active, and:
```
A.Start < B.End  &&  A.End > B.Start
```

### RN-03 — Weekend night restriction

Saturday or Sunday → cannot start at or after 22:00.

### Event.Status — derived, never persisted

```csharp
public EventStatus Status =>
    _cancelled              ? EventStatus.Cancelled  :
    UtcNow > EndDateTime    ? EventStatus.Completed  :
                              EventStatus.Active;
```

Only `_cancelled` (bool) is persisted. `Completed` is always time-derived.

### RN-07 — Cancellation penalty

If cancelling a confirmed reservation within 48h of event start:
- Set `IsLost = true` on the reservation
- Do NOT return tickets to the available pool

### RF-04 — Reservation code format

`EV-{6 digits}` — generated server-side, unique, not sequential.

---

## Shared conventions

- All `DateTime` stored and compared in **UTC**. Always `DateTime.UtcNow`.
- Commands mutate state. Queries never mutate state.
- FluentValidation runs before handlers — invalid input never reaches a handler.
- Repositories return domain entities, not EF models or DTOs.
- No business logic in controllers.
- User-facing messages (API error responses, validation messages) stay in Spanish.
- All code identifiers, comments, and error codes in English.

---

## Running locally

```bash
docker run -d --name eventosvivos-db \
  -e POSTGRES_PASSWORD=<password> -e POSTGRES_DB=eventosvivos \
  -p 5432:5432 postgres:16

dotnet ef database update --project EventosVivos.Infrastructure \
  --startup-project EventosVivos.API

dotnet run --project EventosVivos.API
dotnet test
```

`appsettings.Development.json` (gitignored — create locally, never commit):
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=eventosvivos;Username=<user>;Password=<password>"
  },
  "Jwt": {
    "Key": "<256-bit-secret>",
    "Issuer": "EventosVivos.API",
    "ExpiresInMinutes": 60
  }
}
```
