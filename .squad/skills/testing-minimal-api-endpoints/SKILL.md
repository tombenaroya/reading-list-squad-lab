---
name: testing-minimal-api-endpoints
description: Use when integration-testing ASP.NET Core minimal API endpoints in-process.
---

# Testing Minimal API Endpoints

When to use: When a .NET minimal API needs endpoint-level tests without running an external server or real database.

## Purpose
Exercise minimal API routes, JSON binding, validation, and persistence boundaries in-process with realistic HTTP requests.

## Process
1. Expose the generated minimal API entry point with `public partial class Program { }`.
2. Use `WebApplicationFactory<Program>` to run the API in-process.
3. Replace persistence with EF Core InMemory using a unique database name per test class/factory.
4. Assert status codes and deserialize response bodies so contract drift is visible.

```csharp
public partial class Program { }

await using var app = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder => builder.ConfigureServices(services =>
    {
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase($"ReadingListTests-{Guid.NewGuid()}"));
    }));

var response = await app.CreateClient().GetAsync("/api/books");
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

## Anti-patterns
- Do not only assert status codes when response payload shape is part of the contract.
- Do not reuse one shared InMemory database across unrelated test classes.

## Output
Fast integration tests that verify endpoint behavior and payload contracts without external services.

confidence: medium
