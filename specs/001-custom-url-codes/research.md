# Research: Custom URL Codes

## Decisions

### Optional request field and compatibility

- **Decision**: Add nullable optional `Code` to the existing `CreateShortUrlRequest`; keep the current response record and endpoint routes unchanged. Treat missing, null, empty, or whitespace-only values as omitted and retain the generated-code path.
- **Rationale**: This is an extension of the existing create flow, and the constitution requires new request fields to be optional. The specification defines blank input as equivalent to no custom code.
- **Alternatives considered**: A separate create endpoint would split one operation and violate the user's explicit direction. Making the field required would break existing clients.

### Code policy

- **Decision**: Place the format and reserved-code rules in a small stateless `ShortCodeValidator` class under `Services/`. Accept 3-32 ASCII letters or digits, hyphen, and underscore; preserve casing; reject `urls` with an ordinal case-insensitive comparison.
- **Rationale**: This centralizes the feature's rules without a package or a new application layer. `Code` is already limited to 32 characters in the EF model. The current `/urls` route namespace makes `urls` a reserved one-segment code.
- **Alternatives considered**: Data-annotation validation would distribute policy in the request contract and would require wiring Minimal API validation behavior; an external validation package is explicitly out of scope. Trimming or lowercasing would change the code chosen by the user and conflict with the specification.

### Duplicate checks and concurrent creation

- **Decision**: Check custom-code availability before insert for a direct 409 response. Treat the SQLite unique index as authoritative: catch `DbUpdateException` around the custom-code `SaveChangesAsync`, require the SQLite extended result code `SQLITE_CONSTRAINT_UNIQUE` (2067), and verify that the exact requested code now exists before returning 409 ProblemDetails. Re-throw unrelated persistence errors. Do not rely on the pre-check to prevent races.
- **Rationale**: Two requests can both pass the pre-check. The existing unique index resolves the race, and the persistence exception is the correct point to translate it. Confirming the exact code avoids mapping unrelated unique failures to a duplicate-code response without parsing provider error text. Only custom-code attempts get the user-directed 409 behavior.
- **Alternatives considered**: Pre-check-only is racy. Catching every `DbUpdateException` would misreport unrelated storage failures. Parsing the formatted SQLite message to identify an index is provider-message dependent. Adding a new database constraint or migration is unnecessary because the index already exists.

### HTTP result contracts

- **Decision**: Keep the existing typed return union, `Results<Created<ShortUrlResponse>, ValidationProblem, ProblemHttpResult>`. Return `ValidationProblem` with status 400 for invalid or reserved values and `TypedResults.Problem` with status 409 for duplicate codes. Keep 201 and the current response/Location behavior for success; document 409 response metadata on the existing POST route.
- **Rationale**: The current handler already uses these result shapes, and the constitution requires typed results, 400 validation responses, and ProblemDetails for other errors. No endpoint is added.
- **Alternatives considered**: Returning untyped `IResult` would weaken compile-time and OpenAPI result metadata. Returning 400 for a duplicate would not distinguish a resource conflict.

### Persistence and verification scope

- **Decision**: Do not change `ShortUrl`, `AppDbContext`, SQLite schema, generator, package references, or project count. Extend the existing `.http` file and run `dotnet build`.
- **Rationale**: `Code` already has max length 32 and a unique index; the feature requires no new stored field. The constitution requires HTTP success/failure scenarios and a clean build. The user explicitly rules out packages and a test project.
- **Alternatives considered**: Schema migration and a new test project are unnecessary and explicitly out of scope.

## Technical References

- SQLite result code `SQLITE_CONSTRAINT_UNIQUE` (2067): https://www.sqlite.org/rescode.html#constraint_unique
- Microsoft.Data.Sqlite database errors: https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/database-errors
- ASP.NET Core Minimal API OpenAPI response metadata: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0

The project-level patterns were checked in `ShortUrlEndpoints.cs`, `ShortUrlContracts.cs`, `AppDbContext.cs`, `Program.cs`, and `EncurtaUrl.Api.http`. No runtime test was performed during planning.
