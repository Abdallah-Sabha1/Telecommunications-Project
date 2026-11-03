# Aithos backend architecture

The backend is organized as a modular monolith using Clean Architecture.

## Dependency direction

```text
Aithos.Domain          <- no project dependencies
Aithos.Application     -> Aithos.Domain
Aithos.Infrastructure  -> Aithos.Application
Aithos.Api             -> Aithos.Application + Aithos.Infrastructure
```

## Responsibilities

- `Aithos.Domain`: business entities and domain concepts. It does not know about SQL Server, ASP.NET, Claude, or PDF generation.
- `Aithos.Application`: use-case contracts, DTOs, repository/service abstractions, and application workflows grouped by feature.
- `Aithos.Infrastructure`: EF Core persistence, repositories, migrations, Claude integration, file imports, and PDF reporting.
- `Aithos.Api`: HTTP controllers, middleware, configuration, dependency-injection composition, and API transport concerns.

## Feature organization

Application workflows are grouped by capability rather than by technical type. Current capabilities include towers, readings, alerts, analysis, energy, carbon reporting, AI results, and imports.

Each feature follows the same predictable structure:

```text
FeatureName
├── DTOs
│   ├── Requests
│   └── Responses
├── Services
│   ├── Interfaces
│   └── Implementations
└── Repositories
    └── Interfaces
```

Repository implementations live in Infrastructure because they depend on EF Core:

```text
Infrastructure/Persistence/Repositories/FeatureName/Implementations
```

This keeps the interface close to the feature contract while keeping SQL and EF Core outside the Application layer.

## Compatibility rule

This refactor must preserve the existing API routes, request/response contracts, frontend behavior, database schema, and calculation behavior. New features should be introduced only through the appropriate application abstraction and infrastructure adapter.
