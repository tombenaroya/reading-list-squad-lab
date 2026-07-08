# Project Context

- **Owner:** Tom Ben Aroya
- **Project:** "Reading List" — a small full-stack app to track books you want to read / are reading / have finished.
- **Stack:** .NET 10 minimal API, React + TypeScript, SQLite storage
- **Created:** 2026-07-08

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- Backend is a .NET 10 minimal API over SQLite. Keep data access simple and contracts stable.
- First vertical slice (2026-07-08T11:39:12+03:00): Implemented the .NET 10 minimal API, EF Core SQLite persistence, Book model/status, POST/GET `/api/books`, and local CORS setup.
- Enum validation fix (2026-07-08T11:53:17+03:00): Fixed numeric enum coercion by configuring `JsonStringEnumConverter(allowIntegerValues: false)`; backend suite is 17/17 green. Open follow-ups from review: Location header/GET-by-id and frontend form-reset-on-failure.
- Review remediation (2026-07-08T11:53:17+03:00): Added `GET /api/books/{id}`, fixed create Location to `/api/books/{id}`, updated backend README and gitignore entries. Neuer expanded backend contract coverage to 21/21 green.
- Status filter + validation slice (2026-07-08T12:10:50+03:00): Added nullable-string status query parsing for `GET /api/books`, `all` alias/no-filter behavior, invalid status ValidationProblem, and stricter title/author POST validation; tester expanded backend suite to 31/31 green.
