using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;

namespace Users.Application.Features.Users.Queries
{
    // Query record to request device information
    public record GetCurrentDeviceInfoQuery(
        string? UserAgent,
        string? IpAddress,
        string? DeviceId
    ) : IQuery<DeviceInfo>;
}
