using TicketManagement.Api.DTOs;

namespace TicketManagement.Api.Services;

public interface ITicketService
{
    Task<List<TicketResponseDto>> GetAllAsync(
        int userId,
        bool isAdmin);

    Task<TicketResponseDto?> GetByIdAsync(
        int id,
        int userId,
        bool isAdmin);

    Task<TicketResponseDto?> CreateAsync(
        TicketRequestDto request,
        int userId);

    Task<TicketResponseDto?> UpdateAsync(
        int id,
        TicketRequestDto request);

    Task<bool> DeleteAsync(int id);
}
