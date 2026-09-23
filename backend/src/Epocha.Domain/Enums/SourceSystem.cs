namespace Epocha.Domain.Enums;

/// <summary>
/// Which museum API a record originated from. Stored alongside the museum's own
/// identifier so the pair (SourceSystem, SourceExternalId) uniquely identifies an
/// upstream record — the Met and the Art Institute both number their objects from 1.
/// </summary>
public enum SourceSystem
{
    ArtInstituteOfChicago = 1,
    MetropolitanMuseumOfArt = 2
}
