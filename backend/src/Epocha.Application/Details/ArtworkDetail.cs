namespace Epocha.Application.Details;

public record ArtistSummary(int Id, string Name, string? Nationality, int? BirthYear, int? DeathYear);

/// <summary>Everything an artwork detail page needs. Unlike ArtworkSummary, this is the full record.</summary>
public record ArtworkDetail(
    int Id,
    string Title,
    string? Description,
    string? MediumDisplay,
    string MediumCategory,
    string? Department,
    string? Dimensions,
    string? CreditLine,
    string? DateDisplay,
    int? DateStartYear,
    int? DateEndYear,
    string Era,
    string? ImageUrl,
    string? ThumbnailUrl,
    bool IsPublicDomain,
    string SourceSystem,
    string? SourceUrl,
    ArtistSummary? Artist,
    IReadOnlyList<string> Movements);
