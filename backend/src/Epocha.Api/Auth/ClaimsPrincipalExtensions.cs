using System.Security.Claims;

namespace Epocha.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The current user's id. Safe to assume present on any [Authorize]-protected action.</summary>
    public static int GetUserId(this ClaimsPrincipal principal) =>
        int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
