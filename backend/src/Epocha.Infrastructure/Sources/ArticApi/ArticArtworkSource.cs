using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Epocha.Application.Ingestion;
using Epocha.Domain.Enums;

namespace Epocha.Infrastructure.Sources.ArticApi;

/// <summary>
/// Implements <see cref="IArtworkSource"/> for the Art Institute of Chicago's public API
/// (no API key needed). Fetches one page, then translates each museum-specific
/// artwork into the neutral <see cref="ArtworkRecord"/>.
/// </summary>
/// <remarks>
/// The <see cref="HttpClient"/> arrives via constructor injection. It's created by
/// <c>AddHttpClient</c> (see DependencyInjection.cs), which manages the underlying
/// connections for us — never <c>new HttpClient()</c> per request, which can exhaust
/// sockets.
/// </remarks>
internal sealed partial class ArticArtworkSource(HttpClient http) : IArtworkSource
{
    // Only ask for the fields we use; the API's default response is much larger.
    private const string Fields =
        "id,title,description,medium_display,classification_title,department_title,dimensions," +
        "credit_line,date_display,date_start,date_end,image_id,is_public_domain," +
        "artist_id,artist_title,style_titles";

    private const string DefaultIiifUrl = "https://www.artic.edu/iiif/2";

    public async Task<ArtworkPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var url = $"artworks?page={page}&limit={pageSize}&fields={Fields}";

        var response = await http.GetFromJsonAsync<ArticResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Art Institute API returned an empty response.");

        var iiif = response.Config?.IiifUrl ?? DefaultIiifUrl;

        var records = response.Data
            .Where(a => !string.IsNullOrWhiteSpace(a.Title))
            .Select(a => ToRecord(a, iiif))
            .ToList();

        return new ArtworkPage(records, HasMore: response.Pagination.CurrentPage < response.Pagination.TotalPages);
    }

    private static ArtworkRecord ToRecord(ArticArtwork a, string iiif)
    {
        var id = a.Id.ToString();

        // IIIF is an image-serving standard: the URL itself says what size you want.
        // "843," means "843px wide, height scaled to match".
        var hasImage = !string.IsNullOrEmpty(a.ImageId);

        return new ArtworkRecord(
            SourceSystem: SourceSystem.ArtInstituteOfChicago,
            ExternalId: id,
            Title: a.Title!,
            SourceUrl: $"https://www.artic.edu/artworks/{id}",
            Description: StripHtml(a.Description),
            MediumDisplay: a.MediumDisplay,
            Classification: a.ClassificationTitle,
            Department: a.DepartmentTitle,
            Dimensions: a.Dimensions,
            CreditLine: a.CreditLine,
            DateDisplay: a.DateDisplay,
            DateStartYear: a.DateStart,
            DateEndYear: a.DateEnd,
            ImageUrl: hasImage ? $"{iiif}/{a.ImageId}/full/843,/0/default.jpg" : null,
            ThumbnailUrl: hasImage ? $"{iiif}/{a.ImageId}/full/300,/0/default.jpg" : null,
            IsPublicDomain: a.IsPublicDomain,
            Artist: a.ArtistId is { } artistId && !string.IsNullOrWhiteSpace(a.ArtistTitle)
                ? new ArtistRecord(artistId.ToString(), a.ArtistTitle)
                : null,
            Movements: a.StyleTitles ?? []);
    }

    // The API's descriptions arrive as HTML ("<p>...</p>"); we store plain text.
    private static string? StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var text = WebUtility.HtmlDecode(HtmlTag().Replace(html, " "));
        return string.IsNullOrWhiteSpace(text) ? null : Regex.Replace(text, @"\s+", " ").Trim();
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTag();
}
