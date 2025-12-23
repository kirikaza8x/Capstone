using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;

namespace Shared.Authentication;

public class JwtTokenService : IJwtTokenService, IConfigurableJwtService
{
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtConfigs _defaultConfig;
    private readonly ReaderWriterLockSlim _lock = new();

    // Hot-reloadable values
    private int _expiryMinutes;
    private int _refreshTokenExpiryDays;

    // Immutable security settings
    private readonly byte[] _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IOptions<JwtConfigs> options, ILogger<JwtTokenService> logger)
    {
        _defaultConfig = options.Value;
        _logger = logger;

        _expiryMinutes = _defaultConfig.ExpiryMinutes;
        _refreshTokenExpiryDays = _defaultConfig.RefreshTokenExpiryDays;

        _key = Encoding.ASCII.GetBytes(_defaultConfig.Secret);
        _issuer = _defaultConfig.Issuer;
        _audience = _defaultConfig.Audience;

        _logger.LogInformation(
            "JwtTokenService initialized. Expiry: {Expiry}m, Refresh: {Refresh}d", 
            _expiryMinutes, _refreshTokenExpiryDays);
    }

    public int ExpiryMinutes { get { _lock.EnterReadLock(); try { return _expiryMinutes; } finally { _lock.ExitReadLock(); } } }
    public int RefreshTokenExpiryDays { get { _lock.EnterReadLock(); try { return _refreshTokenExpiryDays; } finally { _lock.ExitReadLock(); } } }

    public Task UpdateConfigurationAsync(int expiryMinutes, int refreshTokenExpiryDays)
    {
        _lock.EnterWriteLock();
        try
        {
            _logger.LogInformation("Hot-reloading JWT Config: Expiry {Old} -> {New}", _expiryMinutes, expiryMinutes);
            _expiryMinutes = expiryMinutes;
            _refreshTokenExpiryDays = refreshTokenExpiryDays;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        return Task.CompletedTask;
    }

    public string GenerateToken(Guid userId, string? email, string? name, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, name ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(ExpiryMinutes), // Uses thread-safe property
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
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
            _logger.LogDebug(ex, "Token validation failed");
            return null;
        }
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public bool IsTokenExpired(string token) => GetMinutesUntilExpiry(token) <= 0;

    public int GetMinutesUntilExpiry(string token)
    {
        try
        {
            var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return (int)(jsonToken.ValidTo - DateTime.UtcNow).TotalMinutes;
        }
        catch
        {
            return -1;
        }
    }

    public void UseEmergencyDefaults()
    {
        _lock.EnterWriteLock();
        try
        {
            _logger.LogCritical("Applying Emergency JWT Defaults");
            _expiryMinutes = _defaultConfig.ExpiryMinutes > 0 ? _defaultConfig.ExpiryMinutes : 60;
            _refreshTokenExpiryDays = _defaultConfig.RefreshTokenExpiryDays > 0 ? _defaultConfig.RefreshTokenExpiryDays : 7;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private TokenValidationParameters GetValidationParameters(bool allowExpired) => new()
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