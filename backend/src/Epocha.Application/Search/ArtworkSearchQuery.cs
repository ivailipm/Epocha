namespace Epocha.Application.Search;

public enum ArtworkSort
{
    Relevance,
    DateAsc,
    DateDesc,
    TitleAsc,
    TitleDesc
}

/// <summary>
/// A search request. Filter lists are OR-ed within a facet (era Baroque OR Modern) and
/// AND-ed across facets. The year range is inclusive and applies to the artwork's start year.
/// </summary>
public record ArtworkSearchQuery
{
    public string? Text { get; init; }
    public IReadOnlyList<string> Eras { get; init; } = [];
    public IReadOnlyList<string> Mediums { get; init; } = [];
    public IReadOnlyList<string> Movements { get; init; } = [];
    public IReadOnlyList<string> Artists { get; init; } = [];
    public int? YearFrom { get; init; }
    public int? YearTo { get; init; }
    public ArtworkSort Sort { get; init; } = ArtworkSort.Relevance;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = ArtworkSearchService.DefaultPageSize;
}
