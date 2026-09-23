namespace Epocha.Application.Ingestion;

/// <summary>
/// "Something that can hand me pages of artworks." Application defines WHAT it needs;
/// Infrastructure supplies HOW (an HTTP client for a specific museum's API). This is
/// the dependency inversion at the heart of clean architecture: the arrow of
/// dependency points inward, so Application never references HttpClient or any museum.
/// Adding the Met later means writing one more implementation of this interface.
/// </summary>
public interface IArtworkSource
{
    Task<ArtworkPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
}
