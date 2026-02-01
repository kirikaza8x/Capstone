using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.EventBus;
using Shared.Infrastructure.Configs.MessageBroker;
using Shared.Infrastructure.EventBus;
using System.Reflection;

namespace Shared.Infrastructure.Extensions;

public static class MassTransitExtentions
{
    public static IServiceCollection AddMassTransitWithAssemblies
        (this IServiceCollection services,
            IConfiguration configuration,
            params Assembly[] assemblies)
    {
        services.ConfigureOptions<MessageBrokerConfigSetup>();

        // Register IEventBus
        services.AddScoped<IEventBus, Shared.Infrastructure.EventBus.EventBus>();

        // Auto-register all IntegrationEventHandlers
        RegisterIntegrationEventHandlers(services, assemblies);

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.SetInMemorySagaRepositoryProvider();

            // Register regular consumers from assemblies
            config.AddConsumers(assemblies);

            // Register consumers for IntegrationEventHandlers
            RegisterIntegrationEventConsumers(config, assemblies);

            config.AddSagaStateMachines(assemblies);
            config.AddSagas(assemblies);
            config.AddActivities(assemblies);

            config.UsingRabbitMq((context, configurator) =>
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

    private static void RegisterIntegrationEventHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        // find all class implement IIntegrationEventHandler<T>
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract &&
                   t.GetInterfaces().Any(i => i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            // Regiser handler into DI container
            services.AddScoped(interfaceType, handlerType);
        }
    }

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