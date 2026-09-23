namespace Epocha.Application.Search;

/// <summary>Normalises a search request, then delegates to the search backend.</summary>
public class ArtworkSearchService(IArtworkSearcher searcher)
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 100;

    /// <summary>Elasticsearch refuses to page past this many results (from + size) by default.</summary>
    public const int MaxResultWindow = 10_000;

    /// <summary>Returns an error message if the query can't be served, otherwise null.</summary>
    public static string? Validate(ArtworkSearchQuery query)
    {
        if (query.YearFrom > query.YearTo)
        {
            return "yearFrom must not be greater than yearTo.";
        }

        var size = Math.Clamp(query.PageSize, 1, MaxPageSize);
        if ((long)Math.Max(query.Page, 1) * size > MaxResultWindow)
        {
            return $"Cannot page beyond the first {MaxResultWindow} results; narrow the search with filters.";
        }

        return null;
    }

    public Task<ArtworkSearchResult> SearchAsync(ArtworkSearchQuery query, CancellationToken cancellationToken)
    {
        var normalised = query with
        {
            Text = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim(),
            Page = Math.Max(query.Page, 1),
            PageSize = Math.Clamp(query.PageSize, 1, MaxPageSize),
            Eras = Clean(query.Eras),
            Mediums = Clean(query.Mediums),
            Movements = Clean(query.Movements),
            Artists = Clean(query.Artists)
        };

        return searcher.SearchAsync(normalised, cancellationToken);
    }

    private static IReadOnlyList<string> Clean(IReadOnlyList<string> values) =>
        values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct().ToList();
}
