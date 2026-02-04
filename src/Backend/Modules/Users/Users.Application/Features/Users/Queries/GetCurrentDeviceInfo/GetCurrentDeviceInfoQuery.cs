using Shared.Application.DTOs;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Queries
{
    // Query record to request device information
    public record GetCurrentDeviceInfoQuery(
        string? UserAgent,
        string? IpAddress,
        string? DeviceId
    ) : IQuery<DeviceInfo>;
}
