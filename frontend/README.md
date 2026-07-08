# Reading List Frontend

React + TypeScript frontend for the Reading List app.

## Run locally

```bash
npm install
npm run dev
```

The Vite dev server runs on <http://localhost:5173>. The backend API must be running on <http://localhost:5180> unless you set `VITE_API_BASE` to a different base URL.

The book list includes an All / Unread / Read status filter. Filtered views call the backend with `?status=unread` or `?status=read`; All omits the status query parameter.

## Build

```bash
npm run build
```
