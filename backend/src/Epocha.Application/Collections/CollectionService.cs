using Epocha.Application.Abstractions;
using Epocha.Application.Search;
using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Collections;

/// <summary>
/// Every operation here scopes its query by (Id, UserId) together, rather than looking up by Id
/// and checking ownership afterward. A collection belonging to another user therefore behaves
/// identically to a nonexistent one (NotFound), which avoids leaking which collection ids exist.
/// </summary>
public class CollectionService(IEpochaDbContext db)
{
    public async Task<IReadOnlyList<CollectionSummary>> ListAsync(int userId, CancellationToken cancellationToken) =>
        await db.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CollectionSummary(c.Id, c.Name, c.Artworks.Count, c.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task<CollectionOutcome<CollectionSummary>> CreateAsync(int userId, string name, CancellationToken cancellationToken)
    {
        name = name.Trim();

        var exists = await db.Collections.AnyAsync(c => c.UserId == userId && c.Name == name, cancellationToken);
        if (exists)
        {
            return CollectionOutcome<CollectionSummary>.Failure(CollectionError.DuplicateName);
        }

        var collection = new Collection { UserId = userId, Name = name };
        db.Collections.Add(collection);
        await db.SaveChangesAsync(cancellationToken);

        return CollectionOutcome<CollectionSummary>.Success(new CollectionSummary(collection.Id, collection.Name, 0, collection.CreatedAt));
    }

    public async Task<CollectionOutcome<CollectionSummary>> RenameAsync(int userId, int collectionId, string name, CancellationToken cancellationToken)
    {
        name = name.Trim();

        var collection = await db.Collections
            .Include(c => c.Artworks)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId, cancellationToken);
        if (collection is null)
        {
            return CollectionOutcome<CollectionSummary>.Failure(CollectionError.NotFound);
        }

        var duplicate = await db.Collections.AnyAsync(c => c.UserId == userId && c.Name == name && c.Id != collectionId, cancellationToken);
        if (duplicate)
        {
            return CollectionOutcome<CollectionSummary>.Failure(CollectionError.DuplicateName);
        }

        collection.Name = name;
        await db.SaveChangesAsync(cancellationToken);

        return CollectionOutcome<CollectionSummary>.Success(new CollectionSummary(collection.Id, collection.Name, collection.Artworks.Count, collection.CreatedAt));
    }

    public async Task<CollectionError> DeleteAsync(int userId, int collectionId, CancellationToken cancellationToken)
    {
        var collection = await db.Collections.FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId, cancellationToken);
        if (collection is null)
        {
            return CollectionError.NotFound;
        }

        db.Collections.Remove(collection);
        await db.SaveChangesAsync(cancellationToken);
        return CollectionError.None;
    }

    public async Task<CollectionDetail?> GetDetailAsync(int userId, int collectionId, CancellationToken cancellationToken)
    {
        var collection = await db.Collections
            .AsNoTracking()
            .Include(c => c.Artworks).ThenInclude(a => a.Artist)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId, cancellationToken);

        if (collection is null)
        {
            return null;
        }

        var artworks = collection.Artworks
            .OrderBy(a => a.Title)
            .Select(a => new ArtworkSummary(
                a.Id, a.Title, a.Artist?.Name, a.DateDisplay, a.DateStartYear,
                a.Era.ToString(), a.MediumCategory.ToString(), a.ThumbnailUrl, a.IsPublicDomain))
            .ToList();

        return new CollectionDetail(collection.Id, collection.Name, collection.CreatedAt, artworks);
    }

    public async Task<CollectionError> AddArtworkAsync(int userId, int collectionId, int artworkId, CancellationToken cancellationToken)
    {
        var collection = await db.Collections
            .Include(c => c.Artworks)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId, cancellationToken);
        if (collection is null)
        {
            return CollectionError.NotFound;
        }

        if (collection.Artworks.Any(a => a.Id == artworkId))
        {
            return CollectionError.None;
        }

        var artwork = await db.Artworks.FirstOrDefaultAsync(a => a.Id == artworkId, cancellationToken);
        if (artwork is null)
        {
            return CollectionError.ArtworkNotFound;
        }

        collection.Artworks.Add(artwork);
        await db.SaveChangesAsync(cancellationToken);
        return CollectionError.None;
    }

    public async Task<CollectionError> RemoveArtworkAsync(int userId, int collectionId, int artworkId, CancellationToken cancellationToken)
    {
        var collection = await db.Collections
            .Include(c => c.Artworks)
            .FirstOrDefaultAsync(c => c.Id == collectionId && c.UserId == userId, cancellationToken);
        if (collection is null)
        {
            return CollectionError.NotFound;
        }

        var artwork = collection.Artworks.FirstOrDefault(a => a.Id == artworkId);
        if (artwork is not null)
        {
            collection.Artworks.Remove(artwork);
            await db.SaveChangesAsync(cancellationToken);
        }

        return CollectionError.None;
    }
}
