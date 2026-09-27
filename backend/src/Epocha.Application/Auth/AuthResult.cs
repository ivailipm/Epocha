namespace Epocha.Application.Auth;

/// <summary>What a client gets back after registering or logging in. Never includes the password hash.</summary>
public record AuthResult(string Token, DateTimeOffset ExpiresAt, int UserId, string Email, string DisplayName);
