using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Shared.Domain.Abstractions;

namespace Shared.Application.Behaviors;

/// <summary>
/// Pipeline behavior for request logging with Serilog structured logging
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string moduleName = GetModuleName(typeof(TRequest).FullName!);
        string requestName = typeof(TRequest).Name;

        // OpenTelemetry Activity tags
        Activity.Current?.SetTag("request.module", moduleName);
        Activity.Current?.SetTag("request.name", requestName);

        var stopwatch = Stopwatch.StartNew();

        using (LogContext.PushProperty("Module", moduleName))
        {
            logger.LogInformation("Processing request {RequestName}", requestName);

            TResponse result = await next();

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                logger.LogInformation(
                    "Completed request {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                using (LogContext.PushProperty("Error", result.Error, destructureObjects: true))
                {
                    logger.LogError(
                        "Completed request {RequestName} with error in {ElapsedMilliseconds}ms",
                        requestName,
                        stopwatch.ElapsedMilliseconds);
                }
            }

            return result;
        }
    }

    private static string GetModuleName(string fullName)
    {
        var parts = fullName.Split('.');
        return parts.Length > 0 ? parts[0] : "Unknown";
    }
}
