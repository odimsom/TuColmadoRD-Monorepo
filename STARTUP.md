# TuColmadoRD — Guía de inicio local

## Prerrequisitos

| Herramienta | Versión mínima |
|---|---|
| Docker + Docker Compose | 24+ |
| Node.js | 20+ |
| .NET SDK | 9.0 |
| Python | 3.11+ |

---

## Opción A — Docker Compose (stack completo)

```bash
# En la raíz del monorepo
docker compose up --build -d
```

Servicios expuestos:

| Servicio | URL local |
|---|---|
| Gateway (API pública) | http://localhost:5032 |
| Frontend web-admin | http://localhost:5209 |
| Auth service | http://localhost:5239 |
| .NET Core API | http://localhost:5229 |
| e-CF Generator | http://localhost:5000 |
| PostgreSQL | localhost:54329 |
| MongoDB | localhost:27019 |

> Primera vez: las migraciones de EF Core se aplican automáticamente al arrancar el backend.

---

## Opción B — Servicios individuales (desarrollo activo)

Levanta los servicios en este orden.

### 1. Bases de datos (Docker)

```bash
docker compose up postgres mongo -d
```

Espera ~10 s y valida:
```bash
docker compose ps   # ambos "healthy"
```

### 2. Auth service (Node.js)

```bash
cd auth
npm install
# auth/.env ya tiene las variables correctas (JWT_SECRET, MONGODB_URI, PORT)
npm run dev
```

Valida: `GET http://localhost:5239/health` → `{ status: "ok" }`

### 3. e-CF Generator (Python)

```bash
cd generadordexmle-cf
pip install -r requirements.txt
python app.py          # escucha en :5000
```

Valida:
```bash
curl http://localhost:5000/
# {"status":"TuColmadoRD - Generador de e-CF", "version":"..."}
```

### 4. .NET Core API

```bash
cd backend/src/Presentations/TuColmadoRD.Presentation.API
dotnet run
# o: dotnet watch run
```

Variables de entorno necesarias (o `appsettings.Development.json`):
```
ConnectionStrings__DefaultConnection=Host=localhost;Port=54329;Database=TuColmadoDb;Username=postgres;Password=1234
EcfGenerator__BaseUrl=http://localhost:5000
```

Las migraciones se aplican al arrancar. Valida: `GET http://localhost:5229/health`

### 5. Gateway YARP

```bash
cd backend/src/Presentations/TuColmadoRD.Presentation.Gateway
dotnet run
```

Variables requeridas:
```
GatewayOptions__AuthApiUrl=http://localhost:3000
GatewayOptions__CoreApiUrl=http://localhost:5070
GatewayOptions__JwtSecret=dominican-street-premium-secret-key-2026
```
> En desarrollo local los valores en `appsettings.json` ya son correctos; no se necesitan las variables de entorno.

Escucha en `:5032`.

### 6. Frontend web-admin

```bash
cd frontend/web-admin
npm install
npm run start         # ng serve → http://localhost:4200
```

> En desarrollo local el `environment.ts` apunta a `gatewayUrl: 'http://localhost:5032'`.

---

## Seed de cuentas de prueba

Una vez que PostgreSQL y MongoDB estén corriendo:

```bash
cd scripts
npm install               # instala mongoose, bcryptjs, pg, tsx
npx tsx seed-test-accounts.ts
```

Salida esperada:
```
✅ Seed completado.
Tenant ID : <uuid>
Password  : Pruebas@1234

Cuentas de acceso:
  owner@pruebassrl.com    (owner)    → /portal
  cajero@pruebassrl.com   (seller)   → /pos
  delivery@pruebassrl.com (delivery) → app móvil/delivery
```

---

## Flujo de prueba manual

1. Abre http://localhost:5209 (o http://localhost:4200 en dev)
2. Login con `owner@pruebassrl.com` / `Pruebas@1234`
3. Ingresa al **Portal** → Dashboard → verifica cards
4. Ve a **Configuración** → completa el perfil del negocio (RNC, dirección)
5. Ve a **Inventario** → agrega productos con distintas tasas ITBIS
6. Abre el **POS** (o login con `cajero@pruebassrl.com` en otra pestaña)
7. Realiza una venta → verifica que el recibo muestra NCF y datos reales del negocio

---

## Problemas frecuentes

| Síntoma | Causa probable | Solución |
|---|---|---|
| Auth devuelve 401 en `/portal` | JWT_SECRET distinto entre auth y gateway | Asegúrate que ambos usen el mismo valor |
| `TENANT_NOT_FOUND` al hacer login | MongoDB sin datos | Corre el seed |
| EF Core migration error | DB sin schema `System` | El backend lo crea; verifica que postgres esté activo |
| e-CF endpoint 422 | Campos `eNCF`/`TipoeCF` mal formateados | Revisar payload contra `build_ecf()` en `ecf_builder.py` |
| Frontend no conecta al gateway | Puerto incorrecto | Verifica `environment.ts` → `gatewayUrl` |
