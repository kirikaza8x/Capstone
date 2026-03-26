using System.Security.Cryptography;
using System.Text;
using Shared.Application.Abstractions;

namespace Shared.Infrastructure.Services;

public class TrackingTokenGenerator : ITrackingTokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}