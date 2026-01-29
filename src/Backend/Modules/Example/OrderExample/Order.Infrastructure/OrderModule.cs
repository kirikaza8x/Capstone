using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Order.Domain.Orders;
using Order.Infrastructure.Data;
using Order.Infrastructure.Data.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.EventBus;
using Shared.Domain.Data;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.EventBus;

namespace Order.Infrastructure;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions<DatabaseConfigSetup>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        services.TryAddScoped<IOrderRepository, OrderRepository>();
        services.TryAddScoped<IOrderUnitOfWork, OrderUnitOfWork>();

        services.AddDbContext<OrdersDbContext>((sp, options) =>
        {
            var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            options.UseNpgsql(dbConfig.ConnectionString, a =>
            {
                if (dbConfig.MaxRetryCount > 0)
                {
                    a.EnableRetryOnFailure(dbConfig.MaxRetryCount);
                }
                if (dbConfig.CommandTimeout > 0)
                {
                    a.CommandTimeout(dbConfig.CommandTimeout);
                }
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<IEventBus, EventBus>();
        return services;
    }

    public static IApplicationBuilder UseOrderModule(this IApplicationBuilder app)
    {
        app.UseMigration<OrdersDbContext>();
        return app;
    }
}
