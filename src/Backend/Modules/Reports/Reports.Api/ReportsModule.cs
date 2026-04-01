using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Api;

public static class ReportsModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public static IApplicationBuilder UseUserModule(this IApplicationBuilder app)
    {
        return app;
    }
}
