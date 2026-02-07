using AI.Application.Abstractions;
using AI.Infrastructure.Data;
using Infrastructure.Services.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Data;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Interceptors;

namespace AI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            // Register all config classes inheriting from ConfigBase
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            // Register binder once for all configs inheriting ConfigBase
            services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigurationBinderSetup<>));

            // Register DbContext with DatabaseConfig
            services.AddDbContext<AIModuleDbContext>((sp, options) =>
            {
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
                options.UseNpgsql(dbConfig.ConnectionString, a =>
                {
                    if (dbConfig.MaxRetryCount > 0)
                        a.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                    if (dbConfig.CommandTimeout > 0)
                        a.CommandTimeout(dbConfig.CommandTimeout);
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            });

            // Register repositories
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>() // assembly containing repositories
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Register Unit of Work
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IUnitOfWork)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Register interceptors
            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Register services
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
