# Project Context

- **Owner:** Tom Ben Aroya
- **Project:** "Reading List" — a small full-stack app to track books you want to read / are reading / have finished.
- **Stack:** .NET 10 minimal API, React + TypeScript, SQLite storage
- **Created:** 2026-07-08

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- Frontend is React + TypeScript, consuming Kroos's minimal API. Keep components small and typed.
- First vertical slice (2026-07-08T11:39:12+03:00): Implemented the React + TypeScript frontend with typed books API client, add-book form, book list, and app wiring.
- Review remediation (2026-07-08T11:53:17+03:00): Preserved add-book form input on failed submit by returning `Promise<boolean>` from the add handler and resetting the form only after success; frontend build/lint/tests passed.
- Status filter UI slice (2026-07-08T12:10:50+03:00): Added typed All/Unread/Read controls wired to the backend `?status=` contract; active filter is preserved across refetches and add-book refresh. Frontend suite remained 8/8 green.
