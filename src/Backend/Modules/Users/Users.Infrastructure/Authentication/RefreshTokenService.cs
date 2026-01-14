using System.Security.Cryptography;
using Shared.Application.Abstractions.Authentication;

namespace Users.Infrastructure.Authentication
{
    public sealed class RefreshTokenService : IRefreshTokenService
    {
        public int RefreshTokenExpiryDays { get; }

        public RefreshTokenService(int refreshTokenExpiryDays = 7)
        {
            RefreshTokenExpiryDays = refreshTokenExpiryDays;
        }

        /// <summary>
        /// Generates a new secure refresh token entity for a user.
        /// </summary>
        public RefreshToken GenerateToken(Guid userId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new RefreshToken(
                token: token,
                expiryDate: DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                userId: userId
            );
        }

        /// <summary>
        /// Validates a refresh token (expiry + revoked status).
        /// </summary>
        public bool ValidateToken(RefreshToken token)
        {
            return token != null && !token.IsRevoked && !token.IsExpired();
        }

        /// <summary>
        /// Revokes a refresh token so it can no longer be used.
        /// </summary>
        public void RevokeToken(RefreshToken token)
        {
            token?.Revoke();
        }
    }
}
