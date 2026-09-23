namespace Epocha.Application.Search;

/// <summary>Writes artworks into the search index. Implemented in Infrastructure with Elasticsearch.</summary>
public interface IArtworkSearchIndexer
{
    /// <summary>Creates the index with its field mappings if it does not exist.</summary>
    Task EnsureIndexAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Indexes the documents in one bulk request and returns the ids that were written. A bulk
    /// request can partly succeed, so callers must only mark the returned ids as indexed.
    /// </summary>
    Task<IReadOnlyCollection<int>> IndexAsync(
        IReadOnlyCollection<ArtworkSearchDocument> documents,
        CancellationToken cancellationToken);
}
