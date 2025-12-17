using ConfigsDB.Application.Abstractions.Configs;
using ConfigsDB.Application.Features.ConfigSettings.SyncStrategies;
using ConfigsDB.Application.Services;
using ConfigsDB.Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Common.Commands;
namespace ConfigsDB.Application
{
    public static class ApplicationAssemblyReference { }
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            var appliAssembly = typeof(ApplicationAssemblyReference).Assembly;
            var domainAssembly = typeof(DomainAssemblyReference).Assembly;
            var sharedLibraryAssembly = typeof(SaveChangesCommandHandler).Assembly;
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(domainAssembly);
                configuration.RegisterServicesFromAssembly(appliAssembly);
                configuration.RegisterServicesFromAssembly(sharedLibraryAssembly);
            });
            services.AddValidatorsFromAssembly(domainAssembly);
            services.AddValidatorsFromAssembly(appliAssembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddAutoMapper(appliAssembly);
            // 1. The Distributor
            services.AddScoped<IConfigDistributor, ConfigDistributor>();

            // 2. The Strategies (The Map)
            // Add a new line here every time you create a new strategy class
            services.AddScoped<IConfigSyncStrategy, JwtSyncStrategy>();
            // services.AddScoped<IConfigSyncStrategy, EmailSyncStrategy>();
            return services;
        }
    }
}
