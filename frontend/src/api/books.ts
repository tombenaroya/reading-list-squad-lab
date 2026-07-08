export const API_BASE =
  import.meta.env.VITE_API_BASE ?? 'http://localhost:5180'

export type BookStatus = 'unread' | 'read'

export type Book = {
  id: string
  title: string
  author: string
  status: BookStatus
  notes?: string | null
}

export type AddBookInput = {
  title: string
  author: string
  status: BookStatus
  notes?: string
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string) {
  if (!response.ok) {
    throw new Error(fallbackMessage)
  }

  return (await response.json()) as T
}

export async function listBooks(status?: BookStatus) {
  const url = status
    ? `${API_BASE}/api/books?status=${encodeURIComponent(status)}`
    : `${API_BASE}/api/books`
  const response = await fetch(url)
  return parseJsonResponse<Book[]>(response, 'Unable to load books.')
}

export async function addBook(input: AddBookInput) {
  const response = await fetch(`${API_BASE}/api/books`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })

  return parseJsonResponse<Book>(response, 'Unable to add book.')
}
