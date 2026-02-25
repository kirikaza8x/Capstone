using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Configs;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Shared.Infrastructure;
public  class SharedInfrastructureAssemblyReference
{
   
}

public static class IInfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Assembly[] moduleAssemblies)
    {
        services.AddOptions();
        services.Scan(scan => scan
            .FromAssemblyOf<SharedInfrastructureAssemblyReference>()
            .AddClasses(classes => classes.AssignableTo<ConfigBase>())
            .AsSelf()
            .WithSingletonLifetime());

        services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigurationBinderSetup<>));

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddHttpContextAccessor();
        return services;
    }
}