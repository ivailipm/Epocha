using System.Text.RegularExpressions;

namespace Epocha.Domain.Common;

/// <summary>
/// Turns a display name into a stable, URL-safe deduplication key:
/// "Post-Impressionism" and "post impressionism" both become "post-impressionism".
/// </summary>
public static partial class Slug
{
    public static string From(string value) =>
        NonAlphanumeric().Replace(value.Trim().ToLowerInvariant(), "-").Trim('-');

    // [GeneratedRegex] makes the compiler generate the regex code at build time, which
    // is faster than constructing a Regex at runtime. The method must be `partial`.
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
