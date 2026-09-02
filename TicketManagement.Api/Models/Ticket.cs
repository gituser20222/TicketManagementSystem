using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;
}
