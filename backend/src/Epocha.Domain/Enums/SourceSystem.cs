namespace Epocha.Domain.Enums;

/// <summary>
/// Which museum API a record came from. Paired with the museum's own id to identify an
/// upstream record, since both museums number their objects from 1.
/// </summary>
public enum SourceSystem
{
    ArtInstituteOfChicago = 1,
    MetropolitanMuseumOfArt = 2
}
