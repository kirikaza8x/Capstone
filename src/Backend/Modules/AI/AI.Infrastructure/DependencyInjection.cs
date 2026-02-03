using AI.Application.Abstractions;
using AI.Infrastructure.Configs;
using Infrastructure.Services.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions.Authentication;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Interceptors;


namespace AI.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureOptions<DatabaseConfigSetup>();
            services.ConfigureOptions<GeminiConfigSetup>();
            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
            services.AddHttpContextAccessor();
            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
