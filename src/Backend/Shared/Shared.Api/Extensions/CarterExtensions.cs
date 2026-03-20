using System.Reflection;
using Carter;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Api.Extensions;

public static class CarterExtensions
{
    public static IServiceCollection AddCarterWithAssemblies
        (this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddCarter(configurator: config =>
        {
            foreach (var assembly in assemblies)
            {
                var modules = assembly.GetTypes()
                .Where(t => t.IsAssignableTo(typeof(ICarterModule))).ToArray();

                config.WithModules(modules);
            }
        });

        return services;
    }
}
