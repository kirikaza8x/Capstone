using AI.Infrastructure;
using AI.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Infrastructure.Extensions;
using Users.Application;

public static class AiModule
{
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AiAddApplication();
        services.AddAiInfrastructure(configuration);
        return services;
    }


    public static IApplicationBuilder UseAiModule(this IApplicationBuilder app)
    {
        // Note: This synchronous version doesn't enable pgvector extension
        // Use UseAiModuleAsync for proper initialization
        return app;
    }
}