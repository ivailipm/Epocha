using System.Globalization;
using Epocha.Application.Abstractions;
using Epocha.Domain.Common;
using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epocha.Application.Ingestion;

/// <param name="Created">Rows that did not exist before and were inserted.</param>
/// <param name="Matched">Rows that already existed; EF only writes them if a value actually changed.</param>
public record UpsertResult(int Created, int Matched);

/// <summary>
/// Takes one page of museum-neutral <see cref="ArtworkRecord"/>s and writes them to
/// Postgres as an "upsert": insert what's new, update what already exists. That's what
/// makes the ingestion job idempotent — running it twice does not create duplicates,
/// because rows are matched on (SourceSystem, SourceExternalId), the same pair that has
/// a unique index in the database.
/// </summary>
/// <remarks>
/// The constructor takes its dependencies as parameters ("primary constructor"): the DI
/// container sees the parameter types and supplies matching services automatically.
/// This is constructor injection, the standard way .NET code gets its collaborators.
/// </remarks>
public class ArtworkIngestionService(IEpochaDbContext db, ILogger<ArtworkIngestionService> logger)
{
    public async Task<UpsertResult> UpsertAsync(IReadOnlyList<ArtworkRecord> records, CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return new UpsertResult(0, 0);
        }

        // A page comes from a single museum, so one source value covers the whole batch.
        var source = records[0].SourceSystem;

        // Strategy: load everything this page touches in a handful of queries (rather
        // than one query per artwork), build in-memory dictionaries, then do all the
        // inserts/updates and save ONCE. Query count per page stays constant no matter
        // how many artworks the page holds — avoiding the classic "N+1 queries" problem.

        // ---- Artists --------------------------------------------------------
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

        // ---- Movements ------------------------------------------------------
        // Deduplicated by slug, so "Impressionism" and "impressionism" are one row.
        var movementNamesBySlug = records
            .SelectMany(r => r.Movements)
            .GroupBy(Slug.From)
            .Where(g => g.Key.Length > 0)
            .ToDictionary(g => g.Key, g => g.First());

        var slugs = movementNamesBySlug.Keys.ToList();
        var movements = await db.Movements
            .Where(m => slugs.Contains(m.Slug))
            .ToDictionaryAsync(m => m.Slug, cancellationToken);

        foreach (var (slug, name) in movementNamesBySlug)
        {
            if (!movements.ContainsKey(slug))
            {
                var movement = new Movement { Name = Truncate(ToTitleCase(name), 128)!, Slug = slug };
                db.Movements.Add(movement);
                movements[slug] = movement;
            }
        }

        // ---- Artworks -------------------------------------------------------
        var externalIds = records.Select(r => r.ExternalId).Distinct().ToList();

        // Include() tells EF to also load each artwork's Movements in the same query;
        // lazy loading is off, so related data is only present if you ask for it.
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
                // Also track it in the dictionary so a duplicate id later in the same
                // page updates this instance instead of violating the unique index.
                artworks[record.ExternalId] = artwork;
                created++;
            }

            Apply(record, artwork, artists, movements);
        }

        // One SaveChanges = one database transaction: the whole page is written
        // atomically, or (on error) not at all.
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Upserted page: {Created} new, {Matched} already existed", created, matched);
        return new UpsertResult(created, matched);
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

        // Derived fields: computed here, once, and stored. Postgres and (later)
        // Elasticsearch therefore always agree on them.
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

        // Only touch the many-to-many collection if the set of movements really
        // changed, otherwise every re-run would delete and re-insert unchanged join rows.
        var wanted = record.Movements.Select(Slug.From).Where(s => s.Length > 0).Distinct().ToList();
        var current = artwork.Movements.Select(m => m.Slug).ToHashSet();

        if (!current.SetEquals(wanted))
        {
            artwork.Movements.Clear();
            foreach (var slug in wanted)
            {
                artwork.Movements.Add(movements[slug]);
            }
        }
    }

    private static string ToTitleCase(string value) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());

    // Museum data is messy. Truncating to the column's max length means one unusually
    // long string can't make the whole page's INSERT fail.
    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
