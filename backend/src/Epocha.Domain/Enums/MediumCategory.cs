namespace Epocha.Domain.Enums;

/// <summary>
/// Normalised bucket for a museum's free-text medium, used for the Medium filter. The
/// original text is kept on <see cref="Entities.Artwork.MediumDisplay"/>.
/// </summary>
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
