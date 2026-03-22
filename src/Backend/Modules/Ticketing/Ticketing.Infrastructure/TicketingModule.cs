using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Extensions;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;
using Ticketing.Infrastructure.Data;
using Ticketing.Infrastructure.Data.Repositories;
using Ticketing.Infrastructure.Data.Uow;
using Ticketing.Infrastructure.Jobs;
using Ticketing.Infrastructure.Locks;
using Ticketing.Infrastructure.PublicApi;
using Ticketing.PublicApi;
using Ticketing.PublicApi.PublicApi;

namespace Ticketing.Infrastructure;

public static class TicketingModule
{
    public static IServiceCollection AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IOrderRepository, OrderRepository>();
        services.TryAddScoped<IVoucherRepository, VoucherRepository>();
        services.TryAddScoped<ITicketingUnitOfWork, TicketingUnitOfWork>();
        services.AddScoped<ITicketLockService, TicketLockService>();
        services.AddScoped<ITicketingSeatStatusPublicApi, TicketingSeatStatusPublicApi>();
        services.AddScoped<ITicketingPublicApi, TicketingPublicApi>();

        services.AddDbContext<TicketingDbContext>((sp, options) =>
        {
            var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;

            options.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);

                if (dbConfig.CommandTimeout > 0)
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        services.AddTicketingQuartzJobs();

        return services;
    }

    public static IApplicationBuilder UseTicketingModule(this IApplicationBuilder app)
    {
        app.UseMigration<TicketingDbContext>();
        return app;
    }
}
