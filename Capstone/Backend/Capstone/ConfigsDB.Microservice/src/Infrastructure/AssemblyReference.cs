using ConfigsDB.Infrastructure.Persistence.Contexts;
using Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Repositories;
using Shared.Domain.UnitOfWork;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Common;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Configs.Security;
using Shared.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
namespace ConfigsDB.Domain
{
    public static class InfrastructureAssemblyReference { }
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var infrasAssembly = typeof(InfrastructureAssemblyReference).Assembly;
            // Configuration options
            services.ConfigureOptions<ErrorHandlingConfigSetup>();
            services.ConfigureOptions<DatabaseConfigSetup>();
            services.ConfigureOptions<JwtConfigSetup>();

            // EF Core + interceptors
            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddDbContext<ConfigSettingDbContext>((sp, options) =>
            {
                var dbConfig = sp.GetRequiredService<
                IOptions<DatabaseConfig>>().Value;
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
                });

                var interceptors = sp.GetServices<ISaveChangesInterceptor>().ToArray();
                if (interceptors.Any())
                {
                    options.AddInterceptors(interceptors);
                }
            });
            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Configure JWT options after authentication is added
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtConfigs>>((options, jwtConfigs) =>
                {
                    var jwtSettings = jwtConfigs.Value;
                    var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
                    options.TokenValidationParameters.ValidIssuer = jwtSettings.Issuer;
                    options.TokenValidationParameters.ValidAudience = jwtSettings.Audience;
                });

            // Register all repositories by marker interface (RECOMMENDED)
            // Requires: Create IRepository marker interface in Domain layer
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IRepository<>))) // Generic repository interface
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Scrutor: Auto-register all IDbContextUnitOfWork implementations
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes
                    .AssignableTo<IDbContextUnitOfWork>()
                    .Where(type => !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Scrutor: Auto-register all IBulkOperationRepository implementations
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IBulkOperationRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());


            // Manually register CompositeUnitOfWork 
            services.AddScoped<ICompositeUnitOfWork, CompositeUnitOfWork>();
            //shared application 

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
            services.AddProblemDetails();

            return services;
        }
    }
}
