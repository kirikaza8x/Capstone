using AI.Application.Abstractions;
using AI.Application.Services;
using AI.Domain.Services;
using AI.Infrastructure.BackgroundJobs;
using AI.Infrastructure.Data;
using AI.Infrastructure.ExternalServices;
using AI.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Data;
using Shared.Infrastructure.Service.Authentication;
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

            services.AddSingleton<NpgsqlDataSource>(sp =>
            {
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConfig.ConnectionString);

                dataSourceBuilder.EnableDynamicJson();

                return dataSourceBuilder.Build();
            });
            // Register DbContext with DatabaseConfig
            services.AddDbContext<AIModuleDbContext>((sp, options) =>
            {
                var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

                options.UseNpgsql(dataSource, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);
                    if (dbConfig.MaxRetryCount > 0)
                        npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                    if (dbConfig.CommandTimeout > 0)
                        npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
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
            services.AddScoped<IGlobalTrendService, GlobalTrendService>();
            services.AddScoped<IUserActivityOrchestrator, UserActivityOrchestrator>();
            services.AddScoped<InteractionWeightCalculator>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
            services.AddHostedService<GlobalTrendWorker>();
            services.AddHttpContextAccessor();

            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
