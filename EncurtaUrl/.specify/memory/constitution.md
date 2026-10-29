# EncurtaUrl Constitution

## Core Principles

### 1. Simplicity

The project MUST remain a single .NET 10 Minimal API. Endpoint handlers MUST use
`AppDbContext` directly. The project MUST NOT add application layers, repositories,
or MediatR. Services MUST remain small and use an interface when registered as a
dependency. This keeps the architecture aligned with the existing application.

### 2. Organization

Endpoints MUST live in `Endpoints/` as extension methods over
`IEndpointRouteBuilder`, with related routes grouped using `MapGroup`. Request and
response records MUST live in `Contracts/`, entities in `Models/`, and service
implementations and their interfaces in `Services/`.

### 3. HTTP Responses

Endpoint handlers MUST use `TypedResults` and concrete typed return types. They
MUST use `Results<...>` when a handler can return multiple result types. Validation
failures MUST return `ValidationProblem` with status 400. Other errors MUST produce
`ProblemDetails` with the appropriate HTTP status.

### 4. Data

Persistence MUST use Entity Framework Core with SQLite. The application MUST create
the database with `EnsureCreated`; migrations MUST NOT be introduced. Any schema
change MUST be justified in the feature plan, including its effect on an existing
database.

### 5. Compatibility

Existing API contracts MUST remain compatible. New request fields MUST be optional
unless a breaking contract change is explicitly approved and documented in the
feature plan.

### 6. Dependencies

No NuGet package may be added without an explicit justification in the feature
plan. The plan MUST explain why existing platform or project capabilities are
insufficient.

### 7. Validation

The project currently has no test project. Every feature MUST add success and
failure scenarios to `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, and `dotnet build`
MUST complete without new warnings.

### 8. Language

User-facing error messages MUST be written in Portuguese. Code identifiers and
technical comments MAY follow the existing project conventions.

## Technical Constraints

The API targets `net10.0`, enables nullable reference types, and uses implicit
usings. OpenAPI is exposed only in the Development environment. Configuration
values, including the SQLite connection string, MUST remain in application
configuration rather than being hard-coded in endpoint logic.

## Feature Delivery

Feature plans MUST identify contract or schema impacts and justify any new
dependencies. Before a feature is considered complete, its HTTP scenarios MUST
cover at least one successful outcome and one error outcome, and the API project
MUST build without introducing warnings. Reviewers MUST check the change against
these principles and the existing endpoint and data-access patterns.

## Governance

This constitution governs new features and changes to the API. Amendments MUST be
made in this file, state their rationale, and update the version and amendment
date. A feature that cannot comply MUST document the exception and its rationale
in its plan for explicit review before implementation.

The version follows Semantic Versioning: MAJOR for backward-incompatible changes
to principles, MINOR for added principles or materially expanded requirements,
and PATCH for clarifications that do not change requirements. Every feature plan
and code review MUST assess compliance with this constitution and identify any
approved exceptions.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm with project maintainers | **Last Amended**: 2026-10-29
