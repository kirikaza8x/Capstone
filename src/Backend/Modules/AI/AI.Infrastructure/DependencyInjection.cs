using AI.Application.Abstractions;
using AI.Application.Abstractions.Qdrant;
using AI.Application.Services;
using AI.Infrastructure.Data;
using AI.Infrastructure.Embedding;
using AI.Infrastructure.ExternalServices;
using AI.Infrastructure.Qdrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Qdrant.Client;
using Shared.Application.Abstractions;
using Shared.Application.Abstractions.Embbeding;
using Shared.Domain.Data;
using Shared.Domain.Data.Repositories;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Configs.Qdrant;
using Shared.Infrastructure.Data.Seeds;
using Shared.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Infrastructure.Configs.MessageBroker;

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
            var builder = new NpgsqlDataSourceBuilder(dbConfig.ConnectionString);
            builder.EnableDynamicJson();
            return builder.Build();
        });

        services.AddDbContext<AIModuleDbContext>((sp, options) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

            options.UseNpgsql(dataSource, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                    npgsql.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                if (dbConfig.CommandTimeout > 0)
                    npgsql.CommandTimeout(dbConfig.CommandTimeout);
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<ITrackingTokenGenerator, TrackingTokenGenerator>();

        // ── 3. External AI services ───────────────────────────────
        services.AddHttpClient<IImageGenerationService, OpenRouterImageService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120);
        });

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

        services.AddHostedService<QdrantStartupService>();
        services.AddHostedService<BackgroundJobs.EventReIndexJob>();
        services.AddHostedService<BackgroundJobs.GlobalCategoryStatUpdateJob>();

        services.AddScoped<IEventReIndexService, EventReIndexService>();

        // ── 5. Embedding (RabbitMQ → Python) ──────────────────────

        // ── Embedding (HTTP fallback - DEV only) ─────────────────────
        //
        // Uncomment this block if you want to bypass RabbitMQ
        // and call Python (or any embedding API) directly via HTTP.
        //
        // Useful for:
        // - debugging
        // - local development
        // - testing without message broker
        //

        // var embeddingOptions = configuration
        //     .GetSection(HttpEmbeddingOptions.Section)
        //     .Get<HttpEmbeddingOptions>() ?? new HttpEmbeddingOptions();
        //
        // services.Configure<HttpEmbeddingOptions>(
        //     configuration.GetSection(HttpEmbeddingOptions.Section));
        //
        // services.AddHttpClient<IEmbeddingService, HttpEmbeddingService>(client =>
        // {
        //     client.BaseAddress = new Uri(embeddingOptions.BaseUrl);
        //     client.Timeout = TimeSpan.FromSeconds(embeddingOptions.TimeoutSeconds);
        //     client.DefaultRequestHeaders.Add("Accept", "application/json");
        // });

        services.Configure<EmbeddingQueueOptions>(
            configuration.GetSection(EmbeddingQueueOptions.Section));

        services.Configure<MessageBrokerConfig>(
            configuration.GetSection("MessageBroker"));

        services.AddSingleton<IConnection>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<MessageBrokerConfig>>().Value;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(config.Host),
                UserName = config.Username,
                Password = config.Password
            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddSingleton<IEmbeddingService, RabbitMqEmbeddingService>();

        return services;
    }
}