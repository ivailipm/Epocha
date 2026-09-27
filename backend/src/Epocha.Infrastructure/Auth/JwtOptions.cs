namespace Epocha.Infrastructure.Auth;

/// <summary>Bound from the "Jwt" section of configuration.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC signing key. Must be at least 32 characters (256 bits) for HS256.</summary>
    public required string Secret { get; set; }

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60 * 24 * 7;
}
