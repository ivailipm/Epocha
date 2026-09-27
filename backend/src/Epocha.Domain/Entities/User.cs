using Epocha.Domain.Common;

namespace Epocha.Domain.Entities;

public class User : AuditableEntity
{
    public int Id { get; set; }

    public required string Email { get; set; }

    /// <summary>A PBKDF2 hash (via <c>PasswordHasher&lt;User&gt;</c>), never the plain password.</summary>
    public required string PasswordHash { get; set; }

    public required string DisplayName { get; set; }

    public ICollection<Collection> Collections { get; set; } = [];
}
