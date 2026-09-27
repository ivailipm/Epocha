using System.ComponentModel.DataAnnotations;

namespace Epocha.Api.Contracts;

public class RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(8)]
    public required string Password { get; init; }

    [Required, MinLength(1), MaxLength(128)]
    public required string DisplayName { get; init; }
}

public class LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
