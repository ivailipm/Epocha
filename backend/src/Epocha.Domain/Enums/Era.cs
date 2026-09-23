namespace Epocha.Domain.Enums;

/// <summary>
/// Coarse historical period derived from an artwork's start year (see <see cref="Common.EraCalculator"/>).
/// Stored rather than computed at query time so Postgres and Elasticsearch always agree.
/// The boundaries are conventional Western art-history ones.
/// </summary>
public enum Era
{
    Unknown = 0,
    Ancient = 1,        // .. 500
    Medieval = 2,       // 500 .. 1400
    Renaissance = 3,    // 1400 .. 1600
    Baroque = 4,        // 1600 .. 1750
    Neoclassical = 5,   // 1750 .. 1830
    Romantic = 6,       // 1830 .. 1860
    Modern = 7,         // 1860 .. 1945
    Contemporary = 8    // 1945 ..
}
