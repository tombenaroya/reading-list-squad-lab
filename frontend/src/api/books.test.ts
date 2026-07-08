import { beforeEach, describe, expect, it, vi } from 'vitest'
import { addBook, listBooks, type Book } from './books'

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
})

describe('books API client', () => {
  it('lists books from the backend API', async () => {
    const books: Book[] = [
      { id: '1', title: 'Dune', author: 'Frank Herbert', status: 'unread' },
    ]
    fetchMock.mockResolvedValueOnce({
      ok: true,
      json: async () => books,
    })

    await expect(listBooks()).resolves.toEqual(books)
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5180/api/books')
  })

  it('appends the status query parameter when listing filtered books', async () => {
    const books: Book[] = [
      { id: '1', title: 'Dune', author: 'Frank Herbert', status: 'unread' },
    ]
    fetchMock.mockResolvedValueOnce({
      ok: true,
      json: async () => books,
    })

    await expect(listBooks('unread')).resolves.toEqual(books)
    expect(fetchMock).toHaveBeenCalledWith(
      'http://localhost:5180/api/books?status=unread',
    )
  })

  it('adds a book with JSON and returns the created book', async () => {
    const created: Book = {
      id: '2',
      title: 'Kindred',
      author: 'Octavia E. Butler',
      status: 'read',
    }
    fetchMock.mockResolvedValueOnce({
      ok: true,
      json: async () => created,
    })

    await expect(
      addBook({ title: 'Kindred', author: 'Octavia E. Butler', status: 'read' }),
    ).resolves.toEqual(created)
    expect(fetchMock).toHaveBeenCalledWith('http://localhost:5180/api/books', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: 'Kindred',
        author: 'Octavia E. Butler',
        status: 'read',
      }),
    })
  })
})
