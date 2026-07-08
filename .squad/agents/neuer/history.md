# Project Context

- **Owner:** Tom Ben Aroya
- **Project:** "Reading List" — a small full-stack app to track books you want to read / are reading / have finished.
- **Stack:** .NET 10 minimal API, React + TypeScript, SQLite storage
- **Created:** 2026-07-08

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- Tester covers both the .NET API and the React UI. Focus on edge cases and unhappy paths.
- First vertical slice (2026-07-08T11:39:12+03:00): Added backend xUnit/WebApplicationFactory contract tests covering empty GET, valid POST, validation, status round-trip, and client-id ignoring; 9/9 passed.
- Edge-case testing (2026-07-08T11:53:17+03:00): Added 8 backend API edge-case tests and documented current behaviors. Numeric enum status initially returned `201`; Kroos fixed it and the backend suite is now 17/17 green. Open follow-ups from review: Location header/GET-by-id and form-reset-on-failure.
- Review remediation (2026-07-08T11:53:17+03:00): Added four backend contract tests for create Location, follow-Location GET, unknown Guid 404, and malformed ID 404; suite is now 21/21 green.
- Status filter test coverage (2026-07-08T12:10:50+03:00): Added backend contract coverage for unread/read/all/no-param/case-insensitive/invalid status and title/author ValidationProblem keys; backend suite reached 31/31 green.
