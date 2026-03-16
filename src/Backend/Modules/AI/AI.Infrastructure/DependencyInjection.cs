using AI.Application.Abstractions;
using AI.Infrastructure.Data;
using AI.Infrastructure.ExternalServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Pgvector;
using Quartz;
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
            // 1. Configuration & Repository Scanning
            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<AiInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

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

            // 2. Npgsql DataSource Setup (Driver Level)
            services.AddSingleton<NpgsqlDataSource>(sp =>
            {
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConfig.ConnectionString);

                dataSourceBuilder.EnableDynamicJson();
                // dataSourceBuilder.UseVector(); 

                return dataSourceBuilder.Build();
            });

            services.AddDbContext<AIModuleDbContext>((sp, options) =>
            {
                var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

                options.UseNpgsql(dataSource, npgsqlOptions =>
                {
                    // npgsqlOptions.UseVector();

                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);

                    if (dbConfig.MaxRetryCount > 0)
                        npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                    if (dbConfig.CommandTimeout > 0)
                        npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            });

            services.AddHttpClient<IImageGenerationService, OpenRouterImageService>();


            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IRecommendationAiService,RecommendationAiService>();
           
            return services;
        }




    }
}