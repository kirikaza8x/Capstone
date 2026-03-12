using AI.Infrastructure;
using AI.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        // app.UseMigration<AIModuleDbContext>();
        return app;
    }
}