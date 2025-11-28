using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWithScrutor<TMarker>(
            this IServiceCollection services,
            string? @namespace = null,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<TMarker>() // marker type to locate assembly
                .AddClasses(classes =>
                {
                    if (!string.IsNullOrWhiteSpace(@namespace))
                        classes.InNamespaces(@namespace);
                })
                .AsImplementedInterfaces()
                .WithLifetime(lifetime)
            );

            return services;
        }
    }
}
