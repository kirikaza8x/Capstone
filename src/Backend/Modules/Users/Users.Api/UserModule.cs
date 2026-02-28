using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Middleware;
using Users.Application;
using Users.Infrastructure;
using Users.Infrastructure.Persistence.Contexts;

public static class UserModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.UsersAddApplication();
        services.AddUserInfrastructure(configuration);
        return services;
    }

    public static IApplicationBuilder UseUserModule(this IApplicationBuilder app)
    {
        app.UseMiddleware<DeviceIdMiddleware>();
        app.UseMigration<UserModuleDbContext>();
        return app;
    }
}