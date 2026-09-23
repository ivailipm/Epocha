using Epocha.Domain.Enums;

namespace Epocha.Domain.Common;

/// <summary>
/// Buckets a museum's free-text medium ("Oil on canvas") into a <see cref="MediumCategory"/>.
/// A keyword heuristic: rule order matters (more specific first, so "printed cotton" is
/// Textile, not Print), and anything unmatched is Unknown rather than guessed.
/// </summary>
public static class MediumClassifier
{
    private static readonly (MediumCategory Category, string[] Keywords)[] Rules =
    [
        (MediumCategory.Photograph, ["photograph", "gelatin silver", "daguerreotype", "albumen", "cyanotype", "chromogenic"]),
        (MediumCategory.Textile, ["textile", "silk", "wool", "cotton", "linen", "embroider", "tapestry", "velvet"]),
        (MediumCategory.Ceramic, ["ceramic", "porcelain", "stoneware", "earthenware", "faience", "terracotta", "glazed"]),
        (MediumCategory.Furniture, ["chair", "cabinet", "desk", "sofa", "commode", "chest of drawers"]),
        (MediumCategory.Print, ["woodcut", "etching", "engraving", "lithograph", "screenprint", "screen print", "aquatint", "mezzotint", "print"]),
        (MediumCategory.Painting, ["oil on", "acrylic", "tempera", "fresco", "watercolor", "gouache", "encaustic", "painting"]),
        (MediumCategory.Drawing, ["graphite", "charcoal", "chalk", "pastel", "pencil", "pen and", "ink on", "drawing"]),
        (MediumCategory.Sculpture, ["sculpture", "marble", "bronze", "limestone", "sandstone", "carved", "statue"]),
        (MediumCategory.Metalwork, ["silver", "gold", "brass", "iron", "copper", "pewter", "enamel"]),
    ];

    public static MediumCategory Classify(string? mediumDisplay, string? classification = null)
    {
        var text = $"{classification} {mediumDisplay}".ToLowerInvariant();

        foreach (var (category, keywords) in Rules)
        {
            if (keywords.Any(text.Contains))
            {
                return category;
            }
        }

        return MediumCategory.Unknown;
    }
}
