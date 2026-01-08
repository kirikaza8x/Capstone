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
       

        return services;
    }
}
