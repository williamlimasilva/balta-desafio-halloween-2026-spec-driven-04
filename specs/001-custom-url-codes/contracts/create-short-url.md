# API Contract: Create Short URL

## Existing operation

`POST /urls` remains the only create operation. The request keeps `url` and adds optional `code`; omission preserves automatic code generation.

### Custom-code request

```http
POST /urls
Content-Type: application/json

{
  "url": "https://balta.io",
  "code": "balta-io"
}
```

`code` may be omitted, null, empty, or whitespace-only to use the current generator. A nonblank value is preserved as written and must match `[A-Za-z0-9_-]{3,32}`. `urls` is reserved case-insensitively.

## Responses

| Condition                                               | Status | Response                                                                                                             |
| ------------------------------------------------------- | -----: | -------------------------------------------------------------------------------------------------------------------- |
| Created with generated or custom code                   |    201 | Existing `ShortUrlResponse`; `Code` and `ShortUrl` contain the selected code; existing Location behavior is retained |
| Invalid format or reserved code                         |    400 | Validation Problem Details with a `code` error in Portuguese                                                         |
| Code already exists, including a concurrent insert race |    409 | ProblemDetails in Portuguese stating that the code is already in use                                                 |
| Invalid original URL                                    |    400 | Existing URL ValidationProblem behavior                                                                              |

A successful response retains the existing shape:

```json
{
  "code": "balta-io",
  "shortUrl": "http://localhost:5080/balta-io",
  "originalUrl": "https://balta.io",
  "createdAt": "<creation timestamp>",
  "clicks": 0
}
```

The `shortUrl` host is determined by the request as it is today; the example uses the local development profile.

## Unchanged operations

- `GET /urls` lists generated and custom short links using the existing response shape.
- `GET /urls/{code}` returns details using the existing response shape.
- `GET /{code}` redirects to the original URL and increments clicks as it does for generated codes.

No new route, response DTO, authentication requirement, expiration behavior, or code-edit operation is introduced.
