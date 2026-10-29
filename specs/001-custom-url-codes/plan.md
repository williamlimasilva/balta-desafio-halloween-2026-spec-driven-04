# Implementation Plan: Custom URL Codes

**Branch**: `main` | **Date**: 2026-10-29 | **Spec**: [spec.md](spec.md)

**Input**: Extend URL creation with an optional custom code, validation and reserved-code handling, while preserving the existing generated-code flow and endpoints.

## Summary

Add an optional `Code` field to the existing create request. A small validator in `Services/` will enforce the specification's length, character, and reserved-code rules. Keep the existing entity, SQLite schema, and unique index. Pre-check custom-code availability for a clear response, then catch a SQLite unique-index race during `SaveChangesAsync` and return 409 ProblemDetails only when the submitted code is now present. Continue using the current generator when `Code` is absent or blank. No new endpoint, NuGet package for custom-code behavior, or test project is planned.

## Technical Context

**Language/Version**: C# / .NET 10, nullable reference types enabled

**Primary Dependencies**: Existing ASP.NET Core Minimal APIs, `TypedResults`, EF Core SQLite, and built-in ProblemDetails; `NSwag.AspNetCore` 14.7.1 is used only to host the requested Swagger UI over the existing OpenAPI document

**Storage**: Existing SQLite `ShortUrls` table; `Code` is already required, limited to 32 characters, and protected by a unique index. The local database file is `data/encurtaurl.db` at repository root; the configured relative path is resolved from the API content root.

**Testing**: Extend `src/EncurtaUrl.Api/EncurtaUrl.Api.http` with success and failure scenarios; run `dotnet build`; no test project per user direction and current project convention

**Target Platform**: .NET 10 web API; development HTTP profile at `http://localhost:5080`

**Project Type**: Single-project web service / Minimal API

**Performance Goals**: No new latency or throughput target is specified; retain existing behavior and limit added database checks to custom-code creation and conflict paths

**Constraints**: Optional `Code`; blank is treated as omitted; custom codes are 3-32 ASCII letters/digits, hyphen, or underscore; preserve case; reserve `urls` case-insensitively; format/reserved failures return 400 ValidationProblem; duplicates return 409 ProblemDetails; no schema changes or migrations; no package for custom-code behavior; Swagger UI package is separately justified below; no new API endpoint or test project

**Scale/Scope**: One request-contract extension, one small validation class, create-handler behavior, and manual HTTP scenarios; listing, details, redirect, response shape, and click tracking remain unchanged

## Constitution Check

_GATE: Checked before research and re-evaluated after design._

| Principle      | Gate                                                                                                            | Plan decision                                                                                                     | Result |
| -------------- | --------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- | ------ |
| Simplicity     | Remain a single .NET 10 Minimal API; handlers use `AppDbContext` directly; no repositories or application layer | Keep persistence and creation orchestration in the existing endpoint handler; add only a focused validator        | PASS   |
| Organization   | Contracts in `Contracts/`, endpoints in `Endpoints/`, services in `Services/`                                   | Extend `CreateShortUrlRequest`; keep `ShortUrlEndpoints`; add `ShortCodeValidator`                                | PASS   |
| HTTP Responses | Use `TypedResults`; validation is 400 ValidationProblem; other errors use ProblemDetails                        | Keep the existing typed result union; return 400 for invalid/reserved codes and 409 ProblemDetails for duplicates | PASS   |
| Data           | EF Core SQLite; `EnsureCreated`; no migrations; justify schema changes                                          | Existing 32-character column and unique index meet the need; no model or schema change                            | PASS   |
| Compatibility  | New request fields are optional                                                                                 | `Code` defaults to null; existing requests and responses remain valid and unchanged                               | PASS   |
| Dependencies   | Justify any NuGet addition                                                                                      | Add `NSwag.AspNetCore` only for the requested Swagger UI; retain built-in OpenAPI generation                      | PASS   |
| Validation     | Add success/failure `.http` examples and build without new warnings                                             | Extend the existing HTTP file and run `dotnet build`; no test project                                             | PASS   |
| Language       | User-facing errors in Portuguese                                                                                | Validation and conflict messages are Portuguese                                                                   | PASS   |

No constitution exception is required.

## Project Structure

### Documentation (this feature)

```text
specs/001-custom-url-codes/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── create-short-url.md
└── tasks.md                 # Created by /speckit-tasks
```

### Source Code (repository root)

```text
src/EncurtaUrl.Api/
├── Contracts/
│   └── ShortUrlContracts.cs       # Add optional Code to create request
├── Data/
│   └── AppDbContext.cs            # Existing unique index and max length; unchanged
├── Endpoints/
│   └── ShortUrlEndpoints.cs       # Extend existing create handler
├── Models/
│   └── ShortUrl.cs                # Existing persisted entity; unchanged
├── Services/
│   ├── ShortCodeGenerator.cs      # Existing generator; unchanged
│   └── ShortCodeValidator.cs      # New format and reserved-code rules
└── EncurtaUrl.Api.http            # Add custom-code scenarios
```

**Structure Decision**: Keep the existing single API project and folder conventions. The validator is a small stateless policy class, not a new persistence or application layer. The endpoint continues using `AppDbContext` directly, as required by the constitution.

## Addendum: Swagger UI

After the original plan, the user requested an interactive API documentation page. `NSwag.AspNetCore` 14.7.1 hosts the Swagger UI at `/swagger` in Development and points it at the existing `/openapi/v1.json` document. The built-in ASP.NET Core OpenAPI generator remains the sole document generator; the API contract and database schema are unchanged by this addition.

## Runtime Database Location

The local SQLite database and sidecars are stored in the repository-root `data/` directory, outside `src/`. `ConnectionStrings:Default` keeps the relative path `../../data/encurtaurl.db`; startup resolves relative SQLite paths against `ContentRootPath` and creates the parent directory. Absolute connection-string overrides remain available for other environments.
