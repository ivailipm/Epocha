namespace Epocha.Domain.Common;

/// <summary>Base class for entities that track when they were created and last changed.</summary>
public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
