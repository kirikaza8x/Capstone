using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Report;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Service.Report;
using Ticketing.Application.Abstractions.Locks;
using Ticketing.Application.Abstractions.Notifications;
using Ticketing.Application.Orders.Queries.ExportOrdersSheet;
using Ticketing.Application.Orders.Queries.ExportVoucherSheet;
using Ticketing.Application.Services;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;
using Ticketing.Infrastructure.Data;
using Ticketing.Infrastructure.Data.Repositories;
using Ticketing.Infrastructure.Data.Uow;
using Ticketing.Infrastructure.Jobs;
using Ticketing.Infrastructure.Locks;
using Ticketing.Infrastructure.PublicApi;
using Ticketing.Infrastructure.Services.Reports;
using Ticketing.Infrastructure.SignalR;
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

        services.AddSignalR();
        services.AddTransient<ICheckInStatsNotifier, SignalRCheckInStatsNotifier>();
        services.AddScoped<ICheckInStatsBroadcaster, CheckInStatsBroadcaster>();
        // Report services
        services.AddScoped<ISheetMappings<OrderExportDto>, OrderSheetMappings>();
        services.AddScoped<IFileImportExportService<OrderExportDto>>(sp =>
        {
            var mappings = sp.GetRequiredService<ISheetMappings<OrderExportDto>>();
            return new ClosedXmlImportExportService<OrderExportDto>(
                mappings.GetRowMapper(),
                mappings.Exporter
            );
        });
        services.AddScoped<ISheetMappings<VoucherExportDto>, VoucherSheetMappings>();
        services.AddScoped<IFileImportExportService<VoucherExportDto>>(sp =>
        {
            var mappings = sp.GetRequiredService<ISheetMappings<VoucherExportDto>>();
            return new ClosedXmlImportExportService<VoucherExportDto>(
                mappings.GetRowMapper(),
                mappings.Exporter
            );
        });

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

        // Map SignalR hubs
        var endpointRouteBuilder = (IEndpointRouteBuilder)app;
        endpointRouteBuilder.MapHub<TicketHub>("/hubs/ticket-hub");
        return app;
    }
}
