using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.EventBus;
using Shared.Infrastructure.Configs.MessageBroker;
using Shared.Infrastructure.EventBus;

namespace Shared.Infrastructure.Extensions;

public static class MassTransitExtentions
{
    public static IServiceCollection AddMassTransitWithAssemblies
        (this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] assemblies)
    {

        var redisConfig = configuration.GetSection("MessageBroker").Get<MessageBrokerConfig>() ?? new MessageBrokerConfig();

        // Register IEventBus
        services.AddScoped<IEventBus, EventBus.EventBus>();

        // Auto-register all IntegrationEventHandlers
        RegisterIntegrationEventHandlers(services, assemblies);

        services.AddMassTransit(redisConfig =>
        {
            redisConfig.SetKebabCaseEndpointNameFormatter();
            redisConfig.SetInMemorySagaRepositoryProvider();

            // Register regular consumers from assemblies
            redisConfig.AddConsumers(assemblies);

            // Register consumers for IntegrationEventHandlers
            RegisterIntegrationEventConsumers(redisConfig, assemblies);

            redisConfig.AddSagaStateMachines(assemblies);
            redisConfig.AddSagas(assemblies);
            redisConfig.AddActivities(assemblies);

            redisConfig.UsingRabbitMq((context, configurator) =>
            {
                var brokerConfig = context.GetRequiredService<IOptions<MessageBrokerConfig>>().Value;
                configurator.Host(new Uri(brokerConfig.Host), host =>
                {
                    host.Username(brokerConfig.Username);
                    host.Password(brokerConfig.Password);
                });
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    /// <summary>
    /// Scan assemblies with Scrutor to find all IIntegrationEventHandler<T> and register them into DI container
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    private static void RegisterIntegrationEventHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .AssignableTo(typeof(IIntegrationEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    /// <summary>
    /// Scan assemblies to find all IIntegrationEventHandler<T> and register corresponding IntegrationEventConsumer<T> into MassTransit
    /// </summary>
    /// <param name="config"></param>
    /// <param name="assemblies"></param>
    // private static void RegisterIntegrationEventHandlers(IServiceCollection services, Assembly[] assemblies)
    // {
    //     // find all class implement IIntegrationEventHandler<T>
    //     var handlerTypes = assemblies
    //         .SelectMany(a => a.GetTypes())
    //         .Where(t => t.IsClass && !t.IsAbstract &&
    //                t.GetInterfaces().Any(i => i.IsGenericType &&
    //                i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
    //         .ToList();

    //     foreach (var handlerType in handlerTypes)
    //     {
    //         var interfaceType = handlerType.GetInterfaces()
    //             .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

    //         // Regiser handler into DI container
    //         services.AddScoped(interfaceType, handlerType);
    //     }
    // }

    private static void RegisterIntegrationEventConsumers(IRegistrationConfigurator config, Assembly[] assemblies)
    {
        // Find all IntegrationEventHandler
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract &&
                   t.GetInterfaces().Any(i => i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // Get event type
            var eventType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                .GetGenericArguments()[0];

            // Create consumer wrapper type
            var consumerType = typeof(IntegrationEventConsumer<>).MakeGenericType(eventType);
            config.AddConsumer(consumerType);
        }
    }
}


