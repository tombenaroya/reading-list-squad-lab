import type { Book } from '../api/books'

type BookListProps = {
  books: Book[]
  isLoading: boolean
}

export function BookList({ books, isLoading }: BookListProps) {
  if (isLoading) {
    return <p>Loading…</p>
  }

  if (books.length === 0) {
    return <p>No books yet. Add your first book above.</p>
  }

  return (
    <div className="table-wrap">
      <table>
        <thead>
          <tr>
            <th scope="col">Title</th>
            <th scope="col">Author</th>
            <th scope="col">Status</th>
            <th scope="col">Notes</th>
          </tr>
        </thead>
        <tbody>
          {books.map((book) => (
            <tr key={book.id}>
              <td>{book.title}</td>
              <td>{book.author}</td>
              <td>
                <span className={`status-pill status-${book.status}`}>
                  {book.status}
                </span>
              </td>
              <td className="notes-cell">
                {book.notes?.trim() ? (
                  book.notes
                ) : (
                  <span className="notes-empty">—</span>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
