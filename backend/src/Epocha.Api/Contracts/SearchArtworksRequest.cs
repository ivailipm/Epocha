using Epocha.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace Epocha.Api.Contracts;

/// <summary>
/// Query-string contract for the search endpoint. Repeat a parameter to select several values,
/// e.g. <c>?era=Baroque&amp;era=Modern</c>.
/// </summary>
public class SearchArtworksRequest
{
    [FromQuery(Name = "q")]
    public string? Text { get; init; }

    [FromQuery(Name = "era")]
    public string[] Eras { get; init; } = [];

    [FromQuery(Name = "medium")]
    public string[] Mediums { get; init; } = [];

    [FromQuery(Name = "movement")]
    public string[] Movements { get; init; } = [];

    [FromQuery(Name = "artist")]
    public string[] Artists { get; init; } = [];

    public int? YearFrom { get; init; }
    public int? YearTo { get; init; }
    public ArtworkSort Sort { get; init; } = ArtworkSort.Relevance;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = ArtworkSearchService.DefaultPageSize;

    public ArtworkSearchQuery ToQuery() => new()
    {
        Text = Text,
        Eras = Eras,
        Mediums = Mediums,
        Movements = Movements,
        Artists = Artists,
        YearFrom = YearFrom,
        YearTo = YearTo,
        Sort = Sort,
        Page = Page,
        PageSize = PageSize
    };
}
