
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;

namespace Users.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection UsersAddApplication(this IServiceCollection services)
        {
            services.AddApplication(new[]
            {
                UsersApplicationAssemblyReference.Assembly,
                typeof(ApplicationConfiguration).Assembly
            });
            services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(UsersApplicationAssemblyReference.Assembly);
            cfg.AddOpenBehavior(typeof(UserUnitOfWorkBehavior<,>));
            });
            return services;
        }
    }
}

