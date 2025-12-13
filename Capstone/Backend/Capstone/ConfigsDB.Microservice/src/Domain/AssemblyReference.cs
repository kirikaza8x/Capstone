using Microsoft.Extensions.DependencyInjection;
namespace ConfigsDB.Domain
{
    public static class DomainAssemblyReference { }
    public static class DependencyInjection
    {
       public static IServiceCollection AddDomain(this IServiceCollection services)
       {
           return services;
       }
    }
}
