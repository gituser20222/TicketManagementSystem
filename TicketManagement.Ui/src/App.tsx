import { useEffect, useState } from 'react'
import { jwtDecode } from 'jwt-decode'
import './App.css'
import Login from './components/Login'
import type { Ticket } from './types/Ticket'
import TicketList from './components/TicketList'
import TicketForm from './components/TicketForm'

interface JwtPayload {
  [key: string]: unknown
}

function App() {
  const [token, setToken] = useState('')
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const getUsername = () => {
    if (!token) {
      return ''
    }

    try {
      const decoded = jwtDecode<JwtPayload>(token)

      return String(
        decoded[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
        ] ?? ''
      )
    } catch {
      return ''
    }
  }

  const getRole = () => {
    if (!token) {
      return ''
    }

    try {
      const decoded = jwtDecode<JwtPayload>(token)

      return String(
        decoded[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] ?? ''
      )
    } catch {
      return ''
    }
  }

  const loadTickets = async () => {
    try {
      const response = await fetch(
        'http://localhost:5186/api/Tickets',
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      )

      if (!response.ok) {
        throw new Error('Failed to load tickets')
      }

      const data: Ticket[] = await response.json()
      setTickets(data)
    } catch {
      setError('Unable to load tickets.')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = () => {
    setToken('')
    setTickets([])
    setError('')
    setLoading(true)
  }

  useEffect(() => {
    if (token) {
      loadTickets()
    }
  }, [token])

  if (!token) {
    return <Login onLogin={setToken} />
  }

  const username = getUsername()
  const role = getRole()

  return (
    <div className="app">
      <header className="app-header">
        <div className="app-title">
          <h1>Ticket Management System</h1>
          <p>Manage and track support tickets in one place</p>
        </div>
      </header>

      <div className="user-bar">
        <span>{username}</span>
        <button onClick={handleLogout}>Logout</button>
      </div>

      <main className="app-content">
        <section className="create-section">
          <TicketForm
            onTicketCreated={loadTickets}
            token={token}
          />
        </section>

        <section className="tickets-section">
          <h2>List of Tickets Created</h2>

          {loading && <p>Loading tickets...</p>}

          {error && <p>{error}</p>}

          {!loading && !error && tickets.length === 0 && (
            <p>No tickets found.</p>
          )}

          {!loading && !error && tickets.length > 0 && (
            <TicketList
              tickets={tickets}
              role={role}
              token={token}
              onTicketsUpdated={loadTickets}
            />
          )}
        </section>
      </main>
    </div>
  )
}

export default App
