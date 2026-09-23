using Epocha.Application.Abstractions;
using Epocha.Domain.Common;
using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Infrastructure.Persistence;

/// <summary>
/// EF Core's "unit of work": it represents one session with the database, tracks
/// in-memory changes to the entities you load through it, and turns
/// <see cref="SaveChangesAsync"/> into the actual INSERT/UPDATE/DELETE statements.
/// </summary>
/// <remarks>
/// A <c>DbSet&lt;T&gt;</c> property below is roughly "the table for T". Nothing here
/// says HOW each entity maps to columns — that's delegated to the IEntityTypeConfiguration
/// classes in Persistence/Configurations and picked up automatically in
/// <see cref="OnModelCreating"/>, which keeps this class from becoming a giant file of
/// Fluent API calls as more entities are added.
/// </remarks>
public class EpochaDbContext(DbContextOptions<EpochaDbContext> options) : DbContext(options), IEpochaDbContext
{
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scans this assembly for every class that implements IEntityTypeConfiguration<T>
        // and applies it. Add a new entity + its configuration class and it's picked up
        // here with no further changes needed.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EpochaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Stamps CreatedAt/UpdatedAt automatically so every call site doesn't have to
    /// remember to do it. Overriding SaveChanges(Async) is the standard EF Core hook
    /// for "run this logic right before every write."
    /// </summary>
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
