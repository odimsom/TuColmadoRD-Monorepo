# TuColmadoRD - Cloud-Local Hybrid System

![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)
![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-purple.svg)
![Pattern](https://img.shields.io/badge/Pattern-Railway_Oriented_Programming-orange.svg)
![ORM](https://img.shields.io/badge/ORM-EF_Core-green.svg)

**TuColmadoRD** is the core backend and business logic nexus for a modern point-of-sale system tailored for retail grocery stores ("Colmados"). It employs a hybrid-cloud architecture ensuring operations even during internet outages, backed by a cryptographic anti-tamper security layer.

---

## 🏗 System Architecture

The project strictly follows **Domain-Driven Design (DDD)** and **Clean Architecture**, enforcing inversion of dependencies across highly segregated logic layers. 

### Modules

#### Core Layers
1. **`TuColmadoRD.Core.Domain`**: Defines all Aggregates, ValueObjects (`TenantIdentifier`, `Money`), Enums, and custom `DomainError` variations. Agnostic of frameworks. Includes domain events for outbox-pattern event sourcing.
2. **`TuColmadoRD.Core.Application`**: Handlers, Use Cases, Pipelines, DTOs. Orchestrated using **MediatR** and standardizing fallible responses using the **Railway-Oriented Programming (ROP)** `Result<T>` pattern.

#### Infrastructure Layers
3. **`TuColmadoRD.Infrastructure.Persistence`**: **Entity Framework Core** schemas, Migrations, Repositories, and the `DbContext`. Handled for **SQL Server** in Development and **PostgreSQL** in Production. Implements outbox pattern for reliable domain event publishing.
4. **`TuColmadoRD.Infrastructure.CrossCutting`**: Common non-business utilities like Dependency Injection configs, network monitors, background hosted services, local DB tenant resolving (`LocalDeviceTenantProvider`), and the offline `LicenseVerifierService`.

#### Presentation Layers
5. **`TuColmadoRD.Presentation.API`**: RESTful API endpoints, Swagger exposition. Tenant-aware request routing and minimal APIs (.NET 10+) with endpoint groups.
6. **`TuColmadoRD.ApiGateway`**: The central **YARP (Yet Another Reverse Proxy)** gateway proxying both `.NET API` functionalities and the standalone `Node.js Auth API`. Hosts unified multi-system Swagger UI configuration natively.

### 📊 Business Domains

- **Inventory Management**: Product catalogs, stock levels, SKU management with cloud-local sync.
- **Sales & WorkShift**: Terminal-based shift management with cash reconciliation, multi-shift support per terminal, and real-time cash balance tracking. Built with DDD patterns including shift aggregates, domain events, and CQRS read/write repositories.
- **Tenant Management**: Multi-tenant SaaS architecture with isolated data per retail location.

### 🛡 Offline Subscription & Anti-Tamper System

A cornerstone module designed specifically to prevent subscription manipulation on standalone devices:
- **`ITimeGuard` & `SystemConfig`**: Employs an internal monotonic database clock ("Last Known Time") enforcing progression and detecting OS manual date tampering. Blockading all execution if reverse clock drift is detected via `ClockAdvancePipelineBehavior`.
- **`LicenseVerifierService`**: Offloads license evaluations offline by using an externally issued `RS256` token validated strictly against the device's public key injected at initial pairing (`device_identity.dat`).
- **`SubscriptionGuardMiddleware`**: Blocks REST endpoint access if the active offline JWT sub-license fails validity execution.

## 🧪 Testing

The solution employs comprehensive unit and integration testing across all layers:
- **Domain Tests**: Validate aggregates, value objects, and business rule enforcement using **xUnit** and **FluentAssertions**.
- **Infrastructure Tests**: Integration tests for EF Core persistence, repository implementations, and database interactions using in-memory SQLite.
- **API Tests**: Endpoint contract validation and request/response mapping.

Run the complete test suite:
```bash
dotnet test TuColmadoRD.slnx
```

---

## 🚀 Getting Started

### 1. Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- SQL Server (Development) or Npgsql (Postgres)
- Active `device_identity.dat` linked through the gateway initialization flow.

### 2. Startup
Navigate to the root directory and build:
```bash
dotnet restore TuColmadoRD.slnx
dotnet build TuColmadoRD.slnx
```

Execute Database EF Migrations:
```bash
$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet ef database update --project src\infrastructure\TuColmadoRD.Infrastructure.Persistence --startup-project src\Presentations\TuColmadoRD.Presentation.API
```

Start systems simultaneously:
```bash
dotnet run --project src/Presentations/TuColmadoRD.ApiGateway
dotnet run --project src/Presentations/TuColmadoRD.Presentation.API
```

### 3. Unified Swagger & UI
Head to `http://localhost:<YARP_PORT>/` to browse the master interactive directory routing your connection properly to `/swagger/index.html` picking endpoints from both microservices.

## � Development Workflow

This project uses feature branch development with comprehensive testing before integration:

```bash
# Create a new feature branch
git switch -c feat/feature-name

# Make changes, commit atomically
git add <changed-files>
git commit -m "feat(domain): add description of changes"

# Run tests locally to validate
dotnet test TuColmadoRD.slnx

# Push to remote and create PR
git push -u origin feat/feature-name
```

Features are merged to the main development branch after code review and all tests pass.

## �🐳 Dockerization & CI/CD
Complete `Dockerfile` targets are mapped across `Dockerfile.api` and `Dockerfile.gateway`. 
Continuous deployment is governed by the comprehensive `.github/workflows/devops.yml` connecting securely to the deployment server orchestrating `docker-compose.yml` instances upon merges to `master`.
## 📋 Project Status

| Component | Status | Notes |
|-----------|--------|-------|
| Domain Layer | ✅ Complete | DDD patterns, value objects, domain events |
| Application Layer | ✅ Complete | CQRS, MediatR pipelines, validators |
| Infrastructure | ✅ Complete | EF Core, repositories, migrations |
| API Endpoints | ✅ Complete | Minimal APIs, tenant-aware routing |
| Security & Auth | ✅ Complete | Offline license verification, clock guards |
| Inventory Module | ✅ Complete | Product management, cloud-local sync |
| Sales/WorkShift Module | ✅ Complete | Shift management, cash reconciliation |
| Testing | ✅ Complete | Domain, infrastructure, and API tests |

## 🤝 Contributing

When implementing new features:
1. Follow DDD principles—domain logic should remain framework-agnostic.
2. Use ROP patterns for fallible operations.
3. Write tests for domain aggregates and critical business logic.
4. Ensure multi-tenant context is preserved across all layers.
5. Run the full test suite before pushing features.

---

**Built with ❤️ for Colmados in the Dominican Republic.**