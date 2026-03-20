using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;

namespace Shared.Application.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRWithAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);

            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        return services;
    }
}
