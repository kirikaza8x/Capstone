using Events.Domain.Repositories;
using Events.Domain.Uow;
using Events.Infrastructure.Data;
using Events.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Authentication;
using Shared.Infrastructure.Service.Authentication;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.Extensions;

namespace Events.Infrastructure;

public static class EventModule
{
    public static IServiceCollection AddEventModule(this IServiceCollection services, IConfiguration configuration)
    {
        // services.Scan(scan => scan
        //     .FromAssemblyOf<EventModuleAssemblyReference>()
        //     .AddClasses(classes => classes.AssignableTo<ConfigBase>())
        //     .AsSelf()
        //     .WithSingletonLifetime());
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();

        services.TryAddScoped<IEventRepository, EventRepository>();
        services.TryAddScoped<IEventUnitOfWork, EventUnitOfWork>();
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

        return services;
    }

    public static IApplicationBuilder UseEventModule(this IApplicationBuilder app)
    {
        app.UseMigration<EventsDbContext>();
        return app;
    }
}