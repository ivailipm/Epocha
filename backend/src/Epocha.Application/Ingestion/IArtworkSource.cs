namespace Epocha.Application.Ingestion;

/// <summary>A museum API that can return pages of artworks. Implemented in Infrastructure, one per museum.</summary>
public interface IArtworkSource
{
    Task<ArtworkPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
}
