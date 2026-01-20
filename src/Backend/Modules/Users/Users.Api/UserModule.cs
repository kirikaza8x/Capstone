using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Infrastructure;

public static class UserModule 
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration){
        //services.UsersAddApplication();
        services.AddUserInfrastructure(configuration);
        return services; 
    }
}