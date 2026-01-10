using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Extensions;
using System.Reflection;

namespace Shared.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Assembly[] moduleAssemblies)
    {
        services.AddMediatRWithBehaviors(moduleAssemblies);
        return services;
    }
}