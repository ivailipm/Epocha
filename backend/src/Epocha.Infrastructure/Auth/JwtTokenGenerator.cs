using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Epocha.Application.Auth;
using Epocha.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Epocha.Infrastructure.Auth;

internal sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
    {
        var settings = options.Value;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(settings.ExpiryMinutes);

        // Standard short claim names keep the token small. JwtBearer's default inbound claim
        // map turns "sub"/"email"/"name" into ClaimTypes.NameIdentifier/Email/Name on the
        // receiving end, so reading them elsewhere still uses the familiar ClaimTypes constants.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
