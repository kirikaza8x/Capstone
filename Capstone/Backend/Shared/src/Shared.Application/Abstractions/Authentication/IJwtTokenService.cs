using System.Security.Claims;

namespace Shared.Application.Abstractions.Authentication;

/// <summary>
/// Defines operations for generating and validating JWT access tokens and refresh tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="userId">The unique ID of the user.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="name">The user's display name.</param>
    /// <param name="roles">A list of roles assigned to the user.</param>
    /// <returns>A signed JWT string containing user claims.</returns>
    string GenerateToken(Guid userId, string? email, string? name, IEnumerable<string> roles);

    /// <summary>
    /// Validates a JWT access token and returns its claims if valid.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="allowExpired">If true, skips lifetime validation (used during refresh).</param>
    /// <returns>The token's ClaimsPrincipal if valid; otherwise null.</returns>
    ClaimsPrincipal? ValidateToken(string token, bool allowExpired = false);

    /// <summary>
    /// Generates a secure, random refresh token.
    /// Format: base64 string (not a JWT).
    /// </summary>
    /// <returns>A random base64-encoded refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Checks whether a JWT token is expired based on its 'exp' claim.
    /// </summary>
    /// <param name="token">The JWT token to inspect.</param>
    /// <returns>True if expired or invalid; false if still valid.</returns>
    bool IsTokenExpired(string token);

    /// <summary>
    /// Gets the number of minutes until the token expires.
    /// Returns a negative value if the token is already expired.
    /// </summary>
    /// <param name="token">The JWT token to inspect.</param>
    /// <returns>Minutes until expiry (negative if expired).</returns>
    int GetMinutesUntilExpiry(string token);
    
    
    int ExpiryMinutes { get; }

    int RefreshTokenExpiryDays { get; }
}
