import '@testing-library/jest-dom/vitest'

import { render, screen } from '@testing-library/react'

import { describe, it, expect, vi } from 'vitest'

import TicketList from './TicketList'

describe('TicketList', () => {
  it('renders ticket title', () => {
    const tickets = [
      {
        id: 1,
        title: 'Test Ticket',
        description: 'Test Description',
        status: 'Open',
        priority: 'Low',
        createdDate: '2026-08-29',
        createdByUserId: 1,
      }
    ]

    render(
      <TicketList
        tickets={tickets}
        role="User"
        token="test-token"
        onTicketsUpdated={vi.fn()}
      />
    )

    expect(screen.getByText('Test Ticket')).toBeInTheDocument()
  })

  it('renders multiple tickets', () => {
    const tickets = [
      {
        id: 1,
        title: 'First Ticket',
        description: 'First Description',
        status: 'Open',
        priority: 'Low',
        createdDate: '2026-08-29',
        createdByUserId: 1,
      },
      {
        id: 2,
        title: 'Second Ticket',
        description: 'Second Description',
        status: 'Closed',
        priority: 'High',
        createdDate: '2026-08-29',
        createdByUserId: 2,
      }
    ]

    render(
      <TicketList
        tickets={tickets}
        role="User"
        token="test-token"
        onTicketsUpdated={vi.fn()}
      />
    )

    expect(screen.getByText('First Ticket')).toBeInTheDocument()

    expect(screen.getByText('Second Ticket')).toBeInTheDocument()
  })
})