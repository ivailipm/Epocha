using Epocha.Application.Search;

namespace Epocha.Application.Collections;

public record CollectionSummary(int Id, string Name, int ArtworkCount, DateTimeOffset CreatedAt);

/// <remarks>Reuses <see cref="ArtworkSummary"/> so a collection's artworks render with the same ArtworkCard as the gallery.</remarks>
public record CollectionDetail(int Id, string Name, DateTimeOffset CreatedAt, IReadOnlyList<ArtworkSummary> Artworks);

public enum CollectionError
{
    None,
    NotFound,
    DuplicateName,
    ArtworkNotFound
}

public record CollectionOutcome<T>(T? Result, CollectionError Error)
{
    public static CollectionOutcome<T> Success(T result) => new(result, CollectionError.None);
    public static CollectionOutcome<T> Failure(CollectionError error) => new(default, error);
}
