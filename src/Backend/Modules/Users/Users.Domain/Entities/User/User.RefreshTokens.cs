namespace Users.Domain.Entities
{
    public partial class User
    {
        // --------------------
        // Refresh Tokens
        // --------------------
        public ICollection<RefreshToken> RefreshTokens { get; private set; }
            = new List<RefreshToken>();

        public RefreshToken AddRefreshToken(
            string token,
            DateTime expiry,
            string? deviceId = null,
            string? deviceName = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            var refreshToken = RefreshToken.Create(
                token,
                expiry,
                Id,
                deviceId,
                deviceName,
                ipAddress,
                userAgent);

            RefreshTokens.Add(refreshToken);
            return refreshToken;
        }

        public void RevokeRefreshToken(string token)
            => RefreshTokens.FirstOrDefault(rt => rt.Token == token)?.Revoke();

        public void RevokeAllRefreshTokens()
        {
            foreach (var token in RefreshTokens.Where(rt => !rt.IsRevoked))
                token.Revoke();
        }

        public void RevokeRefreshTokensByDevice(string deviceId)
        {
            foreach (var token in RefreshTokens
                .Where(rt => rt.DeviceId == deviceId && !rt.IsRevoked))
                token.Revoke();
        }

        public RefreshToken? GetValidRefreshToken(string token, string? deviceId = null)
        {
            var query = RefreshTokens.Where(rt =>
                rt.Token == token &&
                !rt.IsRevoked &&
                rt.ExpiryDate > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(deviceId))
                query = query.Where(rt => rt.DeviceId == deviceId);

            return query.FirstOrDefault();
        }

        public RefreshToken? GetActiveRefreshTokenForDevice(string deviceId)
        {
            return RefreshTokens.FirstOrDefault(rt =>
                rt.DeviceId == deviceId &&
                !rt.IsRevoked &&
                !rt.IsExpired());
        }

        public IEnumerable<RefreshToken> GetActiveDevices()
        {
            return RefreshTokens
                .Where(rt => !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt);
        }

        public void LoginDevice(
            string token,
            DateTime expiry,
            string deviceId,
            string deviceName,
            string? ip,
            string? ua)
        {
            var existing = RefreshTokens.FirstOrDefault(t => t.DeviceId == deviceId);
            if (existing != null)
            {
                RefreshTokens.Remove(existing);
            }

            var finalIp = string.IsNullOrWhiteSpace(ip) ? "0.0.0.0" : ip;
            var finalUa = string.IsNullOrWhiteSpace(ua) ? "System/Unknown" : ua;

            var newToken = RefreshToken.Create(
                token,
                expiry,
                this.Id,
                deviceId,
                deviceName,
                finalIp,
                finalUa);

            RefreshTokens.Add(newToken);
        }
    }
}
