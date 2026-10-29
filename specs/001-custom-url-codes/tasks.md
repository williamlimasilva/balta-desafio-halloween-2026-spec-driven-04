# Tasks: Custom URL Codes

**Input**: Design documents from `specs/001-custom-url-codes/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/create-short-url.md`, `quickstart.md`

**Tests**: No automated test project or test suite is planned. The constitution requires success and failure scenarios in the existing `.http` file, which are included as manual validation tasks.

**Organization**: Tasks are grouped by the three user stories in `spec.md`.

## Phase 1: Setup

**Purpose**: Verify the current application baseline before implementation.

- [x] T001 Confirm the API baseline builds with `dotnet build src/EncurtaUrl.Api/EncurtaUrl.Api.csproj` before source changes.

---

## Phase 2: Foundational

**Purpose**: Confirm existing persistence guarantees so feature work does not introduce schema or migration changes.

- [x] T002 Verify `src/EncurtaUrl.Api/Data/AppDbContext.cs` already configures `Code` as required, `HasMaxLength(32)`, and uniquely indexed; keep the schema and migrations unchanged.

**Checkpoint**: Existing storage supports custom codes; no shared infrastructure changes are required before the stories.

---

## Phase 3: User Story 1 - Create and use a custom code (Priority: P1) MVP

**Goal**: Accept a valid caller-selected code and use it in the short URL while preserving the existing generated-code behavior for omitted or blank values.

**Independent Test**: POST a valid, unused code, confirm the 201 response and Location use that exact code, then follow the short URL and confirm the original destination and one click increment.

- [x] T003 [P] [US1] Extend `CreateShortUrlRequest` with an optional nullable `Code` defaulting to null in `src/EncurtaUrl.Api/Contracts/ShortUrlContracts.cs` so existing request bodies remain valid.
- [x] T004 [P] [US1] Add a stateless `ShortCodeValidator` in `src/EncurtaUrl.Api/Services/ShortCodeValidator.cs`; accept 3-32 characters inclusive, only ASCII letters `A-Z` and `a-z`, digits `0-9`, hyphen, and underscore; preserve case without trimming input, and reject whitespace in every nonblank code; distinguish reserved `urls` case-insensitively from invalid format.
- [x] T005 [US1] Update `CreateAsync` in `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs` to use the existing generator when `Code` is missing, null, empty, or whitespace-only; otherwise validate the submitted code before persistence, return 400 `ValidationProblem` with a Portuguese `code` message that distinguishes invalid format from reserved `urls`, and accept a valid custom code exactly as submitted while retaining the existing 201 response and Location behavior.
- [x] T006 [US1] Add a successful custom-code creation, details, redirect, and click-count sequence to `src/EncurtaUrl.Api/EncurtaUrl.Api.http` using a unique sample code.

**Checkpoint**: A valid custom code creates a short URL and navigates successfully without changing the existing response shape.

---

## Phase 4: User Story 2 - Receive clear feedback for unavailable codes (Priority: P1)

**Goal**: Reject malformed, reserved, and duplicate codes with clear Portuguese errors and no created record, including when duplicate requests race.

**Independent Test**: Submit too-short, too-long, unsupported-character, reserved, and duplicate codes; expect 400 ValidationProblem for format/reserved values, 409 ProblemDetails for duplicates, and no additional link for rejected requests. Send two requests concurrently with the same unused valid code and expect one 201 and one 409.

- [x] T007 [US2] Add a custom-code existence precheck and handle the unique-index race in `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs`: catch `DbUpdateException` around the custom-code save, require inner `SqliteException.SqliteExtendedErrorCode` 2067 and confirm the exact requested code now exists before returning 409 ProblemDetails in Portuguese; rethrow unrelated persistence errors and document 409 with `.ProducesProblem(409)`.
- [x] T008 [US2] Add 400 scenarios for short, overlong, unsupported-character, and case-insensitive reserved `urls` values, plus repeated-code 409 and no-extra-record checks, to `src/EncurtaUrl.Api/EncurtaUrl.Api.http`.

**Checkpoint**: Invalid and reserved values return validation errors, duplicates return conflict, and a concurrent duplicate cannot bypass the unique index.

---

## Phase 5: User Story 3 - Continue using generated codes and existing views (Priority: P2)

**Goal**: Preserve generated-code creation, list and detail response shapes, and redirect/click behavior for both code types.

**Independent Test**: Create one link without `code` and one with a custom code; verify both appear in `GET /urls`, both return the existing shape from `GET /urls/{code}`, and both redirect and increment clicks through `GET /{code}`.

- [x] T009 [US3] Verify the list, detail, redirect, and `ToResponse` paths in `src/EncurtaUrl.Api/Endpoints/ShortUrlEndpoints.cs` remain code-type agnostic; preserve the existing response record in `src/EncurtaUrl.Api/Contracts/ShortUrlContracts.cs` and exercise generated and custom links using `src/EncurtaUrl.Api/EncurtaUrl.Api.http`.

**Checkpoint**: Existing clients can omit `code`, and all stored links continue to use the same list, detail, and redirect operations.

---

## Phase 6: Polish & Cross-Cutting Validation

**Purpose**: Run the documented end-to-end checks and final build after the stories are complete.

- [x] T010 Run the manual scenarios in `specs/001-custom-url-codes/quickstart.md` against `src/EncurtaUrl.Api/EncurtaUrl.Api.http`, including the concurrent PowerShell requests and their 201/409 statuses, and run `dotnet build src/EncurtaUrl.Api/EncurtaUrl.Api.csproj`; confirm expected HTTP outcomes and no new build warnings.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; establishes the baseline build result.
- **Foundational (Phase 2)**: Follows setup and confirms that the existing schema and unique index are sufficient; blocks story implementation until confirmed.
- **User Stories (Phase 3+)**: US1 establishes the optional request, validator, and custom-code creation flow. US2 extends the same create handler with conflict handling and error scenarios. US3 verifies compatibility after those flows exist.
- **Polish (Phase 6)**: Depends on all three user stories and runs the full quickstart and build gate.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2; no dependency on another story.
- **US2 (P1)**: Depends on US1's request and custom-code create path; extends the same endpoint handler.
- **US3 (P2)**: Depends on US1 for custom links; its compatibility checks cover existing generated links and both code types after US1/US2 integration.

### Within Each User Story

- T003 and T004 can run in parallel because they touch separate files and have no dependency on each other.
- T005 depends on T003 and T004. T007 follows T005 because it extends the same handler.
- T006 and T008 modify the same `.http` file and should be applied sequentially to avoid edit conflicts. T009 only executes/verifies its scenarios and does not edit that file.
- T009 follows T006 and T008; T010 follows all implementation and verification work as the final cross-story gate.

## Parallel Opportunities

### User Story 1

```text
Task: T003 Extend the optional create-request contract in src/EncurtaUrl.Api/Contracts/ShortUrlContracts.cs
Task: T004 Add the custom-code policy validator in src/EncurtaUrl.Api/Services/ShortCodeValidator.cs
```

These can be assigned independently, then integrated by T005.

## Implementation Strategy

### MVP First (User Story 1)

1. Complete baseline and persistence checks (T001-T002).
2. Implement request contract, validator, and custom-code creation (T003-T005).
3. Run the custom create/redirect scenario (T006) and validate the US1 independent test.

### Incremental Delivery

1. Deliver US1 with valid custom codes and the unchanged generated-code fallback.
2. Add US2 validation feedback, duplicate precheck, race handling, and failure scenarios.
3. Verify US3 list/detail/redirect compatibility for generated and custom codes.
4. Complete quickstart scenarios and the clean-build gate.

## Notes

- Every task uses the required `- [ ] T### [P?] [US#?]` checklist format and names the exact file or project path to change or validate.
- `[P]` appears only on tasks for different files with no dependencies.
- The 3-32 length bound, allowed ASCII characters, exact preservation, and case-insensitive `urls` reservation are taken verbatim from the design documents.
- No separate automated test tasks, test project, package, migration, schema change, or new endpoint is included.
