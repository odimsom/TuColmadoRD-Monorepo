# TuColmadoRD — Monorepo

Multi-tenant SaaS POS/ERP for the Dominican Republic retail market. This repository contains all services, frontends, infrastructure, and tooling as a single monorepo.

---

## Architecture overview

```
Browser / PWA / Desktop (.exe)
        │
        ▼
   Traefik (TLS termination, routing)
        │
        ▼
   API Gateway  (.NET 10, JWT validation, reverse proxy)
   ┌────────────┬───────────────┬────────────────┬──────────────┐
   ▼            ▼               ▼                ▼              ▼
Core API    Auth Service   Catalog Service  Reports Service  ECF Generator
(.NET 10)   (Node/Express) (Rust/Axum)     (Rust/Axum)      (Python/Flask)
   │            │               │                │
   ▼            ▼               ▼                ▼
PostgreSQL   MongoDB         PostgreSQL       PostgreSQL
                               + Redis          + Redis
                                            (lazy cache)
```

**Supporting services:** Redis (queue + cache), BullMQ (Notification Service / Node.js), MailHog (dev SMTP), Prometheus + Grafana (observability).

---

## Services

| Directory | Language / Runtime | Role |
|---|---|---|
| `backend/src/Presentations/TuColmadoRD.ApiGateway` | .NET 10 | JWT auth middleware, reverse proxy to downstream services |
| `backend/src/Presentations/TuColmadoRD.Presentation.API` | .NET 10 | Core business API — Clean Architecture (Domain → Application → Infrastructure) |
| `auth/` | Node.js 22, Express 5, Mongoose 9 | User identity, bcrypt password hashing, JWT issuance |
| `services/catalog-service/` | Rust, Axum 0.7, SQLx 0.8 | Product/inventory reads with lazy Redis cache and circuit breaker |
| `services/reports-service/` | Rust, Axum 0.7, SQLx 0.8 | Sales reports with lazy Redis cache (TTL=600s) |
| `notification-service/` | Node.js, BullMQ 5, Nodemailer | Async email delivery via Redis-backed queue |
| `generadordexmle-cf/` | Python, Flask | DGII e-CF (electronic fiscal receipt) XML generation and signing |
| `frontend/web-admin/` | Angular 21, TailwindCSS, DaisyUI | Admin panel PWA |
| `frontend/landing-page/` | Static / SSR | Public marketing site |
| `backend/src/Presentations/TuColmadoRD.Desktop/` | .NET 10 | Offline-capable desktop shell (.exe) |

### Backend layer structure (Clean Architecture)

```
backend/src/
  core/
    TuColmadoRD.Core.Domain          # Entities, value objects, domain events
    TuColmadoRD.Core.Application     # CQRS commands/queries (MediatR), interfaces
  infrastructure/
    TuColmadoRD.Infrastructure.Persistence      # EF Core, PostgreSQL, migrations
    TuColmadoRD.Infrastructure.CrossCutting     # Logging, auth helpers
    TuColmadoRD.Infrastructure.IOC              # DI registration
    TuColmadoRD.Infrastructure.Hosts            # Worker/host configuration
  Presentations/
    TuColmadoRD.Presentation.API     # REST API, controllers, Swagger
    TuColmadoRD.ApiGateway           # Gateway proxy + JWT middleware
    TuColmadoRD.Desktop              # Desktop host
```

---

## Running locally

### Prerequisites

- Docker + Docker Compose v2
- A `.env` file at the repo root (copy `.env.example` and fill in values)

### Start everything

```bash
docker compose up --build -d
```

Services and their local ports:

| Service | Port |
|---|---|
| API Gateway | 8080 |
| Core API | 5000 |
| Auth Service | 3000 |
| Catalog Service | 8081 |
| Reports Service | 8082 |
| Notification Service | 4000 |
| ECF Generator | 5001 |
| Web Admin | 4200 (dev) |
| Traefik dashboard | 8090 |
| PostgreSQL | 5432 |
| MongoDB | 27017 |
| Redis | 6379 |
| MailHog UI | 8025 |
| Prometheus | 9090 |
| Grafana | 3001 |

### Database migrations

```bash
# Run EF Core migrations against a running Postgres container
./executions/migrate.sh
```

---

## CI/CD

Four GitHub Actions workflows:

| Workflow | Trigger | What it does |
|---|---|---|
| `ci-dev-to-qa.yml` | Push to `dev` | Lint, test, build all services |
| `ci-qa-to-main.yml` | Push to `qa` | E2E (Playwright), build Docker images |
| `ci-services.yml` | Push touching `services/**` or `workflow_dispatch` | Build + push Rust service images to `ghcr.io/odimsom/{catalog,reports}-service:main` |
| `cd-deploy.yml` | Push to `main` | SSH to VPS, run `executions/deploy-production.sh` |

Branch flow: `feature/* → dev → qa → main → production`

GHCR images are scoped to the `odimsom` org — the `GITHUB_TOKEN` does not have write access to any other namespace.

---

## Infrastructure

```
infrastructure/
  monitoring/
    prometheus/       # prometheus.yml scrape config
    grafana/          # dashboard provisioning
    alertmanager/
    loki/             # log aggregation
    promtail/
  terraform/
    modules/          # reusable Terraform modules
    hostinger/        # VPS (SSH provider)
    aws/
    azure/
  swarm/              # Docker Swarm stack (reference, not currently deployed)
  ansible/
```

Production runs a single VPS (Hostinger) with Docker Compose. Traefik handles TLS via Let's Encrypt ACME (`tlschallenge`). The `letsencrypt/acme.json` file on the host persists certificates across deploys.

Prometheus scrapes: `catalog-service:8080/metrics`, `reports-service:8081/metrics`, `traefik:8080/metrics`. Grafana is served at `devops.tucolmadord.com/grafana`.

---

## Load testing

```
perf-lab/
  scenarios/
    smoke.js          # 1 VU × 10 iterations — sanity check
    lazy-loading.js   # cold vs warm cache latency comparison
    concurrency.js    # ramp 0→50→100 VUs, measures p95/p99 and RPS
  utils/helpers.js
  docker-compose.yml  # k6 + InfluxDB + Grafana stack
```

Run against any environment:

```bash
cd perf-lab
BASE_URL=https://api.tucolmadord.com \
AUTH_TOKEN=<jwt> \
TENANT_ID=<uuid> \
SCENARIO=concurrency.js \
docker compose up --abort-on-container-exit
```

Grafana dashboard (k6 template) available at `http://localhost:3001` after the stack starts.

---

## Multi-tenancy

Every request must include `tenant_id` (query param or header). The gateway enforces this before forwarding to downstream services. Tenant data is row-level isolated in PostgreSQL using the `tenant_id` column; no cross-tenant queries are possible through the public API.

---

## License

See [LICENSE](LICENSE).
