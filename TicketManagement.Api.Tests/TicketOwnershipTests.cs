using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TicketManagement.Api.Data;
using TicketManagement.Api.Models;
using TicketManagement.Api.Services;

namespace TicketManagement.Api.Tests;

public class TicketOwnershipTests
{
    private const int UserOneId = 1;
    private const int UserTwoId = 2;

    private static TicketManagementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TicketManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TicketManagementDbContext(options);
    }

    private static async Task<(TicketService Service, Ticket UserOneTicket, Ticket UserTwoTicket)> CreateServiceWithTicketsAsync()
    {
        var context = CreateDbContext();

        var userOneTicket = new Ticket
        {
            Title = "User one ticket",
            Description = "Owned by user one",
            CreatedByUserId = UserOneId
        };

        var userTwoTicket = new Ticket
        {
            Title = "User two ticket",
            Description = "Owned by user two",
            CreatedByUserId = UserTwoId
        };

        context.Tickets.AddRange(userOneTicket, userTwoTicket);
        await context.SaveChangesAsync();

        var service = new TicketService(
            context,
            NullLogger<TicketService>.Instance);

        return (service, userOneTicket, userTwoTicket);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ReturnsOnlyOwnTickets()
    {
        var (service, userOneTicket, _) = await CreateServiceWithTicketsAsync();

        var result = await service.GetAllAsync(UserOneId, isAdmin: false);

        var ticket = Assert.Single(result);
        Assert.Equal(userOneTicket.Id, ticket.Id);
        Assert.Equal(UserOneId, ticket.CreatedByUserId);
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUserWithNoTickets_ReturnsEmptyList()
    {
        var (service, _, _) = await CreateServiceWithTicketsAsync();

        var result = await service.GetAllAsync(userId: 99, isAdmin: false);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_AsAdmin_ReturnsAllTickets()
    {
        var (service, _, _) = await CreateServiceWithTicketsAsync();

        var result = await service.GetAllAsync(UserOneId, isAdmin: true);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_RegularUserOwnTicket_ReturnsTicket()
    {
        var (service, userOneTicket, _) = await CreateServiceWithTicketsAsync();

        var result = await service.GetByIdAsync(
            userOneTicket.Id, UserOneId, isAdmin: false);

        Assert.NotNull(result);
        Assert.Equal(userOneTicket.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_RegularUserAnotherUsersTicket_ReturnsNull()
    {
        var (service, _, userTwoTicket) = await CreateServiceWithTicketsAsync();

        var result = await service.GetByIdAsync(
            userTwoTicket.Id, UserOneId, isAdmin: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_AdminAnotherUsersTicket_ReturnsTicket()
    {
        var (service, _, userTwoTicket) = await CreateServiceWithTicketsAsync();

        var result = await service.GetByIdAsync(
            userTwoTicket.Id, UserOneId, isAdmin: true);

        Assert.NotNull(result);
        Assert.Equal(UserTwoId, result.CreatedByUserId);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedByUserIdToCaller()
    {
        var context = CreateDbContext();
        var service = new TicketService(
            context,
            NullLogger<TicketService>.Instance);

        var created = await service.CreateAsync(
            new TicketManagement.Api.DTOs.TicketRequestDto
            {
                Title = "Owned ticket",
                Description = "Should belong to the caller"
            },
            UserTwoId);

        Assert.NotNull(created);
        Assert.Equal(UserTwoId, created.CreatedByUserId);
    }
}
