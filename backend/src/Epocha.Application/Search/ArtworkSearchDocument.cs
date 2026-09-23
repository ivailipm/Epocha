using Epocha.Domain.Entities;

namespace Epocha.Application.Search;

/// <summary>
/// An artwork as stored in Elasticsearch: a flat projection with the artist name and movement
/// names copied in. Enums are stored as names so filters and facets are readable. The Postgres
/// Id is the document id, so re-indexing overwrites rather than duplicates.
/// </summary>
public record ArtworkSearchDocument(
    int Id,
    string Title,
    string? Description,
    string? ArtistName,
    IReadOnlyList<string> Movements,
    string Era,
    string MediumCategory,
    string? MediumDisplay,
    string? Department,
    string? DateDisplay,
    int? DateStartYear,
    int? DateEndYear,
    string SourceSystem,
    string? SourceUrl,
    string? ImageUrl,
    string? ThumbnailUrl,
    bool IsPublicDomain)
{
    /// <remarks>Requires <see cref="Artwork.Artist"/> and <see cref="Artwork.Movements"/> to be loaded.</remarks>
    public static ArtworkSearchDocument From(Artwork a) => new(
        a.Id,
        a.Title,
        a.Description,
        a.Artist?.Name,
        a.Movements.Select(m => m.Name).OrderBy(n => n).ToList(),
        a.Era.ToString(),
        a.MediumCategory.ToString(),
        a.MediumDisplay,
        a.Department,
        a.DateDisplay,
        a.DateStartYear,
        a.DateEndYear,
        a.SourceSystem.ToString(),
        a.SourceUrl,
        a.ImageUrl,
        a.ThumbnailUrl,
        a.IsPublicDomain);
}
