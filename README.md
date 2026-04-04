# TuColmadoRD Monorepo

Monorepo raiz de **TuColmadoRD** con backend, auth y frontends unificados en un solo git.

## Estructura

- `backend/`: solución .NET principal, API y gateway.
- `auth/`: servicio Node.js de autenticación y licencias.
- `frontend/landing-page/`: sitio público.
- `frontend/web-admin/`: panel principal y app desktop/web.

## Dominios

- Landing: `landingcolrd.synsetsolutions.com`
- Web principal: `tucolmadord.synsetsolutions.com`
- Gateway/API pública: `api.tucolmadord.synsetsolutions.com`

## Puertos raros del compose

- Landing: `5199`
- Web: `5209`
- Gateway: `5219`
- API core: `5229`
- Auth: `5239`
- PostgreSQL: `54329`
- MongoDB: `27019`

## Arranque

1. Copia `.env.example` a `.env` y ajusta secretos.
2. Ejecuta:

```bash
docker compose up --build -d
```

## Notas

- El frontend web-admin apunta al gateway público en `api.tucolmadord.synsetsolutions.com`.
- El auth service queda detrás del compose y del gateway.

## Flujo de ramas

- `main`: producción estable.
- `qa`: rama de validación funcional y pruebas.
- `develop`: integración continua de desarrollo.
- `feature/<area>-<descripcion>`: nuevas funcionalidades.
- `fix/<area>-<descripcion>`: correcciones puntuales.
- `hotfix/<area>-<descripcion>`: correcciones urgentes sobre producción.

### Flujo recomendado

1. Crear rama desde `develop`:

```bash
git checkout develop
git pull
git checkout -b feature/web-admin-registro-real
```

2. Abrir PR hacia `develop`.
3. Promover `develop` a `qa` mediante PR para validación.
4. Promover `qa` a `main` cuando QA apruebe.

## Release de prueba (Desktop)

Para publicar la primera versión de prueba y habilitar la descarga desde web-admin:

1. Crear y subir un tag con formato `vX.Y.Z-test.N` (ejemplo: `v0.0.1-test.1`).
2. GitHub Actions ejecutará el workflow `Build & Release Test` en `.github/workflows/release-test.yml`.
3. El workflow genera el instalador `.exe` y lo adjunta como prerelease del monorepo.
4. Web-admin lee automáticamente la última prerelease `-test` y muestra el botón de descarga.

Comandos sugeridos:

```bash
git tag v0.0.1-test.1
git push origin v0.0.1-test.1
```
