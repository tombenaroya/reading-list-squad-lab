using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Api.Data;

namespace ReadingList.Api.Tests;

public sealed class BooksApiTests
{
    [Fact]
    public async Task GetBooks_EmptyStore_ReturnsOkWithEmptyArray()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task PostBooks_ValidBody_ReturnsCreatedBookWithLocation()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.Equal(request.Title, book.Title);
        Assert.Equal(request.Author, book.Author);
        Assert.Equal(request.Status, book.Status);
    }

    [Fact]
    public async Task PostBooks_ValidBody_ReturnsLocationForCreatedBookId()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        var location = Assert.IsType<Uri>(response.Headers.Location);
        Assert.Equal($"/api/books/{book.Id}", location.OriginalString);
        var locationId = Guid.Parse(location.OriginalString.Split('/').Last());
        Assert.Equal(book.Id, locationId);
    }

    [Fact]
    public async Task GetBookById_FromPostLocation_ReturnsCreatedBook()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread");
        var postResponse = await client.PostAsJsonAsync("/api/books", request);
        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(createdBook);
        var location = Assert.IsType<Uri>(postResponse.Headers.Location);

        var response = await client.GetAsync(location.OriginalString);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal(createdBook.Id, book.Id);
        Assert.Equal(createdBook.Title, book.Title);
        Assert.Equal(createdBook.Author, book.Author);
        Assert.Equal(createdBook.Status, book.Status);
    }

    [Fact]
    public async Task GetBookById_NonexistentId_ReturnsNotFound()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/books/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBookById_MalformedId_ReturnsNotFound()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/books/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBooks_AfterSuccessfulPost_ReturnsCreatedBook()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("The Left Hand of Darkness", "Ursula K. Le Guin", "unread");
        var postResponse = await client.PostAsJsonAsync("/api/books", request);
        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(createdBook);

        var response = await client.GetAsync("/api/books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        var book = Assert.Single(books);
        Assert.Equal(createdBook.Id, book.Id);
        Assert.Equal(request.Title, book.Title);
        Assert.Equal(request.Author, book.Author);
        Assert.Equal(request.Status, book.Status);
    }

    [Fact]
    public async Task GetBooks_StatusUnread_ReturnsOnlyUnreadBooks()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        await SeedMixedStatusBooksAsync(client);

        var response = await client.GetAsync("/api/books?status=unread");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        Assert.Equal(2, books.Length);
        Assert.All(books, book => Assert.Equal("unread", book.Status));
    }

    [Fact]
    public async Task GetBooks_StatusRead_ReturnsOnlyReadBooks()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        await SeedMixedStatusBooksAsync(client);

        var response = await client.GetAsync("/api/books?status=read");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        var book = Assert.Single(books);
        Assert.Equal("read", book.Status);
    }

    [Theory]
    [InlineData("/api/books?status=all")]
    [InlineData("/api/books")]
    public async Task GetBooks_AllStatusOrNoStatus_ReturnsAllBooks(string url)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        await SeedMixedStatusBooksAsync(client);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        Assert.Equal(3, books.Length);
    }

    [Theory]
    [InlineData("Unread", "unread", 2)]
    [InlineData("READ", "read", 1)]
    public async Task GetBooks_StatusFilter_IsCaseInsensitive(string status, string expectedStatus, int expectedCount)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        await SeedMixedStatusBooksAsync(client);

        var response = await client.GetAsync($"/api/books?status={status}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var books = await response.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        Assert.Equal(expectedCount, books.Length);
        Assert.All(books, book => Assert.Equal(expectedStatus, book.Status));
    }

    [Theory]
    [InlineData("banana")]
    [InlineData("reading")]
    public async Task GetBooks_InvalidStatus_ReturnsValidationProblemWithStatusError(string status)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/books?status={status}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.NotNull(problem.Errors);
        Assert.Contains("status", problem.Errors.Keys);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PostBooks_EmptyOrWhitespaceTitle_ReturnsValidationProblemWithTitleError(string title)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest(title, "Frank Herbert", "unread");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "title");
    }

    [Fact]
    public async Task PostBooks_MissingTitle_ReturnsValidationProblemWithTitleError()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            author = "Frank Herbert",
            status = "unread"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "title");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PostBooks_EmptyOrWhitespaceAuthor_ReturnsValidationProblemWithAuthorError(string author)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", author, "unread");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "author");
    }

    [Fact]
    public async Task PostBooks_MissingAuthor_ReturnsValidationProblemWithAuthorError()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            title = "Dune",
            status = "unread"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "author");
    }

    [Fact]
    public async Task PostBooks_MissingTitleAndAuthor_ReturnsValidationProblemWithBothErrors()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            status = "unread"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "title", "author");
    }

    [Theory]
    [InlineData("\"reading\"")]
    [InlineData("\"banana\"")]
    [InlineData("123")]
    public async Task PostBooks_InvalidStatus_ReturnsBadRequest(string statusJson)
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        using var content = JsonContent($$"""
            {
                "title": "Dune",
                "author": "Frank Herbert",
                "status": {{statusJson}}
            }
            """);

        var response = await client.PostAsync("/api/books", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostBooks_MissingStatus_CurrentlyDefaultsToUnread()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            title = "Dune",
            author = "Frank Herbert"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal("unread", book.Status);
    }

    [Fact]
    public async Task PostBooks_StatusRead_ReturnsCreatedBookWithLowercaseReadStatus()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Parable of the Sower", "Octavia E. Butler", "read");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal("read", book.Status);
    }

    [Fact]
    public async Task PostBooks_StatusPascalCaseRead_CurrentlyAcceptedAsRead()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Parable of the Sower", "Octavia E. Butler", "Read");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal("read", book.Status);
    }

    [Fact]
    public async Task PostBooks_ClientSuppliedId_ReturnsBookWithServerGeneratedId()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var clientId = Guid.NewGuid();
        var request = new
        {
            id = clientId,
            title = "Kindred",
            author = "Octavia E. Butler",
            status = "unread"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.NotEqual(clientId, book.Id);
    }

    [Fact]
    public async Task PostBooks_DuplicateTitleAuthor_CurrentlyAllowsBoth()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread");

        var firstResponse = await client.PostAsJsonAsync("/api/books", request);
        var secondResponse = await client.PostAsJsonAsync("/api/books", request);
        var getResponse = await client.GetAsync("/api/books");

        // Intentional-until-a-policy-is-decided: duplicate title+author is currently allowed.
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var books = await getResponse.Content.ReadFromJsonAsync<BookResponse[]>();
        Assert.NotNull(books);
        Assert.Equal(2, books.Count(book => book.Title == request.Title && book.Author == request.Author));
    }

    [Fact]
    public async Task PostBooks_UnknownJsonFields_CurrentlyIgnoresThem()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new
        {
            title = "Kindred",
            author = "Octavia E. Butler",
            status = "unread",
            shelf = "favorites"
        };

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal(request.title, book.Title);
        Assert.Equal(request.author, book.Author);
        Assert.Equal(request.status, book.Status);
    }

    [Fact]
    public async Task PostBooks_WithNotes_ReturnsCreatedBookWithNotes()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread", "A desert epic worth rereading.");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal(request.Notes, book.Notes);
    }

    [Fact]
    public async Task PostBooks_WithoutNotes_ReturnsCreatedBookWithNullNotes()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread", null);

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Null(book.Notes);
    }

    [Fact]
    public async Task PostBooks_WhitespaceNotes_ReturnsCreatedBookWithNullNotes()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread", "   ");

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Null(book.Notes);
    }

    [Fact]
    public async Task PostBooks_NotesExceedingMaxLength_ReturnsValidationProblemWithNotesError()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread", new string('a', 1001));

        var response = await client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemContainsErrorsAsync(response, "notes");
    }

    [Fact]
    public async Task GetBookById_WithNotes_ReturnsPersistedNotes()
    {
        using var factory = new ReadingListApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateBookRequest("Dune", "Frank Herbert", "unread", "Loved the worldbuilding.");
        var postResponse = await client.PostAsJsonAsync("/api/books", request);
        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(createdBook);

        var response = await client.GetAsync($"/api/books/{createdBook.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal(request.Notes, book.Notes);
    }

    private sealed record CreateBookRequest(string Title, string Author, string Status, string? Notes = null);

    private sealed record BookResponse(Guid Id, string Title, string Author, string Status, string? Notes = null);

    private sealed record ValidationProblemResponse(Dictionary<string, string[]> Errors);

    private static StringContent JsonContent(string json) => new(json, Encoding.UTF8, "application/json");

    private static async Task SeedMixedStatusBooksAsync(HttpClient client)
    {
        var requests = new[]
        {
            new CreateBookRequest("Dune", "Frank Herbert", "unread"),
            new CreateBookRequest("Parable of the Sower", "Octavia E. Butler", "read"),
            new CreateBookRequest("The Left Hand of Darkness", "Ursula K. Le Guin", "unread")
        };

        foreach (var request in requests)
        {
            var response = await client.PostAsJsonAsync("/api/books", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    private static async Task AssertValidationProblemContainsErrorsAsync(HttpResponseMessage response, params string[] errorKeys)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>();
        Assert.NotNull(problem);
        Assert.NotNull(problem.Errors);

        foreach (var errorKey in errorKeys)
        {
            Assert.Contains(errorKey, problem.Errors.Keys);
        }
    }
}

public sealed class ReadingListApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"ReadingListTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || descriptor.ServiceType.Name == "IDbContextOptionsConfiguration`1")
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
