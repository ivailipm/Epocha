using System.Text.RegularExpressions;

namespace Epocha.Domain.Common;

/// <summary>Turns a display name into a URL-safe deduplication key: "Post-Impressionism" becomes "post-impressionism".</summary>
public static partial class Slug
{
    public static string From(string value) =>
        NonAlphanumeric().Replace(value.Trim().ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
