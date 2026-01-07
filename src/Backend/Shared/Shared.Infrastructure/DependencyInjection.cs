using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Quartz;
using Shared.Application.Caching;
using Shared.Application.Data;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Inbox;
using Shared.Infrastructure.Outbox;
using StackExchange.Redis;
using System.Configuration;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
     this IServiceCollection services,
     IConfiguration configuration,
     Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
     string databaseConnectionString,
     string redisConnectionString)
    {
        // PostgreSQL
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        // Redis 
        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
            services.TryAddSingleton<ICacheService, CacheService>();
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, CacheService>();
        // Outbox/Inbox
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.SectionName));
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        // Quartz
        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddMassTransit(configure =>
        {
            foreach (Action<IRegistrationConfigurator> configureConsumers in moduleConfigureConsumers)
            {
                configureConsumers(configure);
            }

            configure.SetKebabCaseEndpointNameFormatter();

            configure.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
