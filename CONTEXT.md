# TuColmadoRD — Contexto del Proyecto

ERP/POS SaaS para colmados dominicanos. Versión actual: `0.3.0`.

---

## Stack exacto y versiones

### Frontend
| Servicio | Tecnología | Versiones clave |
|----------|-----------|-----------------|
| Web admin (SPA) | Angular | 21.2.0 |
| Web admin | TypeScript | ~5.9.2 |
| Landing page | Vue | 3.5.32 |
| Landing page | Vite | 8.0.8 |
| Landing page | TailwindCSS | 4.3.0 |
| Landing page | DaisyUI | 5.5.20 |

### Backend
| Servicio | Tecnología | Versiones clave |
|----------|-----------|-----------------|
| Core API | .NET / ASP.NET Core | 10.0 |
| API Gateway | .NET / ASP.NET Core | 10.0 |
| EF Core | Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 |
| Auth service | Node.js + Express | Express 5.2.1 |
| Auth service | bcryptjs (NO bcrypt) | 3.0.3 |
| Auth service | jsonwebtoken (HS256) | 9.0.3 |
| Auth service | mongoose | 9.6.2 |
| Notification service | Node.js + Express | Express 5.2.1 |
| Notification service | BullMQ + ioredis | BullMQ 5.76.9 |
| Notification service | nodemailer | 8.0.7 |
| Catalog service | Rust | imagen pre-compilada (`ghcr.io/odimsom/catalog-service:main`) |
| Reports service | Rust | imagen pre-compilada (`ghcr.io/odimsom/reports-service:main`) |
| ECF generator | Python | servicio separado en `:5000` |

### Bases de datos e infraestructura
| Componente | Versión |
|-----------|---------|
| PostgreSQL | 15 |
| MongoDB | 7 |
| Redis | 7-alpine |
| Traefik | v2.11 |
| Prometheus | v2.53.0 |
| Grafana | 11.1.0 |
| MailHog | latest (solo desarrollo/staging) |

### Tests
- E2E: Playwright — `frontend/web-admin/playwright.prod.config.ts`
- Unit backend: `dotnet test` contra `TuColmadoRD.slnx`
- Unit auth: Jest (`pnpm test` en `/auth`)

---

## Arquitectura

```
Internet
    │
    ▼
Traefik v2.11 (TLS / Let's Encrypt)
    ├── tucolmadord.com / www.tucolmadord.com
    │       └── Landing Page  (Vue 3 + nginx, :8082)
    │
    ├── app.tucolmadord.com
    │       └── Web Admin SPA  (Angular 21 + nginx, :8083)
    │
    ├── api.tucolmadord.com
    │       └── API Gateway  (.NET 10, :8081 → interno :8080)
    │               ├── /gateway/auth/*  → Auth Service  (Node.js, :3000)
    │               └── /gateway/*       → Core API      (.NET 10, :8080)
    │
    └── devops.tucolmadord.com
            ├── /prometheus  → Prometheus (:9090)
            └── /grafana     → Grafana (:3000)

Servicios internos (no expuestos directamente):
    Auth Service ──► Notification Service  (Node.js, :4000)
                         └── cola BullMQ en Redis
    Core API ────► ECF Generator  (Python, :5000)
    Core API ────► Catalog Service  (Rust, :8080 interno)
    Core API ────► Reports Service  (Rust, :8081 interno)
    Core API ────► Auth Service  (validación JWT)

Bases de datos:
    MongoDB 7       ← Auth Service  (DB: tu_colmado_auth)
    PostgreSQL 15   ← Core API, Catalog Service, Reports Service
    Redis 7         ← BullMQ (colas), caché (catalog/reports)
```

### Capas del backend .NET (Clean Architecture)
```
Presentations/
  TuColmadoRD.Presentation.API     ← API REST principal
  TuColmadoRD.ApiGateway            ← Proxy/gateway
  TuColmadoRD.Desktop               ← Electron POS (Windows)
core/
  TuColmadoRD.Core.Domain           ← Entidades y contratos
  TuColmadoRD.Core.Application      ← Casos de uso / CQRS
infrastructure/
  TuColmadoRD.Infrastructure.Persistence  ← EF Core + Npgsql
  TuColmadoRD.Infrastructure.IOC          ← DI / composición raíz
  TuColmadoRD.Infrastructure.CrossCutting
  TuColmadoRD.Infrastructure.Hosts
```

### Layouts del web-admin Angular
- `/portal/*` — layout escritorio (Owner, Admin): dashboard, inventario, ventas, compras, cajas, clientes, empleados, deliveries, reportes, configuración
- `/pos` — layout POS táctil (Owner, Admin, Seller, Cashier)
- `/delivery` — layout entregas (Delivery)

### Token y sesión (frontend)
- JWT HS256 almacenado en `localStorage` bajo las claves `tc_token`, `tc_user`, `tc_tenant`
- El gateway valida el JWT antes de enrutar al Core API o Auth Service

### CD/CI (branch strategy)
```
dev ──► [CI: unit tests (.NET + auth)] ──► qa
qa  ──► [CI: integration tests]         ──► auto-PR a main
main ──► [CD: self-hosted runner → VPS 177.7.48.169]
```
Workflows:
- `.github/workflows/ci-dev-to-qa.yml`
- `.github/workflows/ci-qa-to-main.yml`
- `.github/workflows/cd-deploy.yml`
- `.github/workflows/ci-services.yml`
- `.github/workflows/release-test.yml`

---

## Decisiones ya tomadas — NO cambiar

### Autenticación
- JWT **HS256 simétrico** (`JWT_SECRET` en `.env`). No se migrará a RSA/RS256.
- La librería de hashing es **bcryptjs** (NO `bcrypt` nativo). El hash se genera dentro del contenedor `tucolmadord-auth-1` con `require('bcryptjs')`.
- El campo en MongoDB es **`password`** (no `passwordHash`). El UserModel en auth usa ese nombre de campo.
- La BD de auth en MongoDB es **`tu_colmado_auth`** (no `tucolmadord_auth`).
- El tenant debe existir en **dos sitios**: colección `tenants` de MongoDB Y tabla `System.TenantProfiles` en PostgreSQL.

### Frontend
- El token se guarda en **localStorage** (no cookies, no sessionStorage). Las claves son `tc_token`, `tc_user`, `tc_tenant`.
- Navegación SPA: usar **`expect(page).toHaveURL()`** en Playwright, nunca `waitForURL()` (pushState de Angular dispara antes del hook).
- Playwright con **`trace: 'off'` y `video: 'off'`** en `playwright.prod.config.ts`. Habilitarlos causa un hang de 56 s por teardown (las conexiones WebSocket del dashboard impiden que el contexto del browser se cierre).
- El tab "Catálogo" en POS es **`sm:hidden`** (solo móvil). No intentar interactuar con él en tests de viewport escritorio.

### Infraestructura y despliegue
- **Docker Compose** con ~15 contenedores. No se está planificando migración a Kubernetes.
- **Self-hosted runner** en el VPS (Hostinger, `root@177.7.48.169`). El CD corre directamente en producción vía `executions/deploy-production.sh`.
- **`main` tiene branch protection** — no se puede hacer push directo. Todo entra por PR.
- El release se versiona desde `frontend/package.json` → campo `"version"`. El tag de GitHub se construye como `v{version}`.
- PostgreSQL monta el volumen en `/var/lib/tucolmadord/postgres_data` y MongoDB en `/var/lib/tucolmadord/mongo_data` (bind mount en el VPS).

### Servicios externos
- **Twilio** para WhatsApp (notification-service). Las credenciales van por env vars: `TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM`.
- **Resend** como alternativa de email en producción (`RESEND_API_KEY`). MailHog solo en staging/local.
- **ECF generator** (`generadordexmle-cf`) es el generador de comprobantes fiscales electrónicos (XML) requerido por la DGII dominicana.

---

## Estado actual

### Hecho y funcionando en producción
- Autenticación completa: login, registro, verificación de email, logout
- Dashboard con 5 stat cards y carga de datos reales
- Inventario: listado, búsqueda, creación de productos (con categorías)
- Clientes: listado, creación, estado de cuenta
- POS: apertura de turno, catálogo, carrito, búsqueda
- Gastos (expenses): listado, registro
- Ventas: listado, detalle de venta
- Página de licencia/suscripción (CTA a WhatsApp)
- Deliveries: estructura de rutas creada
- Landing page (Vue 3, completa)
- Pipeline CI/CD (dev → qa → main → deploy)
- Traefik con TLS automático (Let's Encrypt)
- Monitoreo (Prometheus + Grafana)
- Notification service (email + WhatsApp)
- ECF generator (comprobantes fiscales)
- Suite E2E Playwright: **32 tests pasan, 1 skip intencional**
  - 01-auth, 02-dashboard, 03-customers, 04-pos, 05-expenses, 06-inventory, 07-connectivity, 08-license

### En progreso / incompleto
- **Compras** (`/portal/compras`): ruta creada, page básica, sin flujo completo
- **Cajas** (`/portal/cajas`): ruta creada, sin implementación visible
- **Empleados** (`/portal/empleados`): estructura de carpetas existe, status incierto
- **Reportes** (`/portal/reportes`): ruta creada, sin implementación visible
- **Configuración** (`/portal/configuracion`): ruta creada, sin implementación visible
- **Desktop Electron** (`TuColmadoRD.Desktop`): proyecto .csproj existe, estado de distribución incierto

### Gotchas conocidos / cosas rotas
- **El usuario de prueba se borra periódicamente** en el VPS (redeployments o procesos de fondo lo eliminan). Si el login falla en e2e, hay que recrearlo manualmente (ver `e2e-testing.md` en memory).
- **"Abrir turno" aparece en header Y body del POS** — locator en Playwright siempre requiere `.first()` o un `hasText` más específico para evitar violación de strict mode.
- **Conectividad offline en tests** — el `afterEach` DEBE llamar `context.setOffline(false)`, de lo contrario el estado offline se filtra al siguiente test causando `ERR_INTERNET_DISCONNECTED`.
- **EF Core versión mixta**: la capa de Persistence usa EF Core 9.0.16 pero las Tools son 10.0.8. Esto es intencional (Npgsql aún no tiene release estable para EF Core 10).
- **`Microsoft.AspNetCore.Hosting` 2.3.10** en Infrastructure.Persistence es una referencia legacy que no debería crecer.
