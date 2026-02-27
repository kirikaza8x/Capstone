using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Caching;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Redis;
using Shared.Infrastructure.Extensions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Shared.Infrastructure;
public  class SharedInfrastructureAssemblyReference
{
   
}

public static class IInfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Assembly[] moduleAssemblies,
        IConfiguration configuration)
    {
        services.AddOptions();
        services.Scan(scan => scan
            .FromAssemblyOf<SharedInfrastructureAssemblyReference>()
            .AddClasses(classes => classes.AssignableTo<ConfigBase>())
            .AsSelf()
            .WithSingletonLifetime());

        services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigurationBinderSetup<>));

        // redis
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfig>() ?? new RedisConfig();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
            options.InstanceName = redisConfig.InstanceName;
        });

        services.AddSingleton<ICacheService, CacheService>();

        services.AddMassTransitWithAssemblies(configuration, moduleAssemblies);

        services.AddStorageService(configuration);

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddHttpContextAccessor();

        return services;
    }
}