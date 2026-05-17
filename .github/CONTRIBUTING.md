# Contributing to TuColmadoRD

## Branch flow

```
feature/your-feature
        │
        ▼
       dev  ──►  qa  ──►  main  ──►  production
```

All PRs target `dev`. Never push directly to `qa` or `main` — promotion is automatic via CI.

## Before you start

1. Check open issues and discussions to avoid duplicate work.
2. For significant changes, open an issue or discussion first to align on approach.
3. Fork the repo and create a branch from `dev`:
   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/your-feature-name
   ```

## Local setup

See [[Local Development]] in the wiki or `.docs/STARTUP.md` for full instructions.

```bash
cp .env.example .env   # fill in values
docker compose up --build -d
./executions/migrate.sh
```

## Commit style

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(catalog): add product variant support
fix(gateway): return 404 instead of 500 on unknown tenant
refactor(auth): extract token refresh into service layer
docs: update local development guide
test(reports): add integration test for daily summary
chore(ci): pin docker buildx version
```

**Scopes:** `auth`, `gateway`, `api`, `catalog`, `reports`, `notification`, `ecf`, `web`, `landing`, `ci`, `deploy`, `infra`.

## Pull request checklist

- [ ] Branch is up to date with `dev`
- [ ] All existing tests pass (`docker compose` integration tests)
- [ ] New behavior has test coverage
- [ ] No secrets or credentials in the diff
- [ ] `GHCR_TOKEN` is not committed (it stays in `.env`)
- [ ] PR description explains *why*, not just *what*

## Code style

- **.NET**: follow existing Clean Architecture layer boundaries — no business logic in controllers
- **Rust**: `cargo fmt` and `cargo clippy` must pass
- **TypeScript**: ESLint passes, no `any` casts without a comment explaining why
- **All services**: every public endpoint must include a tenant_id check

## Testing

- Unit tests: run inside each service directory
- Integration tests: run via `docker compose` (full stack must be up)
- Load tests: `perf-lab/` — run the `smoke.js` scenario first to validate reachability

## Questions

Use [GitHub Discussions](https://github.com/odimsom/TuColmadoRD-Monorepo/discussions) — Q&A category for how-to questions, Ideas category for proposals.
