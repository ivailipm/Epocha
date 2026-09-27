using Epocha.Domain.Common;

namespace Epocha.Domain.Entities;

/// <summary>A user-named group of saved artworks, e.g. "Impressionism" or "To revisit".</summary>
public class Collection : AuditableEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public required string Name { get; set; }

    public ICollection<Artwork> Artworks { get; set; } = [];
}
