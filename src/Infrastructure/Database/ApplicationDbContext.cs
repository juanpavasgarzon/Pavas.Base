using Application.Abstractions.Data;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }
}
