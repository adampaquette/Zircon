using System.Security.Claims;

namespace Zircon.Security.Claims.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal authentication checks.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Checks if the current principal is authenticated.
    /// </summary>
    /// <param name="principal">The claims principal to check.</param>
    /// <returns>
    /// <c>true</c> if the principal is authenticated otherwise, <c>false</c>.
    /// </returns>
    public static bool IsAuthenticated(this ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated ?? false;
    }
}
