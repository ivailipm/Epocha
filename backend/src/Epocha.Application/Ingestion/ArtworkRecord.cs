using Epocha.Domain.Enums;

namespace Epocha.Application.Ingestion;

/// <summary>
/// One artwork in a museum-neutral shape. Each museum client (Art Institute, Met, ...)
/// translates its own JSON into this record, so everything downstream — upserting into
/// Postgres, later indexing into Elasticsearch — never needs to know which museum
/// the data came from.
/// </summary>
/// <remarks>
/// A C# <c>record</c> is an immutable data carrier with value-based equality. It's the
/// idiomatic choice for DTOs like this that are created once and then only read.
/// </remarks>
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

/// <summary>One page of results plus whether asking for the next page is worthwhile.</summary>
public record ArtworkPage(IReadOnlyList<ArtworkRecord> Records, bool HasMore);
