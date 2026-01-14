namespace Shared.Application.Abstractions.Authentication
{
    /// <summary>
    /// Defines operations for generating and managing refresh tokens.
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Generates a new secure refresh token entity for a user.
        /// </summary>
        RefreshToken GenerateToken(Guid userId);

        /// <summary>
        /// Validates a refresh token (expiry + revoked status).
        /// </summary>
        bool ValidateToken(RefreshToken token);

        /// <summary>
        /// Revokes a refresh token so it can no longer be used.
        /// </summary>
        void RevokeToken(RefreshToken token);

        /// <summary>
        /// Configured lifetime of refresh tokens in days.
        /// </summary>
        int RefreshTokenExpiryDays { get; }
    }
}
