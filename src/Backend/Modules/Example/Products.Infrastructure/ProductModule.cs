using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Products.Domain.Products;
using Products.Infrastructure.Data;
using Products.Infrastructure.Data.Repositories;
using Products.Infrastructure.PublicApi;
using Products.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Data;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.Extensions;

namespace Products.Infrastructure;

public static class ProductModule
{
    public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions<DatabaseConfigSetup>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        services.TryAddScoped<IProductRepository, ProductRepository>();

        // Register Public API
        services.AddScoped<IProductsApi, ProductsApi>();

        services.AddDbContext<ProductsDbContext>((sp, options) =>
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

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductsDbContext>());

        return services;
    }

    public static IApplicationBuilder UseProductModule(this IApplicationBuilder app)
    {
        app.UseMigration<ProductsDbContext>();
        return app;
    }
}
