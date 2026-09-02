using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.DTOs;
using TicketManagement.Api.Services;

namespace TicketManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketResponseDto>>> GetTickets()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");

        var tickets = await _ticketService.GetAllAsync(
            userId,
            isAdmin);

        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketResponseDto>> GetTicket(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole("Admin");

        var ticket = await _ticketService.GetByIdAsync(
            id,
            userId,
            isAdmin);

        if (ticket == null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponseDto>> CreateTicket(
        TicketRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var createdTicket = await _ticketService.CreateAsync(
            request,
            userId);

        if (createdTicket == null)
        {
            return Conflict(
                "A ticket with the same title and description already exists.");
        }

        return CreatedAtAction(
            nameof(GetTicket),
            new { id = createdTicket.Id },
            createdTicket);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TicketResponseDto>> UpdateTicket(
        int id,
        TicketRequestDto request)
    {
        var updatedTicket = await _ticketService.UpdateAsync(
            id,
            request);

        if (updatedTicket == null)
        {
            return NotFound();
        }

        return Ok(updatedTicket);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var deleted = await _ticketService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
