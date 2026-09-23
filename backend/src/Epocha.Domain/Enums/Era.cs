namespace Epocha.Domain.Enums;

/// <summary>
/// Coarse historical period, derived from an artwork's start year during ingestion.
/// </summary>
/// <remarks>
/// Era is stored rather than computed at query time so that Postgres and Elasticsearch
/// always agree on it, and so the search index can facet on it without recomputation.
/// The boundaries are deliberately conventional Western-art-history ones; see
/// <see cref="EraCalculator"/> for the mapping and its limitations.
/// </remarks>
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
