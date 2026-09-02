using TicketManagement.Api.Models;

namespace TicketManagement.Api.DTOs;

public class TicketResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CreatedByUserId { get; set; }
}