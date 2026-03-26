using Events.Application.Abstractions;
using Events.Application.Abstractions.Caching;
using Events.Application.EventMembers.Queries.ExportEventMembers;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Events.Infrastructure.Caching;
using Events.Infrastructure.Data;
using Events.Infrastructure.Data.Repositories;
using Events.Infrastructure.Jobs;
using Events.Infrastructure.PublicApi;
using Events.Infrastructure.Services;
using Events.Infrastructure.Services.Reports;
using Events.PublicApi.PublicApi;
using Microsoft.AspNetCore.Builder;
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

namespace Events.Infrastructure;

public static class EventModule
{
    public static IServiceCollection AddEventModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IEventRepository, EventRepository>();
        services.TryAddScoped<ICategoryRepository, CategoryRepository>();
        services.TryAddScoped<IHashtagRepository, HashtagRepository>();
        services.TryAddScoped<IEventUnitOfWork, EventUnitOfWork>();
        services.AddScoped<ISeatLockService, SeatLockService>();

        services.AddScoped<IEventMemberPublicApi, EventMemberPublicApi>();
        services.AddScoped<IEventTicketingPublicApi, EventTicketingPublicApi>();
        services.AddScoped<IEventMemberPermissionCacheInvalidator, EventMemberPermissionCacheInvalidator>();

        // Report services
        services.AddScoped<ISheetMappings<EventMemberExportDto>, EventMemberSheetMappings>();
        services.AddScoped<IFileImportExportService<EventMemberExportDto>>(sp =>
        {
            var mappings = sp.GetRequiredService<ISheetMappings<EventMemberExportDto>>();
            return new ClosedXmlImportExportService<EventMemberExportDto>(
                mappings.GetRowMapper(),
                mappings.Exporter
            );
        });

        services.AddDbContext<EventsDbContext>((sp, options) =>
        {
            var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
            options.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Constants.SchemaName);

                if (dbConfig.MaxRetryCount > 0)
                {
                    npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);
                }
                if (dbConfig.CommandTimeout > 0)
                {
                    npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
                }
            })
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        services.AddEventsQuartzJobs();
        return services;
    }

    public static IApplicationBuilder UseEventModule(this IApplicationBuilder app)
    {
        app.UseMigration<EventsDbContext>();
        return app;
    }
}
