using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

public class ArtworkConfiguration : IEntityTypeConfiguration<Artwork>
{
    public void Configure(EntityTypeBuilder<Artwork> builder)
    {
        builder.Property(a => a.SourceExternalId).HasMaxLength(64);
        builder.Property(a => a.Title).HasMaxLength(512);
        builder.Property(a => a.MediumDisplay).HasMaxLength(512);
        builder.Property(a => a.Department).HasMaxLength(256);
        builder.Property(a => a.Dimensions).HasMaxLength(256);
        builder.Property(a => a.DateDisplay).HasMaxLength(128);

        // Ingestion upserts on this pair, which makes re-runs idempotent.
        builder.HasIndex(a => new { a.SourceSystem, a.SourceExternalId }).IsUnique();

        builder.HasIndex(a => a.Era);
        // Backs the "not yet indexed" query used by the indexing backlog pass.
        builder.HasIndex(a => a.LastIndexedAt);

        // Deleting an artist keeps its artworks and just clears the attribution.
        builder.HasOne(a => a.Artist)
            .WithMany(ar => ar.Artworks)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-many; EF Core creates the ArtworkMovement join table.
        builder.HasMany(a => a.Movements)
            .WithMany(m => m.Artworks);
    }
}
