using Epocha.Domain.Common;

namespace Epocha.Domain.Entities;

/// <summary>
/// An art movement or style, e.g. "Impressionism". A lookup table rather than a
/// string column on Artwork, because an artwork can belong to several movements and
/// because we want a stable, deduplicated list to build the filter UI from.
/// </summary>
public class Movement : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Display name, e.g. "Post-Impressionism".</summary>
    public required string Name { get; set; }

    /// <summary>URL-safe, lowercase, deduplication key, e.g. "post-impressionism".</summary>
    public required string Slug { get; set; }

    public ICollection<Artwork> Artworks { get; set; } = [];
}
