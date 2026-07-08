import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import App from './App'
import * as booksApi from './api/books'

beforeEach(() => {
  vi.spyOn(booksApi, 'listBooks').mockResolvedValue([
    { id: '1', title: 'Dune', author: 'Frank Herbert', status: 'unread' },
  ])
  vi.spyOn(booksApi, 'addBook').mockResolvedValue({
    id: '2',
    title: 'Kindred',
    author: 'Octavia E. Butler',
    status: 'read',
  })
})

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

it('shows the reading list and add-book form', async () => {
  render(<App />)

  expect(
    screen.getByRole('heading', { name: /reading list/i }),
  ).toBeInTheDocument()
  expect(await screen.findByText('Dune')).toBeInTheDocument()
  expect(screen.getByLabelText(/title/i)).toBeInTheDocument()
  expect(screen.getByLabelText(/author/i)).toBeInTheDocument()
  expect(screen.getByLabelText(/status/i)).toBeInTheDocument()
})

it('clears the add-book form after a successful add', async () => {
  render(<App />)

  await screen.findByText('Dune')

  fireEvent.change(screen.getByLabelText(/title/i), {
    target: { value: 'Kindred' },
  })
  fireEvent.change(screen.getByLabelText(/author/i), {
    target: { value: 'Octavia E. Butler' },
  })
  fireEvent.change(screen.getByLabelText(/status/i), {
    target: { value: 'read' },
  })
  fireEvent.change(screen.getByLabelText(/notes/i), {
    target: { value: 'A desert epic.' },
  })
  fireEvent.click(screen.getByRole('button', { name: /add book/i }))

  await waitFor(() => {
    expect(screen.getByLabelText(/title/i)).toHaveValue('')
    expect(screen.getByLabelText(/author/i)).toHaveValue('')
    expect(screen.getByLabelText(/status/i)).toHaveValue('unread')
    expect(screen.getByLabelText(/notes/i)).toHaveValue('')
    expect(screen.getByText('0 / 1000')).toBeInTheDocument()
  })
})

it('updates the notes character counter as the user types', async () => {
  render(<App />)

  await screen.findByText('Dune')

  expect(screen.getByText('0 / 1000')).toBeInTheDocument()

  fireEvent.change(screen.getByLabelText(/notes/i), {
    target: { value: 'A desert epic.' },
  })

  expect(screen.getByText('14 / 1000')).toBeInTheDocument()
})

it('renders empty book notes as a wrapped muted dash', async () => {
  vi.mocked(booksApi.listBooks).mockResolvedValueOnce([
    {
      id: '1',
      title: 'Dune',
      author: 'Frank Herbert',
      status: 'unread',
      notes: null,
    },
  ])

  render(<App />)

  expect(await screen.findByText('Dune')).toBeInTheDocument()
  expect(screen.getByText('—').closest('td')).toHaveClass('notes-cell')
})

it('refetches books for the selected status filter', async () => {
  vi.mocked(booksApi.listBooks)
    .mockResolvedValueOnce([
      { id: '1', title: 'Dune', author: 'Frank Herbert', status: 'unread' },
    ])
    .mockResolvedValueOnce([
      { id: '3', title: 'Kindred', author: 'Octavia E. Butler', status: 'read' },
    ])
    .mockResolvedValueOnce([
      {
        id: '4',
        title: 'Parable of the Sower',
        author: 'Octavia E. Butler',
        status: 'unread',
      },
    ])

  render(<App />)

  expect(await screen.findByText('Dune')).toBeInTheDocument()

  fireEvent.click(screen.getByRole('button', { name: /^Read$/i }))

  expect(await screen.findByText('Kindred')).toBeInTheDocument()
  expect(booksApi.listBooks).toHaveBeenLastCalledWith('read')

  fireEvent.click(screen.getByRole('button', { name: /^Unread$/i }))

  expect(await screen.findByText('Parable of the Sower')).toBeInTheDocument()
  expect(booksApi.listBooks).toHaveBeenLastCalledWith('unread')
})

it('refreshes the active status filter after adding a book', async () => {
  vi.mocked(booksApi.listBooks)
    .mockResolvedValueOnce([
      { id: '1', title: 'Dune', author: 'Frank Herbert', status: 'unread' },
    ])
    .mockResolvedValueOnce([
      { id: '3', title: 'Kindred', author: 'Octavia E. Butler', status: 'read' },
    ])
    .mockResolvedValueOnce([
      { id: '3', title: 'Kindred', author: 'Octavia E. Butler', status: 'read' },
    ])

  render(<App />)

  expect(await screen.findByText('Dune')).toBeInTheDocument()
  fireEvent.click(screen.getByRole('button', { name: /^Read$/i }))
  expect(await screen.findByText('Kindred')).toBeInTheDocument()

  fireEvent.change(screen.getByLabelText(/title/i), {
    target: { value: 'Parable of the Sower' },
  })
  fireEvent.change(screen.getByLabelText(/author/i), {
    target: { value: 'Octavia E. Butler' },
  })
  fireEvent.click(screen.getByRole('button', { name: /add book/i }))

  await waitFor(() => {
    expect(booksApi.listBooks).toHaveBeenLastCalledWith('read')
  })
})

it('keeps the add-book form values and shows an error after a failed add', async () => {
  vi.mocked(booksApi.addBook).mockRejectedValueOnce(new Error('Unable to add Kindred.'))
  render(<App />)

  await screen.findByText('Dune')

  fireEvent.change(screen.getByLabelText(/title/i), {
    target: { value: 'Kindred' },
  })
  fireEvent.change(screen.getByLabelText(/author/i), {
    target: { value: 'Octavia E. Butler' },
  })
  fireEvent.change(screen.getByLabelText(/status/i), {
    target: { value: 'read' },
  })
  fireEvent.change(screen.getByLabelText(/notes/i), {
    target: { value: 'A desert epic.' },
  })
  fireEvent.click(screen.getByRole('button', { name: /add book/i }))

  expect(await screen.findByText('Unable to add Kindred.')).toBeInTheDocument()
  expect(screen.getByLabelText(/title/i)).toHaveValue('Kindred')
  expect(screen.getByLabelText(/author/i)).toHaveValue('Octavia E. Butler')
  expect(screen.getByLabelText(/status/i)).toHaveValue('read')
  expect(screen.getByLabelText(/notes/i)).toHaveValue('A desert epic.')
  expect(screen.getByText('14 / 1000')).toBeInTheDocument()
  expect(screen.getByRole('button', { name: /add book/i })).toBeEnabled()
})
