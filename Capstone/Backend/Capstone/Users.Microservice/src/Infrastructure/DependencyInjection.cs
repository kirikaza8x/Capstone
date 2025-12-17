using System.Text;
using Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;
using Shared.Authentication;
using Shared.Domain.Repositories;
using Shared.Domain.UnitOfWork;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Common;
using Shared.Infrastructure.Configs.Security;
using Shared.Infrastructure.UnitOfWork;
using Users.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Configs.Database;
using Shared.Application.Events;
using Shared.Infrastructure.Events;
using MassTransit;
using System.ComponentModel.DataAnnotations;
namespace Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            var infrasAssembly = typeof(InfrastructureAssemblyReference).Assembly;
            // Configuration options
            services.ConfigureOptions<ErrorHandlingConfigSetup>();
            services.ConfigureOptions<JwtConfigSetup>();
            services.ConfigureOptions<DatabaseConfigSetup>();
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

            // EF Core + interceptors
            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>() // or whichever assembly
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddDbContext<UserDbContext>((sp, options) =>
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

            // Manually register CompositeUnitOfWork 
            services.AddScoped<ICompositeUnitOfWork, CompositeUnitOfWork>();
            //shared application 
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
            services.AddProblemDetails();

            services.AddScoped<IServiceBusPublisher, MassTransitServiceBusPublisher>();
        
        // ⭐ NEW: Configure MassTransit with RabbitMQ
        services.AddMassTransit(x =>
        {
            // Register all consumers in this assembly
            x.AddConsumers(typeof(DependencyInjection).Assembly);
            
            // Optional: Register saga state machines
            // x.AddSagaStateMachine<UserRegistrationStateMachine, UserRegistrationState>()
            //     .InMemoryRepository();  // Use Redis/SQL in production
            
            x.UsingRabbitMq((context, cfg) =>
            {
                var serviceName = configuration["MassTransit:ServiceName"] ?? "UserService";
                
                // RabbitMQ connection
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "admin");
                    h.Password(configuration["RabbitMQ:Password"] ?? "admin123");
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                //  RETRY POLICY - Handles transient failures gracefully
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseMessageRetry(r => 
                {
                    r.Incremental(
                        retryLimit: 5,                          // Retry up to 5 times
                        initialInterval: TimeSpan.FromSeconds(1),
                        intervalIncrement: TimeSpan.FromSeconds(2)  // 1s, 3s, 5s, 7s, 9s
                    );
                    
                    // Don't retry on validation/business errors
                    r.Ignore<ArgumentException>();
                    r.Ignore<InvalidOperationException>();
                    r.Ignore<ValidationException>();
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                //  CIRCUIT BREAKER - Prevents cascading failures
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;              // Open if 15% of calls fail
                    cb.ActiveThreshold = 10;            // Minimum 10 attempts needed
                    cb.ResetInterval = TimeSpan.FromMinutes(5);  // Try again after 5 min
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                //  RATE LIMITING - Protects downstream services
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseRateLimit(1000, TimeSpan.FromSeconds(1));

                // Auto-configure endpoints from consumers
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceName, false));
            });
        });

            return services;
        }
    }
}