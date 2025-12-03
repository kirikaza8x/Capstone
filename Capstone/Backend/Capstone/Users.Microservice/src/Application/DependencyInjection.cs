using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Common.Commands;
using Users.Domain;


namespace Users.Application
{
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
            return services;
        }
    }
}