using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;

namespace Users.Infrastructure.Authentication
{
    public sealed class JwtTokenService : IJwtTokenService
    {
        private readonly ILogger<JwtTokenService> _logger;
        private readonly int _expiryMinutes;
        private readonly int _refreshTokenExpiryDays;
        private readonly byte[] _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenService(
            IOptions<JwtConfigs> options,
            ILogger<JwtTokenService> logger)
        {
            var config = options.Value;

            _logger = logger;

            _expiryMinutes = config.ExpiryMinutes;
            _refreshTokenExpiryDays = config.RefreshTokenExpiryDays;
            _key = Encoding.UTF8.GetBytes(config.Secret);
            _issuer = config.Issuer;
            _audience = config.Audience;

            _logger.LogInformation(
                "JwtTokenService initialized. Expiry: {Expiry}m, Refresh: {Refresh}d",
                _expiryMinutes,
                _refreshTokenExpiryDays);
        }

        public int ExpiryMinutes => _expiryMinutes;

        public int RefreshTokenExpiryDays => _refreshTokenExpiryDays;

        public string GenerateToken(
            Guid userId,
            string? email,
            string? name,
            IEnumerable<string> roles,
            string? deviceId = null)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(email))
                claims.Add(new(JwtRegisteredClaimNames.Email, email));

            if (!string.IsNullOrWhiteSpace(name))
                claims.Add(new(JwtRegisteredClaimNames.Name, name));

            if (!string.IsNullOrWhiteSpace(deviceId))
                claims.Add(new("DeviceId", deviceId));

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token, bool allowExpired = false)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var parameters = GetValidationParameters(allowExpired);

                return tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "JWT validation failed");
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public bool IsTokenExpired(string token)
            => GetMinutesUntilExpiry(token) <= 0;

        public int GetMinutesUntilExpiry(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return (int)(jwt.ValidTo - DateTime.UtcNow).TotalMinutes;
            }
            catch
            {
                return -1;
            }
        }

        private TokenValidationParameters GetValidationParameters(bool allowExpired)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),

                ValidateIssuer = true,
                ValidIssuer = _issuer,

                ValidateAudience = true,
                ValidAudience = _audience,

                ValidateLifetime = !allowExpired,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
