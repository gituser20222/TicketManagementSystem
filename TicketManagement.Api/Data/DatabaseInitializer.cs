using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Api.Models;

namespace TicketManagement.Api.Data;

public static class DatabaseInitializer
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    private static readonly (string Username, string Role)[] DemoUsers =
    {
        ("testadmin", "Admin"),
        ("testuser1", "User"),
        ("testuser2", "User")
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<TicketManagementDbContext>();
        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        await MigrateWithRetryAsync(context, logger);

        if (configuration.GetValue<bool>("Seed:DemoUsers"))
        {
            await SeedDemoUsersAsync(
                context,
                configuration["Seed:DemoPassword"],
                logger);
        }
    }

    // The database container may accept connections a few seconds after the API starts.
    private static async Task MigrateWithRetryAsync(
        TicketManagementDbContext context,
        ILogger logger)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied");
                return;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database not ready (attempt {Attempt}/{Max}), retrying in {Delay}s",
                    attempt,
                    MaxAttempts,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay);
            }
        }
    }

    private static async Task SeedDemoUsersAsync(
        TicketManagementDbContext context,
        string? password,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Seed:DemoUsers is enabled but Seed:DemoPassword is empty; skipping demo users");
            return;
        }

        var hasher = new PasswordHasher<User>();

        foreach (var (username, role) in DemoUsers)
        {
            if (await context.Users.AnyAsync(u => u.Username == username))
            {
                continue;
            }

            var user = new User { Username = username, Role = role };
            user.PasswordHash = hasher.HashPassword(user, password);
            context.Users.Add(user);

            logger.LogInformation("Seeded demo user {Username} ({Role})", username, role);
        }

        await context.SaveChangesAsync();
    }
}
