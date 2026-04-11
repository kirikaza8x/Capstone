using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Middleware;
public class DeviceIdMiddleware
{
    private const string HeaderName = "X-Device-ID";
    private readonly RequestDelegate _next;

    public DeviceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var deviceId)
            || string.IsNullOrWhiteSpace(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = deviceId;
        }

        await _next(context);
    }
}
