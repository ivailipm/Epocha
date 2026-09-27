using Epocha.Application.Abstractions;
using Epocha.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Epocha.Application.Auth;

public enum AuthError
{
    None,
    EmailAlreadyRegistered,
    InvalidCredentials
}

public record AuthOutcome(AuthResult? Result, AuthError Error)
{
    public static AuthOutcome Success(AuthResult result) => new(result, AuthError.None);
    public static AuthOutcome Failure(AuthError error) => new(null, error);
}

/// <summary>
/// Registration and login. Password hashing uses ASP.NET Core Identity's PasswordHasher on its
/// own, without the rest of Identity (UserManager, IdentityDbContext, ...), which would otherwise
/// pull its own schema and conventions into a codebase that already has its own User entity.
/// </summary>
public class AuthService(IEpochaDbContext db, IJwtTokenGenerator tokenGenerator)
{
    private readonly PasswordHasher<User> hasher = new();

    public async Task<AuthOutcome> RegisterAsync(string email, string password, string displayName, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return AuthOutcome.Failure(AuthError.EmailAlreadyRegistered);
        }

        var user = new User
        {
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            PasswordHash = string.Empty
        };
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return AuthOutcome.Success(IssueToken(user));
    }

    public async Task<AuthOutcome> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return AuthOutcome.Failure(AuthError.InvalidCredentials);
        }

        // PasswordVerificationResult.Rehash means the hash used an older algorithm/cost and
        // should be upgraded; treating it as a successful verification and re-hashing is the
        // standard pattern, but out of scope here — Failed is the only case that actually blocks login.
        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return AuthOutcome.Failure(AuthError.InvalidCredentials);
        }

        return AuthOutcome.Success(IssueToken(user));
    }

    private AuthResult IssueToken(User user)
    {
        var (token, expiresAt) = tokenGenerator.GenerateToken(user);
        return new AuthResult(token, expiresAt, user.Id, user.Email, user.DisplayName);
    }
}
