# Quickstart: Validate Custom URL Codes

## Prerequisites

- .NET 10 SDK
- Repository root available locally
- Existing development HTTP profile at `http://localhost:5080`
- The API uses `src/EncurtaUrl.Api/EncurtaUrl.Api.http` for request scenarios

## Start the API

From the repository root:

```powershell
cd src/EncurtaUrl.Api
dotnet run --launch-profile http
```

The API runs in Development at `http://localhost:5080`. SQLite creates or reuses `data/encurtaurl.db` at the repository root, outside `src/`; the relative path is anchored to the API content root.

## Run the HTTP scenarios

Open `src/EncurtaUrl.Api/EncurtaUrl.Api.http` in VS Code and execute the requests. Select a custom code not already present in the local database before running the first custom-code request.

1. **Generated-code compatibility**: Create a link with only `url`; expect 201 and a generated code. Existing list, details, redirect, and invalid-URL requests should continue to behave as before.
2. **Custom-code creation**: Create a link with a valid `code` such as `balta-io`; expect 201 and the exact code in `code`, `shortUrl`, and the Location header.
3. **Details and redirect**: Request details for the custom code, access the short address, then request details again. Expect the original destination and exactly one additional click.
4. **Invalid format**: Try a value shorter than 3 characters, longer than 32 characters, or containing a space or unsupported punctuation; expect 400 ValidationProblem with a Portuguese `code` error.
5. **Reserved value**: Try `URLs`; expect 400 ValidationProblem explaining that the code is reserved.
6. **Duplicate value**: Submit the same custom code a second time; expect 409 ProblemDetails stating that the code is already in use. No second link should be created.
7. **Concurrent duplicate (race)**: Run the PowerShell snippet below with the API running. It submits both requests in separate jobs with the same unused valid code; expect one 201 and one 409 (the order is arbitrary).

```powershell
$uri = 'http://localhost:5080/urls'
$code = 'race-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$body = @{ url = 'https://balta.io'; code = $code } | ConvertTo-Json -Compress

$jobs = 1..2 | ForEach-Object {
	Start-Job -ArgumentList $uri, $body -ScriptBlock {
		param($requestUri, $requestBody)
		try {
			$response = Invoke-WebRequest -Uri $requestUri -Method Post `
				-ContentType 'application/json' -Body $requestBody
			[int]$response.StatusCode
		}
		catch {
			if ($_.Exception.Response) {
				[int]$_.Exception.Response.StatusCode
			}
			else {
				throw
			}
		}
	}
}

$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job
```

The pair is a concurrent smoke check; depending on request timing, the losing request may be rejected by the precheck or by the SQLite unique-index race handler. Both paths must return 409. Repeat with a fresh code if the two requests do not produce one 201 and one 409.

Use a new custom code when repeating the successful-create and race scenarios against a database that already contains prior runs.

## Build gate

From the repository root, run:

```powershell
dotnet build src/EncurtaUrl.Api/EncurtaUrl.Api.csproj
```

Expected result: build succeeds without new warnings. No test project or package installation is required.

For data shape and validation rules, see [data-model.md](data-model.md) and [contracts/create-short-url.md](contracts/create-short-url.md).
