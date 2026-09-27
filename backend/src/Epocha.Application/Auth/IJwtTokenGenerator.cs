using Epocha.Domain.Entities;

namespace Epocha.Application.Auth;

/// <summary>Issues signed access tokens. Implemented in Infrastructure, which owns the signing key and library.</summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
}
