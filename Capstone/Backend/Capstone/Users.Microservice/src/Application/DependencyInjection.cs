using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Common.Commands;
using Users.Application;


namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services , IConfiguration configuration)
        {
            var assembly = typeof(DependencyInjection).Assembly;
            var appliAssembly = typeof(ApplicationAssemblyReference).Assembly;
            var sharedLibraryAssembly = typeof(SaveChangesCommandHandler).Assembly;
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(assembly);
                configuration.RegisterServicesFromAssembly(sharedLibraryAssembly);
            });
            services.AddValidatorsFromAssembly(assembly);
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            services.AddAutoMapper(appliAssembly);
            return services;
        }
    }
}