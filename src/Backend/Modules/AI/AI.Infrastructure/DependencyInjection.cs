using AI.Application.Abstractions;
using AI.Application.Abstractions.Qdrant;
using AI.Application.Services;
using AI.Infrastructure.Data;
using AI.Infrastructure.Embedding;
using AI.Infrastructure.ExternalServices;
using AI.Infrastructure.Qdrant;
using Events.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Qdrant.Client;
using Shared.Application.Abstractions.Embbeding;
using Shared.Application.Abstractions.EventBus;
using Shared.Domain.Data;
using Shared.Domain.Data.Repositories;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Configs.Qdrant;
using Shared.Infrastructure.Data.Seeds;

namespace AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Config + Repository scanning ──────────────────────
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

        // ── 2. Database ───────────────────────────────────────────
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

        // ── 3. External AI services ───────────────────────────────
        services.AddHttpClient<IImageGenerationService, OpenRouterImageService>();
        services.AddScoped<IGeminiService, GeminiService>();
        services.AddScoped<IRecommendationAiService, RecommendationAiService>();

        // ── 4. Qdrant ─────────────────────────────────────────────
        var qdrantConfig = configuration.GetSection("Qdrant").Get<QdrantConfig>()
            ?? throw new InvalidOperationException("Qdrant config section is missing.");

        services.AddSingleton(qdrantConfig);

        services.AddSingleton(_ => new QdrantClient(
            host: qdrantConfig.Host,
            port: qdrantConfig.Port,
            https: qdrantConfig.UseHttps,
            apiKey: string.IsNullOrWhiteSpace(qdrantConfig.ApiKey) ? null : qdrantConfig.ApiKey
        ));

        services.AddScoped<IEventVectorRepository, EventVectorRepository>();
        services.AddScoped<IUserBehaviorVectorRepository, UserBehaviorVectorRepository>();

        // Ensures Qdrant collections + indexes exist before app accepts traffic
        services.AddHostedService<QdrantStartupService>();
        services.AddHostedService<BackgroundJobs.EventReIndexJob>();

        // Updates GlobalCategoryStat every 6 hours for cold-start recommendations
        services.AddHostedService<BackgroundJobs.GlobalCategoryStatUpdateJob>();

        // Re-index service — shared by job and manual admin endpoint
        services.AddScoped<IEventReIndexService, EventReIndexService>();

        // ── 5. Embedding — HTTP for dev, swap comment for production ──
        var embeddingOptions = configuration
            .GetSection(HttpEmbeddingOptions.Section)
            .Get<HttpEmbeddingOptions>() ?? new HttpEmbeddingOptions();

        services.Configure<HttpEmbeddingOptions>(
            configuration.GetSection(HttpEmbeddingOptions.Section));

        services.AddHttpClient<IEmbeddingService, HttpEmbeddingService>(client =>
        {
            client.BaseAddress = new Uri(embeddingOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(embeddingOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });


        // Production: comment out HTTP block above and uncomment below
        // services.Configure<EmbeddingQueueOptions>(
        //     configuration.GetSection(EmbeddingQueueOptions.Section));
        // services.AddSingleton<RabbitMQ.Client.IConnection>(_ =>
        // {
        //     var factory = new RabbitMQ.Client.ConnectionFactory
        //     {
        //         HostName = configuration["RabbitMQ:Host"] ?? "localhost",
        //         Port     = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
        //         UserName = configuration["RabbitMQ:Username"] ?? "guest",
        //         Password = configuration["RabbitMQ:Password"] ?? "guest",
        //         AutomaticRecoveryEnabled = true,
        //     };
        //     return factory.CreateConnectionAsync("ai-embedding-client").GetAwaiter().GetResult();
        // });
        // services.AddSingleton<IEmbeddingService>(sp =>
        //     RabbitMqEmbeddingService.CreateAsync(
        //         sp.GetRequiredService<RabbitMQ.Client.IConnection>(),
        //         sp.GetRequiredService<IOptions<EmbeddingQueueOptions>>(),
        //         sp.GetRequiredService<ILogger<RabbitMqEmbeddingService>>()
        //     ).GetAwaiter().GetResult());

        return services;
    }
}
