using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;
using Shared.Infrastructure.Configs.Security;

namespace Shared.Authentication;

/// <summary>
/// JwtTokenService handles generation and validation of authentication tokens.
///
/// <para><b>Access Token:</b></para>
/// - Format: JWT (JSON Web Token)  
/// - Contains user claims, roles, and metadata (exp, iss, aud)  
/// - Short-lived (e.g., 60 minutes)  
/// - Used for authenticating API requests  
/// - Validated via <see cref="ValidateToken"/> with optional expiry bypass  
///
/// <para><b>Refresh Token:</b></para>
/// - Format: Random base64 string (not a JWT)  
/// - Long-lived (e.g., 7 days)  
/// - Stored securely in the database  
/// - Used to obtain new access tokens when expired  
/// - Generated via <see cref="GenerateRefreshToken"/>  
/// - Validated by direct comparison and expiry check — not decoded or parsed  
///
///  <b>Note:</b> Do not use <see cref="ValidateToken"/> on refresh tokens, as they are not JWTs.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfigs _jwt;
    public JwtTokenService(IOptions<JwtConfigs> options)
    {
        _jwt = options.Value;
    }

    /// <inheritdoc />
    public int ExpiryMinutes => _jwt.ExpiryMinutes;
    public int RefreshTokenExpiryDays => _jwt.RefreshTokenExpiryDays;

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string? email, string? name, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwt.Secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email ?? string.Empty),
            new(ClaimTypes.Name, name ?? string.Empty),
            new("jti", Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token, bool allowExpired = false)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwt.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwt.Audience,
                ValidateLifetime = !allowExpired,
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <inheritdoc />
    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            return jsonToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    /// <inheritdoc />
    public int GetMinutesUntilExpiry(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            // ValidTo is always in UTC
            var expiry = jsonToken.ValidTo;
            var remaining = expiry - DateTime.UtcNow;

            return (int)remaining.TotalMinutes;
        }
        catch
        {
            // If parsing fails, treat as expired
            return -1;
        }
    }
}
