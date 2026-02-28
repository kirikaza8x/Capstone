using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using System.Reflection;
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
        // Add CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}