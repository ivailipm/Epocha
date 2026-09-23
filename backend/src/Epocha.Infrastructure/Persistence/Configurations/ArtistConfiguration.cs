using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(256);
        builder.Property(a => a.SortName).HasMaxLength(256);
        builder.Property(a => a.Nationality).HasMaxLength(128);
        builder.Property(a => a.SourceExternalId).HasMaxLength(64);

        // Identifies an artist across ingestion runs and makes the upsert lookup fast.
        builder.HasIndex(a => new { a.SourceSystem, a.SourceExternalId }).IsUnique();
    }
}
