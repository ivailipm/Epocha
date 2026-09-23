using System.Text.Json.Serialization;

namespace Epocha.Infrastructure.Sources.ArticApi;

// Mirrors the Art Institute API's JSON. Used only for deserialization, then mapped to ArtworkRecord.

internal record ArticResponse(
    [property: JsonPropertyName("pagination")] ArticPagination Pagination,
    [property: JsonPropertyName("data")] List<ArticArtwork> Data,
    [property: JsonPropertyName("config")] ArticConfig? Config);

internal record ArticPagination(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("total_pages")] int TotalPages);

internal record ArticConfig(
    [property: JsonPropertyName("iiif_url")] string? IiifUrl);

internal record ArticArtwork(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("medium_display")] string? MediumDisplay,
    [property: JsonPropertyName("classification_title")] string? ClassificationTitle,
    [property: JsonPropertyName("department_title")] string? DepartmentTitle,
    [property: JsonPropertyName("dimensions")] string? Dimensions,
    [property: JsonPropertyName("credit_line")] string? CreditLine,
    [property: JsonPropertyName("date_display")] string? DateDisplay,
    [property: JsonPropertyName("date_start")] int? DateStart,
    [property: JsonPropertyName("date_end")] int? DateEnd,
    [property: JsonPropertyName("image_id")] string? ImageId,
    [property: JsonPropertyName("is_public_domain")] bool IsPublicDomain,
    [property: JsonPropertyName("artist_id")] int? ArtistId,
    [property: JsonPropertyName("artist_title")] string? ArtistTitle,
    [property: JsonPropertyName("style_titles")] List<string>? StyleTitles);
