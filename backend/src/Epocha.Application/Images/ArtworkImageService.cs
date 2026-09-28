using Epocha.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Images;

/// <summary>
/// Serves artwork images through our own domain instead of the museum's. The Art Institute's
/// site blocks image requests whose Referer isn't its own, so a browser can never load its
/// image URLs directly from another site; fetching them server-side (Referer set by us) and
/// streaming the bytes back is the only way to display them elsewhere.
///
/// A hosting-provider IP (ours) gets an interactive Cloudflare challenge from the museum's
/// image host that no server-side HTTP client can solve, so the live fetch below only works
/// from an unflagged network (e.g. local dev). In production, images must be pre-fetched from
/// such a network and placed in <paramref name="cacheDirectory"/> ahead of time — see
/// scripts/cache-images.mjs — and are served from there instead.
/// </summary>
public class ArtworkImageService(IEpochaDbContext db, IExternalImageFetcher fetcher, string cacheDirectory)
{
    public Task<ExternalImage?> GetThumbnailAsync(int artworkId, CancellationToken cancellationToken) =>
        GetAsync(artworkId, "thumbnail", a => a.ThumbnailUrl, cancellationToken);

    public Task<ExternalImage?> GetImageAsync(int artworkId, CancellationToken cancellationToken) =>
        GetAsync(artworkId, "image", a => a.ImageUrl, cancellationToken);

    private async Task<ExternalImage?> GetAsync(
        int artworkId,
        string kind,
        System.Linq.Expressions.Expression<Func<Domain.Entities.Artwork, string?>> urlSelector,
        CancellationToken cancellationToken)
    {
        // The source is always a JPEG from the museum's IIIF endpoint (see ArticArtworkSource),
        // so the cache's content type never needs to be stored alongside the bytes.
        var cachedPath = Path.Combine(cacheDirectory, $"{artworkId}-{kind}.jpg");
        if (File.Exists(cachedPath))
        {
            var cachedBytes = await File.ReadAllBytesAsync(cachedPath, cancellationToken);
            return new ExternalImage(cachedBytes, "image/jpeg");
        }

        var url = await db.Artworks
            .Where(a => a.Id == artworkId)
            .Select(urlSelector)
            .FirstOrDefaultAsync(cancellationToken);

        return url is null ? null : await fetcher.FetchAsync(url, cancellationToken);
    }
}
