using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicketManagement.Api.Data;
using TicketManagement.Api.DTOs.Auth;
using TicketManagement.Api.Models;
using TicketManagement.Api.Services.Auth;

namespace TicketManagement.Api.Tests;

public class AuthServiceTests
{
    private const string Password = "correct-password";

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "TicketManagement.Api",
                ["Jwt:Audience"] = "TicketManagement.Client",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

    private static async Task<AuthService> CreateServiceAsync(
        string username = "testuser1",
        string role = "User")
    {
        var options = new DbContextOptionsBuilder<TicketManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new TicketManagementDbContext(options);

        var user = new User { Username = username, Role = role };
        user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new AuthService(context, CreateConfiguration());
    }

    private static JwtSecurityToken ReadToken(LoginResponse response) =>
        new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var service = await CreateServiceAsync();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "testuser1",
            Password = Password
        });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_Token_ContainsIssuerAudienceAndFutureExpiry()
    {
        var service = await CreateServiceAsync();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "testuser1",
            Password = Password
        });

        var token = ReadToken(result!);

        Assert.Equal("TicketManagement.Api", token.Issuer);
        Assert.Contains("TicketManagement.Client", token.Audiences);
        Assert.True(token.ValidTo > DateTime.UtcNow);
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public async Task LoginAsync_Token_ContainsUsernameAndRole(string role)
    {
        var service = await CreateServiceAsync("someone", role);

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "someone",
            Password = Password
        });

        var claims = ReadToken(result!).Claims.ToList();

        Assert.Contains(claims, c =>
            (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == role);
        Assert.Contains(claims, c =>
            (c.Type == "unique_name" || c.Type == ClaimTypes.Name) && c.Value == "someone");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        var service = await CreateServiceAsync();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "testuser1",
            Password = "wrong-password"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ReturnsNull()
    {
        var service = await CreateServiceAsync();

        var result = await service.LoginAsync(new LoginRequest
        {
            Username = "nobody",
            Password = Password
        });

        Assert.Null(result);
    }
}
