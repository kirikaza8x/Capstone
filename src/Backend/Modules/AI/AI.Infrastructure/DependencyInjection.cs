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
using Shared.Domain.Data;
using Shared.Domain.Data.Repositories;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Seeds;

namespace AI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            // Register repositories
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Register Unit of Work
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IUnitOfWork)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
               .FromAssemblyOf<AiInfrastructureAssemblyReference>()
               .AddClasses(classes => classes.AssignableTo(typeof(IDataSeeder<>)))
               .AsImplementedInterfaces()
               .WithScopedLifetime());


            services.AddSingleton<NpgsqlDataSource>(sp =>
            {
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConfig.ConnectionString);

                dataSourceBuilder.EnableDynamicJson();

                return dataSourceBuilder.Build();
            });

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

            // Register services
            services.AddHttpClient<IImageGenerationService, OpenRouterImageService>();
            services.AddScoped<IGlobalTrendService, GlobalTrendService>();
            services.AddScoped<IUserActivityOrchestrator, UserActivityOrchestrator>();
            services.AddScoped<InteractionWeightCalculator>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddHostedService<GlobalTrendWorker>();

            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
