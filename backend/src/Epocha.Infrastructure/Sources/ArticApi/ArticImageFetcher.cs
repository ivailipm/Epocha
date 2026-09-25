using Epocha.Application.Images;

namespace Epocha.Infrastructure.Sources.ArticApi;

/// <summary>Fetches an image from artic.edu with the Referer it requires; see ArtworkImageService.</summary>
internal sealed class ArticImageFetcher(HttpClient http) : IExternalImageFetcher
{
    public async Task<ExternalImage?> FetchAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Referrer = new Uri("https://www.artic.edu/");

        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return new ExternalImage(bytes, contentType);
    }
}
