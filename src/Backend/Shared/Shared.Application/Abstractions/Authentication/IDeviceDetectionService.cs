using Shared.Application.DTOs;

namespace Shared.Application.Abstractions.Authentication
{
    /// <summary>
    /// Provides methods to detect and generate device information for multi-device session support.
    /// </summary>
    public interface IDeviceDetectionService
    {
        /// <summary>
        /// Extracts device information from the provided user agent string and optional existing device ID.
        /// </summary>
        /// <param name="userAgent">The user agent string from the request headers.</param>
        /// <param name="ipAddress">The IP address of the client.</param>
        /// <param name="existingDeviceId">Optional existing device identifier to reuse.</param>
        /// <returns>A strongly typed device info object.</returns>
        DeviceInfo GetDeviceInfo(string? userAgent, string? ipAddress = null, string? existingDeviceId = null);

        /// <summary>
        /// Generates a new unique device identifier.
        /// </summary>
        /// <returns>A unique device ID string.</returns>
        string GenerateDeviceId();

        /// <summary>
        /// Attempts to resolve a friendly device name (e.g., "iPhone 14", "Windows PC") from the user agent.
        /// </summary>
        /// <param name="userAgent">The user agent string.</param>
        /// <returns>A human-readable device name.</returns>
        string? ResolveDeviceName(string? userAgent);
    }
}
