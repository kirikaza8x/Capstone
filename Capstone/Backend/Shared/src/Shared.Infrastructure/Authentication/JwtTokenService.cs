using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;

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
/// <para><b>Hot-Reload Support:</b></para>
/// - Configuration can be updated at runtime via <see cref="UpdateConfigurationAsync"/>
/// - Listens to JwtConfigurationChangedEvent from ConfigDbService
/// - No service restart required for config changes
///
/// <b>Note:</b> Do not use <see cref="ValidateToken"/> on refresh tokens, as they are not JWTs.
/// </summary>
public class JwtTokenService : IJwtTokenService, IConfigurableJwtService
{
    private readonly JwtConfigs _defaultConfig;
    private readonly ILogger<JwtTokenService> _logger;
    
    //  In-memory cached values (updated via hot-reload)
    private int _expiryMinutes;
    private int _refreshTokenExpiryDays;
    
    // Immutable values (cannot be hot-reloaded for security reasons)
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(
        IOptions<JwtConfigs> options,
        ILogger<JwtTokenService> logger)
    {
        _defaultConfig = options.Value;
        _logger = logger;
        
        //  Initialize from config
        _expiryMinutes = _defaultConfig.ExpiryMinutes;
        _refreshTokenExpiryDays = _defaultConfig.RefreshTokenExpiryDays;
        
        // Immutable security settings
        _secret = _defaultConfig.Secret;
        _issuer = _defaultConfig.Issuer;
        _audience = _defaultConfig.Audience;
        
        _logger.LogInformation(
            "JwtTokenService initialized: ExpiryMinutes={ExpiryMinutes}, RefreshTokenExpiryDays={RefreshDays}",
            _expiryMinutes,
            _refreshTokenExpiryDays);
    }

    /// <inheritdoc />
    public int ExpiryMinutes => _expiryMinutes;  //  Now returns hot-reloadable value
    
    /// <inheritdoc />
    public int RefreshTokenExpiryDays => _refreshTokenExpiryDays;  //  Now returns hot-reloadable value

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  NEW: Hot-Reload Configuration Support
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    
    /// <summary>
    /// Updates JWT configuration at runtime without service restart
    /// Called by JwtConfigurationChangedConsumer when config changes
    /// </summary>
    /// <param name="expiryMinutes">New access token expiry in minutes</param>
    /// <param name="refreshTokenExpiryDays">New refresh token expiry in days</param>
    public Task UpdateConfigurationAsync(int expiryMinutes, int refreshTokenExpiryDays)
    {
        var oldExpiry = _expiryMinutes;
        var oldRefreshDays = _refreshTokenExpiryDays;
        
        //  Update in-memory values (thread-safe for reading)
        _expiryMinutes = expiryMinutes;
        _refreshTokenExpiryDays = refreshTokenExpiryDays;
        
        _logger.LogInformation(
            "JWT configuration updated: " +
            "ExpiryMinutes {OldExpiry}→{NewExpiry}, " +
            "RefreshTokenExpiryDays {OldRefreshDays}→{NewRefreshDays}",
            oldExpiry, _expiryMinutes,
            oldRefreshDays, _refreshTokenExpiryDays);
        
        return Task.CompletedTask;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // Existing Methods (Use hot-reloadable values)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string? email, string? name, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

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
            Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),  //  Uses hot-reloadable value
            Issuer = _issuer,
            Audience = _audience,
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
            var key = Encoding.ASCII.GetBytes(_secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = !allowExpired,
                ClockSkew = TimeSpan.Zero
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
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
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to check token expiry");
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
        catch (Exception ex)
        {
            // If parsing fails, treat as expired
            _logger.LogDebug(ex, "Failed to get token expiry time");
            return -1;
        }
    }
}