using System.Text.Json.Serialization;

namespace Epocha.Infrastructure.Sources.ArticApi;

// These types mirror the Art Institute of Chicago API's JSON exactly, and nothing
// else in the codebase uses them. They exist only to deserialize the response; the
// client then maps them into the museum-neutral ArtworkRecord. Keeping them separate
// means an upstream API change only ever touches this folder.
//
// [JsonPropertyName] maps the API's snake_case names onto C#'s PascalCase properties.

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
