using Epocha.Domain.Common;
using Epocha.Domain.Enums;

namespace Epocha.Domain.Entities;

/// <summary>
/// A maker of artworks. Deduplicated across ingestion runs by
/// (<see cref="SourceSystem"/>, <see cref="SourceExternalId"/>).
/// </summary>
public class Artist : AuditableEntity
{
    public int Id { get; set; }

    public SourceSystem SourceSystem { get; set; }

    /// <summary>The museum's own identifier for this artist.</summary>
    public required string SourceExternalId { get; set; }

    /// <summary>Display name, e.g. "Claude Monet".</summary>
    public required string Name { get; set; }

    /// <summary>Name for alphabetical sorting, e.g. "Monet, Claude".</summary>
    public string? SortName { get; set; }

    public string? Nationality { get; set; }
    public int? BirthYear { get; set; }
    public int? DeathYear { get; set; }

    /// <summary>
    /// Navigation property. EF Core populates this when you <c>.Include()</c> it;
    /// it is not loaded by default (lazy loading is deliberately off — see DbContext).
    /// </summary>
    public ICollection<Artwork> Artworks { get; set; } = [];
}
