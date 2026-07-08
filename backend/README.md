# Reading List Backend

Minimal .NET 10 API for the Reading List app.

## Run

From `backend/`:

```bash
dotnet run --project ReadingList.Api
```

The local development URL is `http://localhost:5180`.

## Endpoints

### `POST /api/books`

Request:

```json
{
  "title": "Dune",
  "author": "Frank Herbert",
  "status": "unread"
}
```

Returns `201 Created` with the created book. `title` and `author` must be non-null and non-whitespace.

Invalid requests return `400 Bad Request` as `application/problem+json` with machine-readable validation errors:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "title": ["Title is required."],
    "author": ["Author is required."]
  }
}
```

### `GET /api/books`

Returns `200 OK` with all books:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "title": "Dune",
    "author": "Frank Herbert",
    "status": "unread"
  }
]
```

Optional status filter:

- `GET /api/books?status=unread` returns only unread books.
- `GET /api/books?status=read` returns only read books.
- `GET /api/books?status=all` is a friendly alias for no filter.
- Status values are case-insensitive.
- Invalid values, such as `banana`, `reading`, or `123`, return `400 Bad Request` as `application/problem+json` with an `errors.status` entry.

### `GET /api/books/{id}`

Returns `200 OK` with the matching book, or `404 Not Found` when no book exists for the supplied GUID.
