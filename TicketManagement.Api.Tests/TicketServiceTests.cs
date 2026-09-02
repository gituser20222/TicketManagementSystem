using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TicketManagement.Api.Data;
using TicketManagement.Api.DTOs;
using TicketManagement.Api.Models;
using TicketManagement.Api.Services;

namespace TicketManagement.Api.Tests;

public class TicketServiceTests
{
    private readonly ILogger<TicketService> _logger =
        NullLogger<TicketService>.Instance;

    private TicketManagementDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TicketManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TicketManagementDbContext(options);
    }

    [Fact]
    public async Task GetAllTickets_ReturnsTickets()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        context.Tickets.Add(new Ticket
        {
            Title = "Test Ticket",
            Description = "Test Description"
        });

        await context.SaveChangesAsync();

        var result = await service.GetAllAsync(1, true);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var result = await service.GetByIdAsync(999, 1, true);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketExists_ReturnsTicket()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var ticket = new Ticket
        {
            Title = "Test Ticket",
            Description = "Test Description"
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(ticket.Id, 1, true);

        Assert.NotNull(result);
        Assert.Equal("Test Ticket", result.Title);
    }

    [Fact]
    public async Task CreateAsync_CreatesTicket()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var request = new TicketRequestDto
        {
            Title = "New Ticket",
            Description = "New Description"
        };

        var result = await service.CreateAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal("New Ticket", result.Title);
        Assert.Single(context.Tickets);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketExists_UpdatesTicket()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var ticket = new Ticket
        {
            Title = "Original Title",
            Description = "Original Description"
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        var request = new TicketRequestDto
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        var result = await service.UpdateAsync(ticket.Id, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var request = new TicketRequestDto
        {
            Title = "Updated Title",
            Description = "Updated Description"
        };

        var result = await service.UpdateAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenTicketDoesNotExist_ReturnsFalse()
    {
        var context = CreateDbContext();
        var service = new TicketService(context, _logger);

        var result = await service.DeleteAsync(999);

        Assert.False(result);
    }
}