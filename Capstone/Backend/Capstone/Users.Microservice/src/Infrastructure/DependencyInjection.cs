using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

using Shared.Application.Abstractions.Authentication;
using Shared.Application.Events;
using Shared.Authentication;
using Shared.Domain.Repositories;
using Shared.Domain.UnitOfWork;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Common;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Configs.Security;
using Shared.Infrastructure.Configs.MessageBus; // Added
using Shared.Infrastructure.Events;
using Shared.Infrastructure.UnitOfWork;

using Users.Infrastructure.Persistence.Contexts;
using Infrastructure.Data.Interceptors;
using System.ComponentModel.DataAnnotations;

namespace Users.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var infrasAssembly = typeof(InfrastructureAssemblyReference).Assembly;

            // --- Options ---
            services.ConfigureOptions<ErrorHandlingConfigSetup>();
            services.ConfigureOptions<JwtConfigSetup>();
            services.ConfigureOptions<DatabaseConfigSetup>();
            services.ConfigureOptions<RabbitMqConfigSetup>(); // Added

            // --- Authentication (JWT) ---
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

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtConfigs>>((options, jwtConfigs) =>
                {
                    var jwtSettings = jwtConfigs.Value;
                    var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
                    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
                    options.TokenValidationParameters.ValidIssuer = jwtSettings.Issuer;
                    options.TokenValidationParameters.ValidAudience = jwtSettings.Audience;
                });

            // --- Database (EF Core) ---
            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddDbContext<UserDbContext>((sp, options) =>
            {
                var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
                options.UseNpgsql(dbConfig.ConnectionString, npgsql =>
                {
                    if (dbConfig.MaxRetryCount > 0) npgsql.EnableRetryOnFailure(dbConfig.MaxRetryCount);
                    if (dbConfig.CommandTimeout > 0) npgsql.CommandTimeout(dbConfig.CommandTimeout);
                });

                var interceptors = sp.GetServices<ISaveChangesInterceptor>().ToArray();
                if (interceptors.Any()) options.AddInterceptors(interceptors);
            });

            // --- Repositories & Unit of Work ---
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes
                    .AssignableTo<IDbContextUnitOfWork>()
                    .Where(type => !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddScoped<ICompositeUnitOfWork, CompositeUnitOfWork>();

            // --- Shared Services ---
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
            services.AddProblemDetails();

            // --- Messaging (MassTransit + RabbitMQ) ---
            services.AddScoped<IServiceBusPublisher, MassTransitServiceBusPublisher>();

            services.AddMassTransit(x =>
        {
            // Register all consumers in this assembly
            x.AddConsumers(typeof(DependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var logger = context.GetRequiredService<ILoggerFactory>().CreateLogger("RabbitMQSetup");

                // --- Added Diagnostic Logger ---
                var rawConfig = context.GetRequiredService<IConfiguration>();
                logger.LogWarning("DIAGNOSTIC - Raw Config: Host={RawHost}, User={RawUser}",
                    rawConfig["RabbitMQ:Host"] ?? "NULL",
                    rawConfig["RabbitMQ:Username"] ?? "NULL");
                // -------------------------------

                var rabbitSettings = context.GetRequiredService<IOptions<RabbitMqConfigs>>().Value;

                var host = rabbitSettings.Host;
                var serviceName = configuration["MassTransit:ServiceName"] ?? "UserService";

                logger.LogInformation("[RabbitMQ] Connecting to {Host}:{Port} as User: {User}",
                    host, rabbitSettings.Port, rabbitSettings.Username);

                cfg.Host(host, rabbitSettings.Port, "/", h =>
                {
                    h.Username(rabbitSettings.Username);
                    h.Password(rabbitSettings.Password);

                    h.Heartbeat(TimeSpan.FromSeconds(10));

                    h.PublisherConfirmation = true;
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                // RETRY POLICY - Handles transient failures
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseMessageRetry(r =>
                {
                    r.Incremental(
                        retryLimit: 5,
                        initialInterval: TimeSpan.FromSeconds(1),
                        intervalIncrement: TimeSpan.FromSeconds(2)
                    );

                    r.Ignore<ArgumentException>();
                    r.Ignore<InvalidOperationException>();
                    r.Ignore<ValidationException>();
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                // CIRCUIT BREAKER - Prevents cascading failures
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                // RATE LIMITING - Protects downstream services
                // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                cfg.UseRateLimit(1000, TimeSpan.FromSeconds(1));

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceName, false));
            });
        });

            return services;
        }
    }
}