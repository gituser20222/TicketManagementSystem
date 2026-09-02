using System.Text.Json.Serialization;

namespace TicketManagement.Api.Models;

public enum TicketStatus
{
    Open,
    InProgress,
    Closed
}