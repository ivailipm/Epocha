using Epocha.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Details;

/// <summary>
/// Reads a single artwork straight from Postgres, unlike search which reads from Elasticsearch.
/// Postgres is the source of truth, so the detail page always shows the current row even if
/// Elasticsearch indexing is lagging behind.
/// </summary>
public class ArtworkDetailService(IEpochaDbContext db)
{
    public async Task<ArtworkDetail?> GetAsync(int id, CancellationToken cancellationToken)
    {
        // AsNoTracking: this is a read-only request, so there is nothing to save afterwards
        // and no need for EF Core to track the loaded entities for changes.
        return await db.Artworks
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new ArtworkDetail(
                a.Id,
                a.Title,
                a.Description,
                a.MediumDisplay,
                a.MediumCategory.ToString(),
                a.Department,
                a.Dimensions,
                a.CreditLine,
                a.DateDisplay,
                a.DateStartYear,
                a.DateEndYear,
                a.Era.ToString(),
                a.ImageUrl,
                a.ThumbnailUrl,
                a.IsPublicDomain,
                a.SourceSystem.ToString(),
                a.SourceUrl,
                a.Artist == null ? null : new ArtistSummary(a.Artist.Id, a.Artist.Name, a.Artist.Nationality, a.Artist.BirthYear, a.Artist.DeathYear),
                a.Movements.Select(m => m.Name).OrderBy(n => n).ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
