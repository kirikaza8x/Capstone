using System.Security.Claims;

namespace Shared.Application.Abstractions.Authentication
{
    /// <summary>
    /// Defines operations for generating and validating JWT access tokens.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a signed JWT access token for the given user.
        /// </summary>
        string GenerateToken( 
            Guid userId, 
            string? email, 
            string? name, 
            IEnumerable<string> roles
            );

        /// <summary>
        /// Validates a JWT access token and returns its claims if valid.
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token, bool allowExpired = false);

        /// <summary>
        /// Checks whether a JWT token is expired based on its 'exp' claim.
        /// </summary>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Gets the number of minutes until the token expires.
        /// Returns a negative value if the token is already expired.
        /// </summary>
        int GetMinutesUntilExpiry(string token);

        /// <summary>
        /// Configured lifetime of JWT access tokens in minutes.
        /// </summary>
        int ExpiryMinutes { get; }

        /// <summary>
        /// Configured lifetime of refresh tokens in days.
        /// </summary>
        int RefreshTokenExpiryDays { get; }

        /// <summary>
        /// Generates a secure random refresh token string.
        /// </summary>
        string GenerateRefreshToken();
    }
}
