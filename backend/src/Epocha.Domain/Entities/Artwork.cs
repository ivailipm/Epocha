using Epocha.Domain.Common;
using Epocha.Domain.Enums;

namespace Epocha.Domain.Entities;

/// <summary>
/// The central entity: one object from a museum's collection.
/// Postgres holds the authoritative copy; Elasticsearch holds a denormalised
/// projection of it for search.
/// </summary>
public class Artwork : AuditableEntity
{
    public int Id { get; set; }

    // ---- Provenance -------------------------------------------------------
    // Together these form a unique index. Ingestion uses them to decide between
    // INSERT and UPDATE, which is what makes re-running the job safe (idempotent).

    public SourceSystem SourceSystem { get; set; }
    public required string SourceExternalId { get; set; }

    /// <summary>Public web page for this object on the museum's own site.</summary>
    public string? SourceUrl { get; set; }

    // ---- Descriptive ------------------------------------------------------

    public required string Title { get; set; }

    /// <summary>Longer description or curatorial text, when the source provides one.</summary>
    public string? Description { get; set; }

    /// <summary>e.g. "Oil on canvas" — preserved verbatim for display.</summary>
    public string? MediumDisplay { get; set; }

    /// <summary>Normalised bucket derived from <see cref="MediumDisplay"/> for faceting.</summary>
    public MediumCategory MediumCategory { get; set; }

    /// <summary>e.g. "Painting and Sculpture of Europe" — the museum's own grouping.</summary>
    public string? Department { get; set; }

    /// <summary>e.g. "39 x 25 cm".</summary>
    public string? Dimensions { get; set; }

    public string? CreditLine { get; set; }

    // ---- Dating -----------------------------------------------------------
    // Museums date objects imprecisely ("c. 1503-19", "late 4th century BC").
    // We keep the human string for display AND a normalised numeric range for
    // filtering and for the timeline view. Negative years mean BC.

    /// <summary>Verbatim date text from the museum, e.g. "c. 1503–19".</summary>
    public string? DateDisplay { get; set; }

    public int? DateStartYear { get; set; }
    public int? DateEndYear { get; set; }

    /// <summary>Derived from <see cref="DateStartYear"/> at ingestion time.</summary>
    public Era Era { get; set; }

    // ---- Imagery ----------------------------------------------------------

    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Whether the image may be displayed freely. Guard the gallery on this so the
    /// project does not republish rights-restricted imagery.
    /// </summary>
    public bool IsPublicDomain { get; set; }

    // ---- Relationships ----------------------------------------------------

    /// <summary>
    /// Nullable: plenty of museum objects are by an unknown or unattributed maker.
    /// Modelling this honestly now saves a painful migration later.
    /// </summary>
    public int? ArtistId { get; set; }
    public Artist? Artist { get; set; }

    /// <summary>Many-to-many. EF Core 10 maps this without an explicit join entity.</summary>
    public ICollection<Movement> Movements { get; set; } = [];

    // ---- Search bookkeeping -----------------------------------------------

    /// <summary>
    /// When this row was last written to Elasticsearch. Null means "never indexed".
    /// This is the seam where you would later bolt on an outbox: a job can find rows
    /// where UpdatedAt &gt; LastIndexedAt and repair drift without a full reindex.
    /// </summary>
    public DateTimeOffset? LastIndexedAt { get; set; }
}
