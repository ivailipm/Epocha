using Epocha.Application.Abstractions;
using Epocha.Domain.Common;
using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Infrastructure.Persistence;

public class EpochaDbContext(DbContextOptions<EpochaDbContext> options) : DbContext(options), IEpochaDbContext
{
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up every IEntityTypeConfiguration<T> in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EpochaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // Stamps CreatedAt/UpdatedAt on every write so callers don't have to.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
