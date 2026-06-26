# CLAUDE.md — EventosVivos UI (Angular 22 + Tailwind)

> Mueve este archivo a la raíz del proyecto Angular como `CLAUDE.md` después de
> `ng new eventos-vivos-ui --style=css --routing`.

## Qué es este proyecto

Frontend SPA de **EventosVivos**, consume la API REST de reservas de eventos.
No SSR (es una app de gestión, no necesita SEO).

Stack: **Angular 22** (standalone components, signals, `@if`/`@for`, `inject()`) ·
**Tailwind CSS** · JWT en `localStorage`.

API base (dev): `https://localhost:63801/api`

---

## Roles y flujo de acceso

| Rol | Puede |
|---|---|
| **Anónimo** (sin login) | Ver lista de eventos y detalle. NADA más. |
| **User** (logueado) | Reservar, ver SUS reservas, cancelar SUS reservas. |
| **Admin** (logueado) | Todo lo de user + crear eventos, confirmar pagos, cancelar cualquier reserva, ver reportes. |

**Regla clave del flujo:** un usuario anónimo ve los eventos, pero cuando hace clic en
**"Reservar"** se le redirige a `/login`. Tras autenticarse, vuelve a completar la reserva.
Cancelar una reserva solo es posible logueado (control de quién cancela).

---

## Trabajo por subagentes (por feature, no por capa)

Igual que el backend usa un CLAUDE.md por capa, aquí hay uno por feature.
Antes de tocar archivos, identifica el feature y lee su CLAUDE.md.

```
src/app/
├── core/            ← auth service, JWT interceptor, guards, api base  (core/CLAUDE.md)
├── shared/          ← componentes UI reutilizables (card, badge, modal, tabla)  (shared/CLAUDE.md)
├── features/
│   ├── eventos/     ← lista pública + detalle  (features/eventos/CLAUDE.md)
│   ├── reservas/    ← crear reserva + "mis reservas"  (features/reservas/CLAUDE.md)
│   ├── auth/        ← login + registro  (features/auth/CLAUDE.md)
│   └── admin/       ← dashboard, gestión reservas, crear evento, reportes  (features/admin/CLAUDE.md)
└── app.routes.ts    ← rutas + guards
```

---

## Estructura de rutas

```
/                      → lista de eventos (pública)
/eventos/:id           → detalle de evento (pública)
/login                 → login
/registro              → registro (crea siempre rol "user")
/mis-reservas          → reservas del usuario [guard: auth]
/admin                 → dashboard admin [guard: admin]
/admin/reservas        → gestión de reservas [guard: admin]
/admin/eventos/nuevo   → crear evento [guard: admin]
/admin/reportes        → reportes de ocupación [guard: admin]
```

---

## Core — autenticación

### AuthService (signals)
- `login(email, password)` → POST `/auth/login`, guarda token en `localStorage`
- `registro(email, password)` → POST `/auth/registro`
- `logout()` → borra token, redirige a `/`
- `token = signal<string | null>` · `isAuthenticated = computed(...)`
- `rol = computed(...)` → decodifica el claim `rol` del JWT (parsear el payload base64)
- `isAdmin = computed(() => rol() === 'admin')`

### JWT Interceptor (functional)
- Agrega `Authorization: Bearer {token}` a cada request si hay token.
- En `401` → `logout()` + redirige a `/login`.

### Guards (functional)
- `authGuard` → exige estar logueado (si no, redirige a `/login` con returnUrl)
- `adminGuard` → exige `isAdmin()`

---

## Endpoints de la API

```
PÚBLICO
GET  /api/eventos                    → lista (filtros: tipo, venueId, estado, titulo, fechaInicio, fechaFin)
GET  /api/eventos/{id}/reporte       → reporte de ocupación  (NOTA: requiere auth hoy)

AUTH
POST /api/auth/registro              → { id }   (siempre rol "user")
POST /api/auth/login                 → { token }

USER (Bearer)
POST /api/reservas                   → { id }   (NO mandar usuarioId, va en el JWT)
GET  /api/reservas                   → reservas del usuario logueado
PUT  /api/reservas/{id}/cancelar     → cancela (solo propias, o cualquiera si admin)

ADMIN (Bearer + rol admin)
POST /api/eventos                    → { id }
PUT  /api/reservas/{id}/confirmar    → { codigoReserva }
```

### DTOs relevantes

```ts
// Evento (de GET /api/eventos)
interface Evento {
  id: string; titulo: string; descripcion: string;
  venueId: number; venueNombre: string;
  capacidadMaxima: number;
  fechaHoraInicio: string; fechaHoraFin: string;  // ISO UTC
  precioEntrada: number;
  tipo: 'Conferencia' | 'Taller' | 'Concierto';
  estado: 'Activo' | 'Cancelado' | 'Completado';
}

// CrearReserva (POST body — sin usuarioId)
interface CrearReservaRequest {
  eventoId: string; cantidad: number;
  nombreComprador: string; emailComprador: string;
}
```

### Enums (mandar como número en POST, llegan como texto en GET)
- `tipo`: 0=Conferencia, 1=Taller, 2=Concierto
- `estado`: 0=Activo, 1=Cancelado, 2=Completado
- `venueId`: 1=Auditorio Central, 2=Sala Norte, 3=Arena Sur

---

## Pantallas (según mockups)

### Lista de eventos (pública) — `/`
- Header con título + botón "Admin" (login si no está logueado)
- Filtros: dropdowns tipo / lugar / estado, dos date pickers, botón "Limpiar filtro", buscador
- Grid de cards (3 columnas): imagen placeholder, badges (tipo + estado), título, venue, precio, fecha, ocupación (X/Y)
- Paginación abajo (cliente o servidor)
- Clic en card → detalle. Botón "Reservar" → si anónimo, va a `/login`

### Gestión de reservas (admin) — `/admin/reservas`
- Header con tabs: Eventos · Reservas · Reportes
- Métricas arriba: Confirmados · Pendientes · Disponibles · Ingresos
- Buscador + filtro por estado
- Tabla: Comprador, Email, Cant, Estado, Código, Acciones (botón "Confirmar")
- Paginación abajo con total

---

## ⚠️ Ajustes pendientes en la API (coordinar con backend)

1. **`GET /api/reservas` para admin** — hoy devuelve solo las del usuario logueado.
   El dashboard admin necesita TODAS las reservas. Falta un endpoint admin
   (ej. `GET /api/admin/reservas`) o que el actual devuelva todas si el rol es admin.
2. **Métricas del dashboard** (confirmados/pendientes/disponibles/ingresos) — no hay
   un endpoint agregado. Habría que crearlo o calcular en el cliente desde la lista.
3. **`GET /api/eventos/{id}`** (detalle) — no existe; hoy solo está el `/reporte`.
   Para la pantalla de detalle se puede filtrar la lista o agregar el endpoint.

---

## Tailwind — convenciones

- Tema oscuro (los mockups son dark). Definir colores base en `tailwind.config`.
- Badges de tipo/estado con colores consistentes (ej. Activo=verde, Cancelado=rojo).
- Componentes de UI reutilizables en `shared/` (no repetir clases sueltas).

---

## Reglas duras de esta capa

- **Nunca** mandar `usuarioId` o `rol` desde el cliente en bodies — vienen del JWT en el backend.
- Todas las fechas se manejan en **UTC** (la API las espera/devuelve con `Z`).
- No guardar lógica de negocio en el front — solo presentación y orquestación de llamadas.
- El token va en `localStorage`; el interceptor lo inyecta. No pegarlo a mano en servicios.
- Manejar el `estado` y `tipo` como texto al leer (GET) y como número al escribir (POST).
```
