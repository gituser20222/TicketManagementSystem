using TicketManagement.Api.DTOs.Auth;

namespace TicketManagement.Api.Services.Auth;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}