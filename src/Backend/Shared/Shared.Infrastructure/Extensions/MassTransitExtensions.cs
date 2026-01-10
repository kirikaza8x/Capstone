using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Configs.MessageBroker;
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
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            config.SetInMemorySagaRepositoryProvider();

            config.AddConsumers(assemblies);
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
}