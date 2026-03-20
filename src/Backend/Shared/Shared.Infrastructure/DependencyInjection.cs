using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Abstractions.Time;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Redis;
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Service.Authentication;
using Shared.Infrastructure.Service.Caching;
using Shared.Infrastructure.Service.Time;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Shared.Infrastructure;

public class SharedInfrastructureAssemblyReference
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

        services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigurationBinderSetup<>));

        // redis
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfig>() ?? new RedisConfig();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig.ConnectionString;
            options.InstanceName = redisConfig.InstanceName;
        });

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(redisConfig.ConnectionString));
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();

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

        // Quartz
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}