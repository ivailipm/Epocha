using Epocha.Domain.Enums;

namespace Epocha.Domain.Common;

/// <summary>
/// Maps a year to an <see cref="Era"/>. Pure domain logic: no database, no HTTP,
/// no framework types — which is exactly why it lives in the Domain project and is
/// trivial to unit test.
/// </summary>
public static class EraCalculator
{
    public static Era FromYear(int? year) => year switch
    {
        null => Era.Unknown,
        < 500 => Era.Ancient,
        < 1400 => Era.Medieval,
        < 1600 => Era.Renaissance,
        < 1750 => Era.Baroque,
        < 1830 => Era.Neoclassical,
        < 1860 => Era.Romantic,
        < 1945 => Era.Modern,
        _ => Era.Contemporary
    };
}
