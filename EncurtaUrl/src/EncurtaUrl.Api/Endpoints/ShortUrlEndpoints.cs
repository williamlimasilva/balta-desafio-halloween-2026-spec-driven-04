using EncurtaUrl.Api.Contracts;
using EncurtaUrl.Api.Data;
using EncurtaUrl.Api.Models;
using EncurtaUrl.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace EncurtaUrl.Api.Endpoints;

public static class ShortUrlEndpoints
{
    private const int MaxGenerationAttempts = 5;

    public static IEndpointRouteBuilder MapShortUrlEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/urls").WithTags("URLs");

        group.MapPost("/", CreateAsync).WithName("CreateShortUrl");
        group.MapGet("/", ListAsync).WithName("ListShortUrls");
        group.MapGet("/{code}", GetByCodeAsync).WithName("GetShortUrl");

        // Navegação: GET /{code} redireciona para a URL original
        app.MapGet("/{code}", RedirectAsync)
            .WithName("RedirectToOriginalUrl")
            .WithTags("Redirect");

        return app;
    }

    private static async Task<Results<Created<ShortUrlResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        CreateShortUrlRequest request,
        AppDbContext db,
        IShortCodeGenerator generator,
        HttpRequest http,
        CancellationToken ct)
    {
        if (!IsValidUrl(request.Url))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["url"] = ["Informe uma URL absoluta válida, começando com http:// ou https://."]
            });
        }

        string? code = null;

        for (var attempt = 0; attempt < MaxGenerationAttempts; attempt++)
        {
            var candidate = generator.Generate();
            if (!await db.ShortUrls.AnyAsync(x => x.Code == candidate, ct))
            {
                code = candidate;
                break;
            }
        }

        if (code is null)
        {
            return TypedResults.Problem(
                title: "Não foi possível gerar um código único.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var shortUrl = new ShortUrl
        {
            Code = code,
            OriginalUrl = request.Url
        };

        db.ShortUrls.Add(shortUrl);
        await db.SaveChangesAsync(ct);

        var response = ToResponse(shortUrl, http);
        return TypedResults.Created($"/urls/{shortUrl.Code}", response);
    }

    private static async Task<Ok<List<ShortUrlResponse>>> ListAsync(
        AppDbContext db,
        HttpRequest http,
        CancellationToken ct)
    {
        var items = await db.ShortUrls
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

        return TypedResults.Ok(items.Select(x => ToResponse(x, http)).ToList());
    }

    private static async Task<Results<Ok<ShortUrlResponse>, NotFound>> GetByCodeAsync(
        string code,
        AppDbContext db,
        HttpRequest http,
        CancellationToken ct)
    {
        var shortUrl = await db.ShortUrls
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);

        return shortUrl is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ToResponse(shortUrl, http));
    }

    private static async Task<Results<RedirectHttpResult, NotFound>> RedirectAsync(
        string code,
        AppDbContext db,
        CancellationToken ct)
    {
        var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(x => x.Code == code, ct);

        if (shortUrl is null)
            return TypedResults.NotFound();

        shortUrl.Clicks++;
        await db.SaveChangesAsync(ct);

        return TypedResults.Redirect(shortUrl.OriginalUrl);
    }

    private static bool IsValidUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static ShortUrlResponse ToResponse(ShortUrl x, HttpRequest http)
        => new(
            x.Code,
            $"{http.Scheme}://{http.Host}/{x.Code}",
            x.OriginalUrl,
            x.CreatedAt,
            x.Clicks);
}
