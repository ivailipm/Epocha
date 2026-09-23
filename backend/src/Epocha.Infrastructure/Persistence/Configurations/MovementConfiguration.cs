using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Epocha.Infrastructure.Persistence.Configurations;

public class MovementConfiguration : IEntityTypeConfiguration<Movement>
{
    public void Configure(EntityTypeBuilder<Movement> builder)
    {
        builder.Property(m => m.Name).HasMaxLength(128);
        builder.Property(m => m.Slug).HasMaxLength(128);

        // Ingestion matches movements by slug so casing differences don't create duplicates.
        builder.HasIndex(m => m.Slug).IsUnique();
    }
}
