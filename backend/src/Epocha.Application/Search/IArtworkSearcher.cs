namespace Epocha.Application.Search;

/// <summary>The read side of search. Implemented in Infrastructure with Elasticsearch.</summary>
public interface IArtworkSearcher
{
    Task<ArtworkSearchResult> SearchAsync(ArtworkSearchQuery query, CancellationToken cancellationToken);
}
