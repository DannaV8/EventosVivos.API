URL: https://eventosvivos-api-q5jw.onrender.com/swagger/index.html

<img width="751" height="468" alt="image" src="https://github.com/user-attachments/assets/b96d005b-ba55-4dec-9110-90dc71ce7fd5" />

# EventosVivos API

API REST para reserva de eventos culturales. El núcleo del problema es el **control de aforo
en tiempo real**: evitar sobreventa, conflictos de horario en venues y reglas de reserva
dependientes del tiempo.

**Stack:** .NET 10 · Clean Architecture · DDD lite · CQRS (MediatR) · EF Core · PostgreSQL · JWT

---

## Arquitectura

Clean Architecture en 4 capas. Las reglas de negocio viven **solo** en Domain y Application.

```
API  →  Application (CQRS)  →  Domain
              ↓
        Infrastructure (EF Core · PostgreSQL · JWT)
```

| Capa | Responsabilidad |
|---|---|
| **Domain** | Entidades, specifications, servicios y excepciones. Cero dependencias. |
| **Application** | Casos de uso (handlers MediatR), validators (FluentValidation), DTOs, interfaces. |
| **Infrastructure** | EF Core, repositorios, JWT, candado de concurrencia, migraciones. |
| **API** | Controllers, middleware, auth, rate limiting (`Program.cs`). |

---

## Modelo de datos

- **Venues** (`Id int`) `1—N` **Events** (`Id uuid`) `1—N` **Reservations** (`Id uuid`)
- **Users** (`Id uuid`) `1—N` **Reservations**
- `Event.Status` es **calculado**, no se persiste (solo `IsCancelled` + `RowVersion` para concurrencia).

---

## Endpoints

| Método | Ruta | Acceso |
|---|---|---|
| POST | `/api/auth/register` | público |
| POST | `/api/auth/login` → `{ token }` | público |
| GET | `/api/events` (filtros + paginación) | público |
| GET | `/api/events/{id}` | público |
| POST | `/api/events` | Admin |
| GET | `/api/events/{id}/report` | autenticado |
| GET | `/api/reservations` | autenticado |
| POST | `/api/reservations` | autenticado |
| PUT | `/api/reservations/{id}/confirm` | Admin |
| PUT | `/api/reservations/{id}/cancel` | autenticado |

Filtros de `GET /api/events`: `type`, `startDate`, `endDate`, `venueId`, `status`, `title`, `page`, `pageSize`.

---

## Reglas de negocio clave

- **Aforo:** `disponible = maxCapacity − confirmados − perdidos` (nunca se almacena, siempre se calcula).
- **Prioridad al reservar:** evento en < 1h → rechaza · sin cupo → rechaza · < 24h → máx 5 · precio > $100 → máx 10.
- **Solapamiento de venue:** dos eventos chocan si comparten venue y `A.Start < B.End && A.End > B.Start`.
- **Restricción nocturna:** sábado/domingo no pueden iniciar a las 22:00 o después.
- **Penalización de cancelación:** cancelar dentro de 48h de una reserva confirmada → `IsLost = true`, no devuelve cupo.
- **Código de reserva:** `EV-{6 dígitos}`, único, generado server-side.
- Concurrencia controlada con `RowVersion` (409 en conflicto) + candado por `EventId`.

---

## Seguridad

- **JWT Bearer**, token de 1h. Claim `rol` (`admin` | `user`); endpoints admin con política `AdminOnly`.
- **Rate limiting** (fixed-window por IP, configurable en `appsettings.json`):
  - Global: 100 req/min · Auth (`/api/auth/*`): 5 req/min.
  - Al exceder → `429` con `{ "error": "RATE_LIMIT_EXCEEDED", ... }`.
- Errores con formato uniforme `{ error, message, detail }` vía middleware.

---

## Cómo correr

```bash
# 1. PostgreSQL en Docker
docker run -d --name eventosvivos-db \
  -e POSTGRES_PASSWORD=<password> -e POSTGRES_DB=eventosvivos \
  -p 5432:5432 postgres:16

# 2. Migraciones
dotnet ef database update --project EventosVivos.Infrastructure \
  --startup-project EventosVivos.API

# 3. Ejecutar API (+ Swagger en /swagger)
dotnet run --project EventosVivos.API

# 4. Tests
dotnet test
```

`EventosVivos.API/appsettings.Development.json` (gitignored — créalo localmente):

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=eventosvivos;Username=<user>;Password=<password>"
  },
  "Jwt": { "Key": "<256-bit-secret>", "Issuer": "EventosVivos.API", "ExpiresInMinutes": 60 }
}
```
##Diagramas
- **ER:**
<img width="805" height="332" alt="Eventos vivos-ER drawio" src="https://github.com/user-attachments/assets/f5cd1ea8-6a79-43ff-920a-90a730108870" />

- **Arquitectura:**
<img width="698" height="1817" alt="Eventos vivos-Architecture drawio (1)" src="https://github.com/user-attachments/assets/fc187f7b-2d75-473b-816d-cf6a2c7313c6" />

- **Diagrama de usuario:**
<img width="623" height="641" alt="Eventos vivos-User Diagram drawio" src="https://github.com/user-attachments/assets/3aa7f1c7-2924-4640-9a7f-d101ddd01b71" />
