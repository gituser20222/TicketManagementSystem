using Microsoft.EntityFrameworkCore;
using TicketManagement.Api.Data;
using TicketManagement.Api.DTOs;
using TicketManagement.Api.Models;

namespace TicketManagement.Api.Services;

public class TicketService : ITicketService
{
    private readonly TicketManagementDbContext _context;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        TicketManagementDbContext context,
        ILogger<TicketService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TicketResponseDto>> GetAllAsync(
        int userId,
        bool isAdmin)
    {
        var query = _context.Tickets.AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(
                ticket => ticket.CreatedByUserId == userId);
        }

        var tickets = await query.ToListAsync();

        return tickets.Select(ticket => new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedDate = ticket.CreatedDate,
            CreatedByUserId = ticket.CreatedByUserId
        }).ToList();
    }

    public async Task<TicketResponseDto?> GetByIdAsync(
        int id,
        int userId,
        bool isAdmin)
    {
        var query = _context.Tickets.AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(
                ticket => ticket.Id == id &&
                          ticket.CreatedByUserId == userId);
        }
        else
        {
            query = query.Where(ticket => ticket.Id == id);
        }

        var ticket = await query.FirstOrDefaultAsync();

        if (ticket == null)
        {
            _logger.LogWarning(
                "Ticket with ID {TicketId} was not found or user {UserId} is not authorized to access it",
                id,
                userId);

            return null;
        }

        return new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedDate = ticket.CreatedDate,
            CreatedByUserId = ticket.CreatedByUserId
        };
    }

    public async Task<TicketResponseDto?> CreateAsync(
        TicketRequestDto request,
        int userId)
    {
        try
        {
            var title = request.Title.Trim();
            var description = request.Description.Trim();

            var duplicateExists = await _context.Tickets.AnyAsync(
                ticket =>
                    ticket.CreatedByUserId == userId &&
                    ticket.Title == title &&
                    ticket.Description == description);

            if (duplicateExists)
            {
                _logger.LogWarning(
                    "Duplicate ticket attempted by user {UserId} with title {Title}",
                    userId,
                    title);

                return null;
            }

            var ticket = new Ticket
            {
                Title = title,
                Description = description,
                Status = request.Status,
                Priority = request.Priority,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = userId
            };

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Ticket created with ID {TicketId} by user {UserId}",
                ticket.Id,
                userId);

            return new TicketResponseDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedDate = ticket.CreatedDate,
                CreatedByUserId = ticket.CreatedByUserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while creating a ticket with title {Title}",
                request.Title);

            throw;
        }
    }

    public async Task<TicketResponseDto?> UpdateAsync(
        int id,
        TicketRequestDto request)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return null;
        }

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;

        await _context.SaveChangesAsync();

        return new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedDate = ticket.CreatedDate,
            CreatedByUserId = ticket.CreatedByUserId
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
        {
            return false;
        }

        _context.Tickets.Remove(ticket);

        await _context.SaveChangesAsync();

        return true;
    }
}
