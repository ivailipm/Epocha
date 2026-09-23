using Epocha.Domain.Enums;

namespace Epocha.Application.Ingestion;

/// <summary>
/// One artwork in a museum-neutral shape. Each museum client maps its own JSON into this,
/// so upserting and indexing never depend on which museum the data came from.
/// </summary>
public record ArtworkRecord(
    SourceSystem SourceSystem,
    string ExternalId,
    string Title,
    string? SourceUrl,
    string? Description,
    string? MediumDisplay,
    string? Classification,
    string? Department,
    string? Dimensions,
    string? CreditLine,
    string? DateDisplay,
    int? DateStartYear,
    int? DateEndYear,
    string? ImageUrl,
    string? ThumbnailUrl,
    bool IsPublicDomain,
    ArtistRecord? Artist,
    IReadOnlyList<string> Movements);

public record ArtistRecord(string ExternalId, string Name);

/// <summary>One page of results plus whether another page is available.</summary>
public record ArtworkPage(IReadOnlyList<ArtworkRecord> Records, bool HasMore);
