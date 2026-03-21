using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application;
using Payments.Infrastructure;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Middleware;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPaymentApplication();
        services.AddPaymentsInfrastructure(configuration);
        return services;
    }

    public static IApplicationBuilder UsePaymentModule(this IApplicationBuilder app)
    {
        app.UseMiddleware<DeviceIdMiddleware>();
        app.UseMigration<PaymentModuleDbContext>();
        return app;
    }
}
