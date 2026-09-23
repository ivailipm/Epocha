namespace Epocha.Domain.Common;

/// <summary>A movement as it is stored: a curated display name and its deduplication slug.</summary>
public record CatalogMovement(string Name, string Slug);

/// <summary>
/// The curated list of art movements the Movement filter offers. Museum "style" data mixes real
/// movements with centuries, cultures, dynasties and decorative-arts periods, so raw values are
/// only accepted if they match an entry here (by canonical name or alias). Variants such as
/// "Analytical Cubism" fold into one canonical movement. Extend the list to widen the filter.
/// </summary>
public static class MovementCatalog
{
    private static readonly (string Name, string[] Aliases)[] Entries =
    [
        ("Renaissance", ["Early Renaissance", "High Renaissance", "Northern Renaissance", "Italian Renaissance"]),
        ("Mannerism", []),
        ("Baroque", []),
        ("Rococo", []),
        ("Neoclassicism", ["Neoclassical", "Neo-Classicism"]),
        ("Romanticism", ["Romantic"]),
        ("Realism", []),
        ("Impressionism", []),
        ("Post-Impressionism", ["Postimpressionism"]),
        ("Symbolism", []),
        ("Art Nouveau", []),
        ("Arts and Crafts", ["Arts and Crafts Movement"]),
        ("Aesthetic Movement", []),
        ("Fauvism", []),
        ("Expressionism", ["German Expressionism"]),
        ("Abstract Expressionism", []),
        ("Cubism", ["Analytical Cubism", "Synthetic Cubism", "Synthetic Cubist", "Confetti Cubism", "Cubist"]),
        ("Futurism", []),
        ("Dada", ["Dadaism"]),
        ("Surrealism", ["Surrealism in Exile"]),
        ("Art Deco", []),
        ("Bauhaus", []),
        ("De Stijl", []),
        ("Constructivism", []),
        ("Pop Art", []),
        ("Minimalism", []),
        ("Conceptual Art", []),
        ("Modernism", []),
        ("Avant-Garde", ["Avant Garde"]),
        ("Biedermeier", []),
        ("Byzantine", ["Early Byzantine", "Late Byzantine"]),
        ("Gothic", ["Gothic (Medieval)"]),
        ("Hellenistic", []),
        ("Ukiyo-e", ["Ukiyoe"]),
    ];

    private static readonly Dictionary<string, CatalogMovement> BySlug = Build();

    /// <summary>Returns the canonical movement for a raw museum value, or null if it isn't a recognised movement.</summary>
    public static CatalogMovement? Normalize(string raw)
    {
        var slug = Slug.From(raw);
        return slug.Length > 0 && BySlug.TryGetValue(slug, out var movement) ? movement : null;
    }

    private static Dictionary<string, CatalogMovement> Build()
    {
        var map = new Dictionary<string, CatalogMovement>();

        foreach (var (name, aliases) in Entries)
        {
            var movement = new CatalogMovement(name, Slug.From(name));

            foreach (var key in aliases.Prepend(name))
            {
                map[Slug.From(key)] = movement;
            }
        }

        return map;
    }
}
