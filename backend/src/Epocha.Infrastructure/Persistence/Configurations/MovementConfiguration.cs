using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

/// <summary>Fluent API configuration for <see cref="Movement"/>.</summary>
public class MovementConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.Property(m => m.Name).HasMaxLength(128);

        builder.Property(m => m.Slug).HasMaxLength(128);

        // The slug ("post-impressionism") is what ingestion matches on to avoid
        // creating "Impressionism" twice when it shows up under slightly different
        // casing across the two museum APIs.
        builder.HasIndex(m => m.Slug).IsUnique();
    }
}
