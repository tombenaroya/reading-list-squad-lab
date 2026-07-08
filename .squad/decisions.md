# Squad Decisions

## Active Decisions

### 2026-07-08 — Persistence via SQLite + EF Core, monorepo layout

- **Author:** Messi (Lead) · merged by Scribe
- **Status:** Accepted

**Context:** Solo-dev Reading List app (books in to-read / reading / finished).
Fixed stack: .NET 10 minimal API, React + TypeScript, SQLite. We need a
data-access approach and repo layout one person can hold in their head.

**Decision:**
1. Persist to a single SQLite file (`reading-list.db`) via **EF Core**
   (`Microsoft.EntityFrameworkCore.Sqlite`) with one `Book` entity and a thin
   `AppDbContext`. No repository/service layers until real complexity appears.
2. **Monorepo** layout: `backend/` (.NET API) and `frontend/` (React) side by
   side, with API tests in `backend/*.Tests/`.

**Alternatives considered:**
- *In-memory only* — simplest but loses data on restart; a reading list that
  forgets books is useless. Rejected.
- *Raw ADO.NET / Dapper* — fewer deps but more boilerplate; EF Core migrations
  are the more maintainable choice at this scale. Rejected.
- *Separate repos for API and UI* — needless coordination overhead for a solo
  dev. Rejected.

**Consequences:** One `Book` entity, one `DbContext`; the first vertical
slice uses `EnsureCreated()` for friction-free local setup, with EF migrations to
be introduced when the schema starts evolving. `reading-list.db` is git-ignored;
frontend talks to API over HTTP/JSON with CORS enabled for local dev.

### 2026-07-08 — Re-evaluation: keep SQLite + EF Core over JSON file storage

- **Author:** Messi (Lead)
- **Status:** Accepted

**Context:** Tom asked whether workshop storage should remain **SQLite + EF
Core** or switch to a simple JSON file. This re-evaluates the accepted
2026-07-08 decision, “Persistence via SQLite + EF Core, monorepo layout.” Since
that decision, the first vertical slice has already shipped on SQLite + EF Core:
`backend/ReadingList.Api/` uses `Microsoft.EntityFrameworkCore.Sqlite` with
`AppDbContext` and `Book`, `POST /api/books` and `GET /api/books` are working,
and `backend/ReadingList.Api.Tests/` has 9 passing tests using the EF Core
InMemory provider.

**Decision:** Confirm **SQLite + EF Core** as the storage choice. It still fits
the workshop goals: local setup remains small, while the code demonstrates
realistic backend patterns through EF Core, LINQ querying, migrations when the
schema evolves, and testable persistence boundaries. Re-doing the slice as JSON
is not justified because it would rip out working, tested code for a less
realistic storage model.

**Alternatives considered:**
- *Simple JSON file* — near-zero setup, no storage dependency, and trivially
  inspectable by workshop participants. Rejected because it pushes manual
  concurrency and file-locking concerns into app code, has no real querying
  model, is unlike production backends, and would require replacing already
  working and tested SQLite + EF Core implementation.

**Consequences:** Keep the current SQLite + EF Core implementation. Continue to
optimize workshop ergonomics with friction-free local defaults rather than
simplifying persistence into a JSON file. Introduce EF migrations only when the
schema starts evolving, as stated in the original accepted decision.

### 2026-07-08 — Book list status filter contract

- **Author:** Kroos (Backend Dev) · merged by Scribe
- **Status:** Accepted

**Context:** The Reading List app needs a stable API/UI contract for filtering books by read status while preserving clear validation semantics.

**Decision:** `GET /api/books` accepts optional `?status=` values `unread` and `read`, treats `all` as no filter, and parses case-insensitively. Missing status returns all books. Blank, unknown, or numeric values return `400` problem+json/ValidationProblem with a `status` error key. Filtering is applied in the EF query.

**Consequences:** Frontend filter controls should omit the query parameter for All and send explicit `unread`/`read` values otherwise. Backend tests should cover no-param, all alias, valid filters, case-insensitive parsing, and invalid values.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
