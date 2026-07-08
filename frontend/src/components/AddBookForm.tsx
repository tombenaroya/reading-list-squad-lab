import { useState } from 'react'
import type { AddBookInput, BookStatus } from '../api/books'

type AddBookFormProps = {
  onAddBook: (input: AddBookInput) => Promise<boolean>
  isSubmitting: boolean
}

const initialFormState: AddBookInput = {
  title: '',
  author: '',
  status: 'unread',
  notes: '',
}

export function AddBookForm({ onAddBook, isSubmitting }: AddBookFormProps) {
  const [form, setForm] = useState(initialFormState)

  const isValid = form.title.trim().length > 0 && form.author.trim().length > 0

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!isValid) {
      return
    }

    const trimmedNotes = form.notes?.trim() ?? ''

    const wasAdded = await onAddBook({
      title: form.title.trim(),
      author: form.author.trim(),
      status: form.status,
      ...(trimmedNotes ? { notes: trimmedNotes } : {}),
    })

    if (wasAdded) {
      setForm(initialFormState)
    }
  }

  return (
    <form className="book-form" onSubmit={handleSubmit}>
      <label>
        Title
        <input
          type="text"
          value={form.title}
          onChange={(event) =>
            setForm((current) => ({ ...current, title: event.target.value }))
          }
          placeholder="e.g. Dune"
          required
        />
      </label>

      <label>
        Author
        <input
          type="text"
          value={form.author}
          onChange={(event) =>
            setForm((current) => ({ ...current, author: event.target.value }))
          }
          placeholder="e.g. Frank Herbert"
          required
        />
      </label>

      <label>
        Status
        <select
          value={form.status}
          onChange={(event) =>
            setForm((current) => ({
              ...current,
              status: event.target.value as BookStatus,
            }))
          }
        >
          <option value="unread">unread</option>
          <option value="read">read</option>
        </select>
      </label>

      <label>
        Notes
        <textarea
          value={form.notes ?? ''}
          onChange={(event) =>
            setForm((current) => ({ ...current, notes: event.target.value }))
          }
          placeholder="Optional personal notes"
          rows={3}
          maxLength={1000}
        />
      </label>

      <button type="submit" disabled={!isValid || isSubmitting}>
        {isSubmitting ? 'Adding…' : 'Add book'}
      </button>
    </form>
  )
}
