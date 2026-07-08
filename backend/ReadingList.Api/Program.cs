using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ReadingList.Api.Data;
using ReadingList.Api.Models;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "ViteDevServer";

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var databasePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "reading-list.db"));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors(CorsPolicyName);

app.MapPost("/api/books", async (CreateBookRequest request, AppDbContext db) =>
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Title))
    {
        errors["title"] = ["Title is required."];
    }

    if (string.IsNullOrWhiteSpace(request.Author))
    {
        errors["author"] = ["Author is required."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var book = new Book
    {
        Title = request.Title.Trim(),
        Author = request.Author.Trim(),
        Status = request.Status
    };

    db.Books.Add(book);
    await db.SaveChangesAsync();

    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapGet("/api/books", async (string? status, AppDbContext db) =>
{
    var query = db.Books.AsNoTracking();

    if (status is not null)
    {
        var trimmedStatus = status.Trim();

        if (!string.Equals(trimmedStatus, "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<BookStatus>(trimmedStatus, ignoreCase: true, out var parsedStatus)
                || !Enum.IsDefined(parsedStatus))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["status"] = ["Status must be one of: unread, read, all."]
                });
            }

            query = query.Where(book => book.Status == parsedStatus);
        }
    }

    var books = await query.OrderBy(book => book.Title).ToListAsync();
    return Results.Ok(books);
});

app.MapGet("/api/books/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var book = await db.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

app.Run();

public sealed record CreateBookRequest(string Title, string Author, BookStatus Status);

public partial class Program
{
}
