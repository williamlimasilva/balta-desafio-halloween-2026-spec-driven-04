using EncurtaUrl.Api.Data;
using EncurtaUrl.Api.Endpoints;
using EncurtaUrl.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
var sqliteConnectionString = new SqliteConnectionStringBuilder(connectionString);

if (!string.Equals(sqliteConnectionString.DataSource, ":memory:", StringComparison.Ordinal))
{
    var databasePath = Path.IsPathRooted(sqliteConnectionString.DataSource)
        ? sqliteConnectionString.DataSource
        : Path.Combine(builder.Environment.ContentRootPath, sqliteConnectionString.DataSource);

    sqliteConnectionString.DataSource = Path.GetFullPath(databasePath);
    var databaseDirectory = Path.GetDirectoryName(sqliteConnectionString.DataSource);
    if (!string.IsNullOrEmpty(databaseDirectory))
        Directory.CreateDirectory(databaseDirectory);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString.ConnectionString));

builder.Services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();

var app = builder.Build();

// Para a aula: cria o banco SQLite na primeira execução, sem migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("EncurtaUrl API"));
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapShortUrlEndpoints();

app.Run();
