using Epocha.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Images;

/// <summary>
/// Serves artwork images through our own domain instead of the museum's. The Art Institute's
/// site blocks image requests whose Referer isn't its own, so a browser can never load its
/// image URLs directly from another site; fetching them server-side (Referer set by us) and
/// streaming the bytes back is the only way to display them elsewhere.
/// </summary>
public class ArtworkImageService(IEpochaDbContext db, IExternalImageFetcher fetcher)
{
    public Task<ExternalImage?> GetThumbnailAsync(int artworkId, CancellationToken cancellationToken) =>
        GetAsync(artworkId, a => a.ThumbnailUrl, cancellationToken);

    public Task<ExternalImage?> GetImageAsync(int artworkId, CancellationToken cancellationToken) =>
        GetAsync(artworkId, a => a.ImageUrl, cancellationToken);

    private async Task<ExternalImage?> GetAsync(
        int artworkId,
        System.Linq.Expressions.Expression<Func<Domain.Entities.Artwork, string?>> urlSelector,
        CancellationToken cancellationToken)
    {
        var url = await db.Artworks
            .Where(a => a.Id == artworkId)
            .Select(urlSelector)
            .FirstOrDefaultAsync(cancellationToken);

        return url is null ? null : await fetcher.FetchAsync(url, cancellationToken);
    }
}
