using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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

        // 1. Try to bind the config
        var corsConfig = new CorsConfig();
        var corsSection = configuration.GetSection("Cors");

        // If the section exists in appsettings, bind it. 
        // Otherwise, corsConfig stays at its default values.
        if (corsSection.Exists())
        {
            corsSection.Bind(corsConfig);
        }

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                // Scenario A: Config says "Allow Any" (Development Mode)
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

    public static WebApplication UseApi(
        this WebApplication app
        // ,
        // ILogger logger
        )
    {
        app.MapHub<LogHub>("api/logHub");

        return app;
    }
}