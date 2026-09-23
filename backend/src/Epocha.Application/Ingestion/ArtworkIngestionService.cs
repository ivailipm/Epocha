using Epocha.Application.Abstractions;
using Epocha.Application.Search;
using Epocha.Domain.Common;
using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epocha.Application.Ingestion;

/// <param name="Created">Rows inserted.</param>
/// <param name="Matched">Rows that already existed; EF only writes them if a value changed.</param>
/// <param name="Indexed">Artworks written to Elasticsearch in this call.</param>
public record UpsertResult(int Created, int Matched, int Indexed);

/// <summary>
/// Upserts a page of <see cref="ArtworkRecord"/>s into Postgres, then indexes them into
/// Elasticsearch. Rows are matched on (SourceSystem, SourceExternalId), the same pair that has
/// a unique index, so re-running ingestion never creates duplicates.
/// </summary>
public class ArtworkIngestionService(
    IEpochaDbContext db,
    IArtworkSearchIndexer indexer,
    ILogger<ArtworkIngestionService> logger)
{
    public async Task<UpsertResult> UpsertAsync(IReadOnlyList<ArtworkRecord> records, CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return new UpsertResult(0, 0, 0);
        }

        // A page comes from a single museum.
        var source = records[0].SourceSystem;

        // Load everything the page touches in a few queries, work in memory, save once.
        // Query count stays constant regardless of page size (no N+1).

        var artistIds = records
            .Where(r => r.Artist is not null)
            .Select(r => r.Artist!.ExternalId)
            .Distinct()
            .ToList();

        var artists = await db.Artists
            .Where(a => a.SourceSystem == source && artistIds.Contains(a.SourceExternalId))
            .ToDictionaryAsync(a => a.SourceExternalId, cancellationToken);

        foreach (var artistRecord in records.Where(r => r.Artist is not null).Select(r => r.Artist!))
        {
            if (artists.TryGetValue(artistRecord.ExternalId, out var existingArtist))
            {
                existingArtist.Name = Truncate(artistRecord.Name, 256)!;
            }
            else
            {
                var artist = new Artist
                {
                    SourceSystem = source,
                    SourceExternalId = artistRecord.ExternalId,
                    Name = Truncate(artistRecord.Name, 256)!
                };
                db.Artists.Add(artist);
                artists[artistRecord.ExternalId] = artist;
            }
        }

        // Only movements recognised by the catalog are kept; the museum's style data also
        // contains centuries, cultures and dynasties that don't belong in a movement filter.
        var catalogMovements = records
            .SelectMany(r => r.Movements)
            .Select(MovementCatalog.Normalize)
            .OfType<CatalogMovement>()
            .DistinctBy(m => m.Slug)
            .ToList();

        var slugs = catalogMovements.Select(m => m.Slug).ToList();
        var movements = await db.Movements
            .Where(m => slugs.Contains(m.Slug))
            .ToDictionaryAsync(m => m.Slug, cancellationToken);

        foreach (var catalogMovement in catalogMovements)
        {
            if (!movements.ContainsKey(catalogMovement.Slug))
            {
                var movement = new Movement { Name = catalogMovement.Name, Slug = catalogMovement.Slug };
                db.Movements.Add(movement);
                movements[catalogMovement.Slug] = movement;
            }
        }

        var externalIds = records.Select(r => r.ExternalId).Distinct().ToList();

        var artworks = await db.Artworks
            .Include(a => a.Movements)
            .Where(a => a.SourceSystem == source && externalIds.Contains(a.SourceExternalId))
            .ToDictionaryAsync(a => a.SourceExternalId, cancellationToken);

        var created = 0;
        var matched = 0;

        foreach (var record in records)
        {
            if (artworks.TryGetValue(record.ExternalId, out var artwork))
            {
                matched++;
            }
            else
            {
                artwork = new Artwork
                {
                    SourceSystem = record.SourceSystem,
                    SourceExternalId = record.ExternalId,
                    Title = record.Title
                };
                db.Artworks.Add(artwork);
                // Tracked here too, so a duplicate id later in the same page updates this
                // instance instead of violating the unique index.
                artworks[record.ExternalId] = artwork;
                created++;
            }

            Apply(record, artwork, artists, movements);
        }

        // One SaveChanges is one transaction: the page is written atomically.
        await db.SaveChangesAsync(cancellationToken);

        // Postgres is committed by now and stays the source of truth even if indexing fails.
        var indexed = await IndexPendingAsync(artworks.Values, cancellationToken);

        logger.LogInformation(
            "Upserted page: {Created} new, {Matched} already existed, {Indexed} indexed",
            created, matched, indexed);
        return new UpsertResult(created, matched, indexed);
    }

    /// <summary>
    /// Brings movements already in Postgres in line with <see cref="MovementCatalog"/>: renames
    /// them to their canonical form, merges duplicates and variants into one row, and deletes
    /// anything that isn't a recognised movement. Affected artworks are marked updated so the
    /// indexer refreshes them. Idempotent; a no-op when everything already matches.
    /// </summary>
    /// <returns>The number of movements that were renamed, merged or removed.</returns>
    public async Task<int> NormalizeExistingMovementsAsync(CancellationToken cancellationToken)
    {
        var movements = await db.Movements.ToListAsync(cancellationToken);
        var bySlug = movements.ToDictionary(m => m.Slug);
        var changed = 0;

        foreach (var movement in movements)
        {
            var canonical = MovementCatalog.Normalize(movement.Name);

            if (canonical is not null && canonical.Slug == movement.Slug && canonical.Name == movement.Name)
            {
                continue;
            }

            var artworks = await db.Artworks
                .Include(a => a.Movements)
                .Where(a => a.Movements.Any(m => m.Id == movement.Id))
                .ToListAsync(cancellationToken);

            Movement? target = null;
            var renameInPlace = canonical is not null
                && (!bySlug.TryGetValue(canonical.Slug, out target) || ReferenceEquals(target, movement));

            if (renameInPlace)
            {
                bySlug.Remove(movement.Slug);
                movement.Name = canonical!.Name;
                movement.Slug = canonical.Slug;
                bySlug[movement.Slug] = movement;
            }
            else
            {
                // Merge into the canonical row, or drop the movement when it isn't in the catalog.
                foreach (var artwork in artworks)
                {
                    artwork.Movements.Remove(movement);
                    if (target is not null && !artwork.Movements.Contains(target))
                    {
                        artwork.Movements.Add(target);
                    }
                }

                db.Movements.Remove(movement);
                bySlug.Remove(movement.Slug);
            }

            // Touch every affected artwork so the backlog pass re-indexes it.
            var now = DateTimeOffset.UtcNow;
            foreach (var artwork in artworks)
            {
                artwork.UpdatedAt = now;
            }

            changed++;
        }

        if (changed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Normalised {Count} movements against the catalog", changed);
        }

        return changed;
    }

    /// <summary>
    /// Indexes every artwork in Postgres that is missing from or stale in Elasticsearch,
    /// whether or not the current run fetched it. Keeps the two stores converging after an
    /// Elasticsearch outage or when the museum's listing order shifts between runs.
    /// </summary>
    public async Task<int> IndexBacklogAsync(int batchSize, CancellationToken cancellationToken)
    {
        var total = 0;

        while (true)
        {
            var batch = await db.Artworks
                .AsNoTracking()
                .Include(a => a.Artist)
                .Include(a => a.Movements)
                .Where(a => a.LastIndexedAt == null || a.UpdatedAt > a.LastIndexedAt)
                .OrderBy(a => a.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                return total;
            }

            var indexed = await IndexPendingAsync(batch, cancellationToken);
            total += indexed;

            // Nothing succeeded (e.g. Elasticsearch is down): stop rather than loop forever.
            if (indexed == 0)
            {
                return total;
            }
        }
    }

    /// <summary>
    /// Indexes artworks that are new or changed since they were last indexed and records
    /// LastIndexedAt for those that succeeded. Failures are logged, not thrown; the rows stay
    /// pending and are retried on the next run.
    /// </summary>
    private async Task<int> IndexPendingAsync(IEnumerable<Artwork> artworks, CancellationToken cancellationToken)
    {
        var pending = artworks
            .Where(a => a.LastIndexedAt is null || a.UpdatedAt > a.LastIndexedAt)
            .ToList();

        if (pending.Count == 0)
        {
            return 0;
        }

        try
        {
            var documents = pending.Select(ArtworkSearchDocument.From).ToList();
            var indexedIds = (await indexer.IndexAsync(documents, cancellationToken)).ToList();

            if (indexedIds.Count == 0)
            {
                return 0;
            }

            // ExecuteUpdate bypasses SaveChanges on purpose: SaveChanges would also bump
            // UpdatedAt, making every artwork look changed since it was indexed.
            var now = DateTimeOffset.UtcNow;
            await db.Artworks
                .Where(a => indexedIds.Contains(a.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.LastIndexedAt, now), cancellationToken);

            return indexedIds.Count;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Elasticsearch indexing failed for {Count} artworks; they will be retried next run", pending.Count);
            return 0;
        }
    }

    private static void Apply(
        ArtworkRecord record,
        Artwork artwork,
        Dictionary<string, Artist> artists,
        Dictionary<string, Movement> movements)
    {
        artwork.Title = Truncate(record.Title, 512)!;
        artwork.SourceUrl = record.SourceUrl;
        artwork.Description = record.Description;
        artwork.MediumDisplay = Truncate(record.MediumDisplay, 512);
        artwork.Department = Truncate(record.Department, 256);
        artwork.Dimensions = Truncate(record.Dimensions, 256);
        artwork.CreditLine = record.CreditLine;
        artwork.DateDisplay = Truncate(record.DateDisplay, 128);
        artwork.DateStartYear = record.DateStartYear;
        artwork.DateEndYear = record.DateEndYear;
        artwork.ImageUrl = record.ImageUrl;
        artwork.ThumbnailUrl = record.ThumbnailUrl;
        artwork.IsPublicDomain = record.IsPublicDomain;

        artwork.MediumCategory = MediumClassifier.Classify(record.MediumDisplay, record.Classification);
        artwork.Era = EraCalculator.FromYear(record.DateStartYear);

        if (record.Artist is null)
        {
            artwork.Artist = null;
            artwork.ArtistId = null;
        }
        else
        {
            artwork.Artist = artists[record.Artist.ExternalId];
        }

        // Only rewrite the join rows when the set of movements actually changed.
        var wanted = record.Movements
            .Select(MovementCatalog.Normalize)
            .OfType<CatalogMovement>()
            .Select(m => m.Slug)
            .Distinct()
            .ToList();
        var current = artwork.Movements.Select(m => m.Slug).ToHashSet();

        if (!current.SetEquals(wanted))
        {
            // Join-row changes don't mark the artwork itself modified; bump UpdatedAt so the
            // indexer sees it as stale.
            artwork.UpdatedAt = DateTimeOffset.UtcNow;
            artwork.Movements.Clear();
            foreach (var slug in wanted)
            {
                artwork.Movements.Add(movements[slug]);
            }
        }
    }

    // Museum data is messy; truncating to the column length keeps one long string from failing a whole page.
    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
