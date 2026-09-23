using Epocha.Domain.Common;

namespace Epocha.Domain.Entities;

/// <summary>
/// An art movement or style, e.g. "Impressionism". A lookup table because an artwork can
/// belong to several movements and the filter UI needs a deduplicated list.
/// </summary>
public class Movement : AuditableEntity
{
    public int Id { get; set; }

    public required string Name { get; set; }

    /// <summary>Lowercase, URL-safe deduplication key, e.g. "post-impressionism".</summary>
    public required string Slug { get; set; }

    public ICollection<Artwork> Artworks { get; set; } = [];
}
