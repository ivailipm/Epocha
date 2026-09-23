namespace Epocha.Domain.Enums;

/// <summary>
/// Normalised bucket for the museum's free-text medium description.
/// </summary>
/// <remarks>
/// Museums record medium as prose ("Oil on canvas", "Gelatin silver print",
/// "Bronze with green patina"). That is great for display and useless for a filter
/// dropdown, so ingestion keeps the original string on
/// <see cref="Entities.Artwork.MediumDisplay"/> and additionally classifies it into
/// one of these buckets for faceting.
/// </remarks>
public enum MediumCategory
{
    Unknown = 0,
    Painting = 1,
    Drawing = 2,
    Print = 3,
    Photograph = 4,
    Sculpture = 5,
    Textile = 6,
    Ceramic = 7,
    Metalwork = 8,
    Furniture = 9,
    Mixed = 10
}
