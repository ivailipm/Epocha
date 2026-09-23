using Epocha.Domain.Common;
using Epocha.Domain.Enums;

namespace Epocha.Domain.Entities;

/// <summary>A maker of artworks, deduplicated by (SourceSystem, SourceExternalId).</summary>
public class Artist : AuditableEntity
{
    public int Id { get; set; }

    public SourceSystem SourceSystem { get; set; }
    public required string SourceExternalId { get; set; }

    /// <summary>Display name, e.g. "Claude Monet".</summary>
    public required string Name { get; set; }

    /// <summary>Name for alphabetical sorting, e.g. "Monet, Claude".</summary>
    public string? SortName { get; set; }

    public string? Nationality { get; set; }
    public int? BirthYear { get; set; }
    public int? DeathYear { get; set; }

    public ICollection<Artwork> Artworks { get; set; } = [];
}
