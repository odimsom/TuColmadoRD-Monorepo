# Security Policy

## Supported versions

| Version | Supported |
|---|---|
| `main` (latest) | ✅ |
| Older tags | ❌ |

Only the latest commit on `main` receives security fixes.

## Reporting a vulnerability

**Do not open a public issue for security vulnerabilities.**

Report vulnerabilities privately to **borrome941@gmail.com** with:

- A description of the vulnerability
- Steps to reproduce
- Potential impact
- (Optional) suggested fix

You will receive an acknowledgment within 48 hours. We aim to release a fix within 7 days for critical issues.

## Scope

In scope:
- API Gateway — authentication bypass, JWT forgery, tenant isolation bypass
- Auth service — credential exposure, token leakage
- Any endpoint that allows cross-tenant data access
- SQL injection or command injection in any service
- Exposed secrets or credentials in build artifacts / Docker images

Out of scope:
- Issues requiring physical access to the server
- Social engineering
- Denial of service (the load-testing perf-lab is intentional)
- Bugs in third-party dependencies (report those upstream)

## Security design notes

- All API requests require a valid JWT issued by the auth service
- `tenant_id` is extracted from the JWT — clients cannot supply their own
- All PostgreSQL queries are parameterized (SQLx compile-time checked in Rust, EF Core in .NET)
- No service credential is stored in the repository — secrets live in `.env` on the VPS only
- Internal services are not exposed through Traefik; only the gateway and frontends have public routes
