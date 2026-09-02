using Microsoft.EntityFrameworkCore;
using TicketManagement.Api.Models;

namespace TicketManagement.Api.Data;

public class TicketManagementDbContext : DbContext
{
    public TicketManagementDbContext(
        DbContextOptions<TicketManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>()
            .Property(t => t.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Ticket>()
            .Property(t => t.Priority)
            .HasConversion<string>();
    }
}