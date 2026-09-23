using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for <see cref="Artist"/>. EF Core would infer a working
/// table from the entity's properties alone, but "working" isn't "correct" — this is
/// where we pin down constraints (required lengths, uniqueness) that data annotations
/// on the entity can't express cleanly, keeping the Domain entity free of EF-specific
/// attributes.
/// </summary>
public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(256);
        builder.Property(a => a.SortName).HasMaxLength(256);
        builder.Property(a => a.Nationality).HasMaxLength(128);
        builder.Property(a => a.SourceExternalId).HasMaxLength(64);

        // An artist row is uniquely identified by which museum it came from plus that
        // museum's own id for it. This index both enforces that at the database level
        // and gives ingestion a fast lookup to decide "insert new artist" vs.
        // "update the one I already have" without scanning the whole table.
        builder.HasIndex(a => new { a.SourceSystem, a.SourceExternalId }).IsUnique();
    }
}
