import { useState } from 'react'

interface TicketFormProps {
  onTicketCreated: () => void
  token: string
}

function TicketForm({ onTicketCreated, token }: TicketFormProps) {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [status, setStatus] = useState('Open')
  const [priority, setPriority] = useState('Low')
  const [error, setError] = useState('')
  const [isCreating, setIsCreating] = useState(false)

  const handleCreateTicket = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()

    setError('')

    if (!title.trim()) {
      setError('Title is required.')
      return
    }

    if (title.trim().length < 3 || title.trim().length > 100) {
      setError('Title must be between 3 and 100 characters.')
      return
    }

    if (!description.trim()) {
      setError('Description is required.')
      return
    }

    if (description.trim().length < 5 || description.trim().length > 500) {
      setError('Description must be between 5 and 500 characters.')
      return
    }

    setIsCreating(true)

    try {
      const response = await fetch('http://localhost:5186/api/Tickets', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          title: title.trim(),
          description: description.trim(),
          status,
          priority,
        }),
      })

      if (response.ok) {
        setTitle('')
        setDescription('')
        setStatus('Open')
        setPriority('Low')
        onTicketCreated()
      } else {
        const errorMessage = await response.text()
        setError(errorMessage || 'Failed to create ticket.')
      }
    } catch {
      setError('Unable to connect to the API.')
    } finally {
      setIsCreating(false)
    }
  }

  return (
    <form onSubmit={handleCreateTicket} className="ticket-form">
      <h2>Create Ticket</h2>

      <div className="form-group">
        <label htmlFor="title">Title</label>

        <input
          type="text"
          id="title"
          placeholder="Title (3–100 characters)"
          value={title}
          minLength={3}
          maxLength={100}
          required
          onChange={(e) => {
            setTitle(e.target.value)
            setError('')
          }}
        />
      </div>

      <div className="form-group">
        <label htmlFor="description">Description</label>

        <textarea
          id="description"
          placeholder="Description (5–500 characters)"
          value={description}
          minLength={5}
          maxLength={500}
          required
          onChange={(e) => {
            setDescription(e.target.value)
            setError('')
          }}
        />
      </div>

      <div className="status-priority">
        <div className="form-group">
          <label htmlFor="status">Status</label>

          <select
            id="status"
            value={status}
            onChange={(e) => setStatus(e.target.value)}
          >
            <option value="Open">Open</option>
            <option value="InProgress">In Progress</option>
            <option value="Closed">Closed</option>
          </select>
        </div>

        <div className="form-group">
          <label htmlFor="priority">Priority</label>

          <select
            id="priority"
            value={priority}
            onChange={(e) => setPriority(e.target.value)}
          >
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
          </select>
        </div>
      </div>

      <button
        type="submit"
        className="create-button"
        disabled={isCreating}
      >
        {isCreating ? 'Creating...' : 'Create Ticket'}
      </button>

      {error && <p className="form-error">{error}</p>}
    </form>
  )
}

export default TicketForm
