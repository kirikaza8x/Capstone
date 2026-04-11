using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Shared.Infrastructure.Configs;

namespace Shared.Api;

public class SharedApiAssemblyReference
{
}

public static class IApiConfiguration
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        Assembly[] moduleAssemblies,
        IConfiguration configuration)
    {
        services.AddCarterWithAssemblies(moduleAssemblies);
        services.AddAuthentication();
        services.AddAuthorization();

        ConfigureRateLimiting(services, configuration);

        var corsConfig = new CorsConfig();
        var corsSection = configuration.GetSection("Cors");

        if (corsSection.Exists())
        {
            corsSection.Bind(corsConfig);
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (corsConfig.AllowAnyOrigin)
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                // Scenario B: Specific Origins are provided in Config
                else if (corsConfig.AllowedOrigins.Any())
                {
                    policy.WithOrigins(corsConfig.AllowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                // Scenario C: No Config found - Default to standard wildcard (No Credentials)
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                    // Note: SignalR might need 'skipNegotiation' in the frontend for this case
                }
            });
        });

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.MapHub<LogHub>("api/logHub");
        return app;
    }

    private static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
    {
        var sectionName = new RateLimitingConfig().SectionName;
        var rateLimitingConfig = configuration.GetSection(sectionName).Get<RateLimitingConfig>();

        if (rateLimitingConfig is null)
        {
            throw new InvalidOperationException($"Missing configuration section '{sectionName}'.");
        }

        if (string.IsNullOrWhiteSpace(rateLimitingConfig.GlobalPolicy))
        {
            throw new InvalidOperationException("RateLimiting:GlobalPolicy is required.");
        }

        if (!rateLimitingConfig.Policies.Any())
        {
            throw new InvalidOperationException("RateLimiting:Policies must contain at least one policy.");
        }

        if (!rateLimitingConfig.Policies.TryGetValue(rateLimitingConfig.GlobalPolicy, out var globalPolicyConfig))
        {
            throw new InvalidOperationException($"Global policy '{rateLimitingConfig.GlobalPolicy}' was not found in RateLimiting:Policies.");
        }

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            foreach (var (policyName, policyConfig) in rateLimitingConfig.Policies)
            {
                options.AddPolicy(policyName, httpContext =>
                {
                    var partitionKey = BuildPartitionKey(httpContext, policyName);

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = policyConfig.PermitLimit,
                            Window = TimeSpan.FromSeconds(policyConfig.WindowSeconds),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = policyConfig.QueueLimit,
                            AutoReplenishment = true
                        });
                });
            }

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = BuildPartitionKey(httpContext, rateLimitingConfig.GlobalPolicy);

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = globalPolicyConfig.PermitLimit,
                        Window = TimeSpan.FromSeconds(globalPolicyConfig.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = globalPolicyConfig.QueueLimit,
                        AutoReplenishment = true
                    });
            });
        });
    }

    private static string BuildPartitionKey(HttpContext httpContext, string policyName)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? httpContext.User.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"{policyName}:user:{userId}";
        }

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        return $"{policyName}:ip:{ipAddress}";
    }
}
