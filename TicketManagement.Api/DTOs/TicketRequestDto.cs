using System.ComponentModel.DataAnnotations;
using TicketManagement.Api.Models;

namespace TicketManagement.Api.DTOs;

public class TicketRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }
}
