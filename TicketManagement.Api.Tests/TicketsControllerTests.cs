using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketManagement.Api.Controllers;
using TicketManagement.Api.DTOs;
using TicketManagement.Api.Services;

namespace TicketManagement.Api.Tests;

public class TicketsControllerTests
{
    private static TicketsController CreateController(
        Mock<ITicketService> service)
    {
        var controller = new TicketsController(service.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "1"),
                            new Claim(ClaimTypes.Role, "User")
                        },
                        "TestAuth"))
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetTicket_WhenTicketDoesNotExist_ReturnsNotFound()
    {
        var service = new Mock<ITicketService>();

        service
            .Setup(x => x.GetByIdAsync(999, 1, false))
            .ReturnsAsync((TicketResponseDto?)null);

        var controller = CreateController(service);

        var result = await controller.GetTicket(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetTicket_WhenTicketExists_ReturnsOk()
    {
        var service = new Mock<ITicketService>();

        var ticket = new TicketResponseDto
        {
            Id = 1,
            Title = "Test Ticket",
            Description = "Test Description"
        };

        service
            .Setup(x => x.GetByIdAsync(1, 1, false))
            .ReturnsAsync(ticket);

        var controller = CreateController(service);

        var result = await controller.GetTicket(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        Assert.Equal(ticket, okResult.Value);
    }

    [Fact]
    public async Task CreateTicket_ReturnsCreated()
    {
        var service = new Mock<ITicketService>();

        var ticket = new TicketResponseDto
        {
            Id = 1,
            Title = "New Ticket",
            Description = "New Description"
        };

        service
            .Setup(x => x.CreateAsync(
                It.IsAny<TicketRequestDto>(),
                1))
            .ReturnsAsync(ticket);

        var controller = CreateController(service);

        var request = new TicketRequestDto
        {
            Title = "New Ticket",
            Description = "New Description"
        };

        var result = await controller.CreateTicket(request);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(ticket, createdResult.Value);
    }

    [Fact]
    public async Task UpdateTicket_WhenTicketExists_ReturnsOk()
    {
        var service = new Mock<ITicketService>();

        var ticket = new TicketResponseDto
        {
            Id = 1,
            Title = "Updated Ticket",
            Description = "Updated Description"
        };

        service
            .Setup(x => x.UpdateAsync(
                1,
                It.IsAny<TicketRequestDto>()))
            .ReturnsAsync(ticket);

        var controller = CreateController(service);

        var request = new TicketRequestDto
        {
            Title = "Updated Ticket",
            Description = "Updated Description"
        };

        var result = await controller.UpdateTicket(1, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        Assert.Equal(ticket, okResult.Value);
    }

    [Fact]
    public async Task DeleteTicket_WhenTicketDoesNotExist_ReturnsNotFound()
    {
        var service = new Mock<ITicketService>();

        service
            .Setup(x => x.DeleteAsync(999))
            .ReturnsAsync(false);

        var controller = CreateController(service);

        var result = await controller.DeleteTicket(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteTicket_WhenTicketExists_ReturnsNoContent()
    {
        var service = new Mock<ITicketService>();

        service
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(true);

        var controller = CreateController(service);

        var result = await controller.DeleteTicket(1);

        Assert.IsType<NoContentResult>(result);
    }
}