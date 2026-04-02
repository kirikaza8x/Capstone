
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Payments.Application.Abstractions;
using Payments.Application.Features.Refunds.Services;
using Payments.Domain.UOW;
using Payments.Infrastructure.Data.UOW;
using Payments.Infrastructure.Persistence.Contexts;
using Payments.Infrastructure.Services;
using Shared.Domain.Data.Repositories;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Seeds;
namespace Payments.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<PaymentsInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<PaymentsInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<PaymentsInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IDataSeeder<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddDbContext<PaymentModuleDbContext>((sp, options) =>
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
            services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
            services.AddScoped<IVnPayService, VnPayService>();
            services.AddScoped<MassRefundService>();
            // Public Api
            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            // app.UseMigration<UserModuleDbContext>();
            return app;
        }
    }
}



