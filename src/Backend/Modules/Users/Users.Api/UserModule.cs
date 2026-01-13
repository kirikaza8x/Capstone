using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Modules;
using Users.Application;
using Users.Infrastructure;

public class UserModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.UsersAddApplication();
        services.AddUserInfrastructure(configuration);
        return services; 
    }
}
