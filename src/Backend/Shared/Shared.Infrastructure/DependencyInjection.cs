using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Quartz;
using Shared.Infrastructure.Inbox;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
     this IServiceCollection services,
     IConfiguration configuration,
     Action<IRegistrationConfigurator>[] moduleConfigureConsumers)
    {
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
