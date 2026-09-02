import { useState } from 'react'

interface LoginProps {
  onLogin: (token: string) => void
}

function Login({ onLogin }: LoginProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoggingIn, setIsLoggingIn] = useState(false)

  const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError('')
    setIsLoggingIn(true)

    try {
      const response = await fetch('http://localhost:5186/api/Auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username,
          password,
        }),
      })

      if (!response.ok) {
        setError('Invalid username or password.')
        return
      }

      const data = await response.json()

      onLogin(data.token)
    } catch {
      setError('Unable to connect to the API.')
    } finally {
      setIsLoggingIn(false)
    }
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="login-header">
          <h1>Ticket Management System</h1>
          <p>Sign in to manage your support tickets</p>
        </div>

        <form onSubmit={handleLogin} className="login-form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              placeholder="Enter your username"
              value={username}
              required
              onChange={(e) => {
                setUsername(e.target.value)
                setError('')
              }}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              placeholder="Enter your password"
              value={password}
              required
              onChange={(e) => {
                setPassword(e.target.value)
                setError('')
              }}
            />
          </div>

          {error && <p className="login-error">{error}</p>}

          <button
            type="submit"
            className="login-button"
            disabled={isLoggingIn}
          >
            {isLoggingIn ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
      </div>
    </div>
  )
}

export default Login
