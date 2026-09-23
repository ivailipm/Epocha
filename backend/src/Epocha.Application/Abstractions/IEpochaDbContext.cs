using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Abstractions;

/// <summary>
/// The slice of the database that Application code is allowed to use. The concrete
/// <c>EpochaDbContext</c> lives in Infrastructure and implements this, so Application
/// services can query and save entities without referencing Infrastructure (which
/// would invert the dependency direction). It also means tests can substitute a
/// different implementation.
/// </summary>
public interface IEpochaDbContext
{
    DbSet<Artwork> Artworks { get; }
    DbSet<Artist> Artists { get; }
    DbSet<Movement> Movements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
