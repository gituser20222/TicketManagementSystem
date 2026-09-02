import { useState } from 'react'

import type { Ticket } from '../types/Ticket'

interface TicketListProps {
  tickets: Ticket[]
  role: string
  token: string
  onTicketsUpdated: () => void
}

function TicketList({
  tickets,
  role,
  token,
  onTicketsUpdated,
}: TicketListProps) {
  const [editingTicketId, setEditingTicketId] = useState<number | null>(null)
  const [editTitle, setEditTitle] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editStatus, setEditStatus] = useState('')
  const [editPriority, setEditPriority] = useState('')
  const [editError, setEditError] = useState('')

  function startEditing(ticket: Ticket) {
    setEditingTicketId(ticket.id)
    setEditTitle(ticket.title)
    setEditDescription(ticket.description)

    if (ticket.status === 'In Progress') {
      setEditStatus('InProgress')
    } else {
      setEditStatus(ticket.status)
    }

    setEditPriority(ticket.priority)
    setEditError('')
  }

  function cancelEditing() {
    setEditingTicketId(null)
    setEditError('')
  }

  async function saveTicket(ticketId: number) {
    setEditError('')

    if (!editTitle.trim()) {
      setEditError('Title is required.')
      return
    }

    if (editTitle.trim().length < 3 || editTitle.trim().length > 100) {
      setEditError('Title must be between 3 and 100 characters.')
      return
    }

    if (!editDescription.trim()) {
      setEditError('Description is required.')
      return
    }

    if (
      editDescription.trim().length < 5 ||
      editDescription.trim().length > 500
    ) {
      setEditError('Description must be between 5 and 500 characters.')
      return
    }

    try {
      const response = await fetch(
        `http://localhost:5186/api/Tickets/${ticketId}`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            title: editTitle.trim(),
            description: editDescription.trim(),
            status: editStatus,
            priority: editPriority,
          }),
        },
      )

      if (!response.ok) {
        const errorText = await response.text()
        console.error('Update failed:', errorText)
        setEditError('Failed to update ticket.')
        return
      }

      setEditingTicketId(null)
      setEditError('')
      onTicketsUpdated()
    } catch (error) {
      console.error(error)
      setEditError('Unable to connect to the API.')
    }
  }

  return (
    <div className="ticket-table-container">
      <table className="ticket-table">
        <thead>
          <tr>
            <th className="ticket-title-column">Title</th>
            <th className="ticket-description-column">Description</th>
            <th className="ticket-status-column">Status</th>
            <th className="ticket-priority-column">Priority</th>
            {role === 'Admin' && <th>Actions</th>}
          </tr>
        </thead>

        <tbody>
          {tickets.map((ticket) => (
            <tr key={ticket.id}>
              {editingTicketId === ticket.id ? (
                <>
                  <td>
                    <div className="ticket-edit">
                      <input
                        type="text"
                        placeholder="Title (3–100 characters)"
                        value={editTitle}
                        minLength={3}
                        maxLength={100}
                        onChange={(e) => {
                          setEditTitle(e.target.value)
                          setEditError('')
                        }}
                      />
                    </div>
                  </td>

                  <td>
                    <div className="ticket-edit">
                      <textarea
                        placeholder="Description (5–500 characters)"
                        value={editDescription}
                        minLength={5}
                        maxLength={500}
                        onChange={(e) => {
                          setEditDescription(e.target.value)
                          setEditError('')
                        }}
                      />
                    </div>
                  </td>

                  <td>
                    <div className="ticket-edit">
                      <select
                        value={editStatus}
                        onChange={(e) => setEditStatus(e.target.value)}
                      >
                        <option value="Open">Open</option>
                        <option value="InProgress">In Progress</option>
                        <option value="Closed">Closed</option>
                      </select>
                    </div>
                  </td>

                  <td>
                    <div className="ticket-edit">
                      <select
                        value={editPriority}
                        onChange={(e) => setEditPriority(e.target.value)}
                      >
                        <option value="Low">Low</option>
                        <option value="Medium">Medium</option>
                        <option value="High">High</option>
                      </select>
                    </div>
                  </td>

                  {role === 'Admin' && (
                    <td>
                      <div className="ticket-edit">
                        <button onClick={() => saveTicket(ticket.id)}>
                          Save
                        </button>

                        <button onClick={cancelEditing}>
                          Cancel
                        </button>

                        {editError && (
                          <p className="form-error">{editError}</p>
                        )}
                      </div>
                    </td>
                  )}
                </>
              ) : (
                <>
                  <td>{ticket.title}</td>

                  <td>{ticket.description}</td>

                  <td>{ticket.status}</td>

                  <td>{ticket.priority}</td>

                  {role === 'Admin' && (
                    <td>
                      <button onClick={() => startEditing(ticket)}>
                        Edit
                      </button>
                    </td>
                  )}
                </>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default TicketList
