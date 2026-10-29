# Data Model: Custom URL Codes

## Existing Entity: ShortUrl

No entity or database-schema change is required.

| Field         | Current meaning                                    | Feature impact                                                                                                               |
| ------------- | -------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| `Id`          | Internal identity of the short link                | Unchanged                                                                                                                    |
| `Code`        | Unique code used in the short address and redirect | Stores either the generated code or the submitted custom code; remains required, maximum 32 characters, and uniquely indexed |
| `OriginalUrl` | Destination address                                | Unchanged                                                                                                                    |
| `CreatedAt`   | Creation timestamp                                 | Unchanged                                                                                                                    |
| `Clicks`      | Successful redirect count                          | Unchanged; custom-code redirects use the existing behavior                                                                   |

## Creation Request

`CreateShortUrlRequest` gains an optional nullable `Code` value. This is request data only; it is not stored separately from `ShortUrl.Code`.

### Code validation and state

- Missing, null, empty, or whitespace-only input selects the existing generated-code flow.
- A nonblank custom code must contain 3-32 characters inclusive.
- Allowed characters are ASCII letters `A-Z` and `a-z`, digits `0-9`, hyphen, and underscore.
- Nonblank input is not trimmed or case-normalized. The selected casing is preserved.
- The value `urls` is reserved without regard to casing.
- A custom code already used by another short link is rejected; the SQLite unique index remains the final persistence guard if requests race.

## Relationships and lifecycle

Each short link has one unique code and one original URL. The code is selected at creation, either by the existing generator or by the caller. Editing or removing a code after creation is out of scope. There are no new entity states or relationships.

## Persistence impact

None. `AppDbContext` already configures `Code` as required, `HasMaxLength(32)`, and uniquely indexed. Do not add migrations or call for database recreation.
