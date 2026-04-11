using Shared.Domain.DDD;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; } = default!;
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public Guid UserId { get; private set; }

    //public string? ReplacedByToken { get; private set; }
    // public string? RevokedByIp { get; private set; }
    // public string? ReasonForRevocation { get; private set; }

    // Multi-device support
    public string? DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(
        string token,
        DateTime expiryDate,
        Guid userId,
        string? deviceId = null,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiryDate = expiryDate,
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    /// <summary>
    /// Marks the refresh token as revoked.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
    }

    /// <summary>
    /// Updates device metadata for this token.
    /// </summary>
    public void UpdateDeviceInfo(string? deviceName, string? ipAddress, string? userAgent)
    {
        if (!string.IsNullOrWhiteSpace(deviceName))
            DeviceName = deviceName;

        if (!string.IsNullOrWhiteSpace(ipAddress))
            IpAddress = ipAddress;

        if (!string.IsNullOrWhiteSpace(userAgent))
            UserAgent = userAgent;
    }

    /// <summary>
    /// Checks if the token is expired.
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow >= ExpiryDate;

    /// <summary>
    /// Checks if the token is still valid (not expired and not revoked).
    /// </summary>
    public bool IsValid() => !IsRevoked && !IsExpired();
}
