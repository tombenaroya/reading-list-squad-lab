import { useCallback, useEffect, useState } from 'react'
import './App.css'
import { addBook, listBooks, type AddBookInput, type Book } from './api/books'
import { AddBookForm } from './components/AddBookForm'
import { BookList } from './components/BookList'
import { StatusFilter, type StatusFilterValue } from './components/StatusFilter'

function App() {
  const [books, setBooks] = useState<Book[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [statusFilter, setStatusFilter] = useState<StatusFilterValue>('all')

  const loadBooks = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      setBooks(await listBooks(statusFilter === 'all' ? undefined : statusFilter))
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Unable to load books.')
    } finally {
      setIsLoading(false)
    }
  }, [statusFilter])

  useEffect(() => {
    void loadBooks()
  }, [loadBooks])

  async function handleAddBook(input: AddBookInput): Promise<boolean> {
    setIsSubmitting(true)
    setError(null)

    try {
      await addBook(input)
      await loadBooks()
      return true
    } catch (error) {
      setError(error instanceof Error ? error.message : 'Unable to add book.')
      return false
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="app-shell">
      <header className="page-header">
        <p className="eyebrow">Reading List</p>
        <h1>Reading List</h1>
        <p>Track books you want to read and the ones you have finished.</p>
      </header>

      {error ? <p className="error-message">{error}</p> : null}

      <section className="card" aria-labelledby="add-book-heading">
        <h2 id="add-book-heading">Add a book</h2>
        <AddBookForm onAddBook={handleAddBook} isSubmitting={isSubmitting} />
      </section>

      <section className="card" aria-labelledby="books-heading">
        <h2 id="books-heading">Books</h2>
        <StatusFilter value={statusFilter} onChange={setStatusFilter} />
        <BookList books={books} isLoading={isLoading} />
      </section>
    </main>
  )
}

export default App
