using Epocha.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Abstractions;

/// <summary>
/// The part of the database that Application code may use. Implemented by EpochaDbContext in
/// Infrastructure, so Application does not depend on Infrastructure.
/// </summary>
public interface IEpochaDbContext
{
    DbSet<Artwork> Artworks { get; }
    DbSet<Artist> Artists { get; }
    DbSet<Movement> Movements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
