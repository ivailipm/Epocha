namespace Epocha.Application.Search;

/// <summary>The fields a gallery card needs; the full record comes from the detail endpoint.</summary>
public record ArtworkSummary(
    int Id,
    string Title,
    string? ArtistName,
    string? DateDisplay,
    int? DateStartYear,
    string Era,
    string MediumCategory,
    string? ThumbnailUrl,
    bool IsPublicDomain);

public record FacetBucket(string Value, long Count);

/// <param name="Facets">Bucket counts keyed by facet name: era, medium, movement, artist.</param>
public record ArtworkSearchResult(
    IReadOnlyList<ArtworkSummary> Items,
    long Total,
    int Page,
    int PageSize,
    IReadOnlyDictionary<string, IReadOnlyList<FacetBucket>> Facets)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}
