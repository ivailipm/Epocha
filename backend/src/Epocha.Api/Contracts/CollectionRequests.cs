using System.ComponentModel.DataAnnotations;

namespace Epocha.Api.Contracts;

public class SaveCollectionRequest
{
    [Required, MinLength(1), MaxLength(128)]
    public required string Name { get; init; }
}
