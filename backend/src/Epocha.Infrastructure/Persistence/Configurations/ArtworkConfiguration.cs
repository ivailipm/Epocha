using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

/// <summary>Fluent API configuration for <see cref="Artwork"/>, the central entity.</summary>
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

        // Same idempotency mechanism as Artist: the ingestion job upserts by this pair
        // instead of blindly inserting, so re-running it doesn't create duplicates.
        builder.HasIndex(a => new { a.SourceSystem, a.SourceExternalId }).IsUnique();

        // Query patterns the browse/search endpoints will need: "artworks in this era"
        // and "artworks not yet pushed to Elasticsearch" (the ingestion job's own
        // bookkeeping query). Indexing them now costs nothing while the table is empty
        // and saves a slow sequential scan once it isn't.
        builder.HasIndex(a => a.Era);
        builder.HasIndex(a => a.LastIndexedAt);

        // One artist can have many artworks; an artwork has at most one (or zero) artists.
        // DeleteBehavior.SetNull: if an Artist row is ever deleted, its artworks aren't
        // deleted with it — ArtistId just goes back to null. Losing an artwork because
        // its artist record was removed would be far worse than losing the attribution.
        builder.HasOne(a => a.Artist)
            .WithMany(ar => ar.Artworks)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-many with no extra columns on the relationship itself, so EF Core can
        // manage the join table (ArtworkMovement) for us without a join entity class.
        builder.HasMany(a => a.Movements)
            .WithMany(m => m.Artworks);
    }
}
