using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Modules;

namespace Shared.Api.Extensions;

public static class ModuleExtensions
{
    private static readonly List<IModule> RegisteredModules = new();

    public static IServiceCollection AddModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : IModule, new()
    {
        var module = new TModule();
        module.RegisterModule(services, configuration);
        RegisteredModules.Add(module);
        return services;
    }

    public static IServiceCollection AddModulesFromAssemblies(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            if (RegisteredModules.Any(m => m.GetType() == type)) continue; 
            var module = (IModule)Activator.CreateInstance(type)!;
            module.RegisterModule(services, configuration);
            RegisteredModules.Add(module);
        }

        return services;
    }
}