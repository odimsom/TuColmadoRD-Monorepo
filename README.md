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
