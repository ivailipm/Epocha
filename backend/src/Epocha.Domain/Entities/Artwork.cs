using Epocha.Domain.Common;
using Epocha.Domain.Enums;

namespace Epocha.Domain.Entities;

/// <summary>
/// One object from a museum's collection. Postgres holds the authoritative copy;
/// Elasticsearch holds a denormalised projection of it for search.
/// </summary>
public class Artwork : AuditableEntity
{
    public int Id { get; set; }

    // (SourceSystem, SourceExternalId) is unique; ingestion upserts on it.
    public SourceSystem SourceSystem { get; set; }
    public required string SourceExternalId { get; set; }

    /// <summary>Public page for this object on the museum's own site.</summary>
    public string? SourceUrl { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }

    /// <summary>Verbatim from the museum, e.g. "Oil on canvas".</summary>
    public string? MediumDisplay { get; set; }

    /// <summary>Normalised bucket derived from <see cref="MediumDisplay"/>, used for faceting.</summary>
    public MediumCategory MediumCategory { get; set; }

    public string? Department { get; set; }
    public string? Dimensions { get; set; }
    public string? CreditLine { get; set; }

    // Museums date objects imprecisely ("c. 1503-19"), so keep the display text plus a
    // normalised numeric range for filtering and the timeline. Negative years are BC.
    public string? DateDisplay { get; set; }
    public int? DateStartYear { get; set; }
    public int? DateEndYear { get; set; }

    /// <summary>Derived from <see cref="DateStartYear"/> at ingestion time.</summary>
    public Era Era { get; set; }

    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    /// <summary>Whether the image may be shown freely; the gallery should respect this.</summary>
    public bool IsPublicDomain { get; set; }

    /// <summary>Nullable: many objects have an unknown or unattributed maker.</summary>
    public int? ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public ICollection<Movement> Movements { get; set; } = [];

    /// <summary>
    /// When this row was last written to Elasticsearch; null means never indexed. Rows where
    /// UpdatedAt is later than this are stale in the index and get re-indexed.
    /// </summary>
    public DateTimeOffset? LastIndexedAt { get; set; }
}
