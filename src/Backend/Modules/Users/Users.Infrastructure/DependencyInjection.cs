using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Application.Abstractions.Authentication;
using Shared.Domain.Data;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Configs.Security;
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.Extensions;
using Users.Application.Abstractions.Authentication;
using Users.Domain.UOW;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Data.UOW;
using Users.Infrastructure.Persistence.Contexts;

namespace Users.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            // Register all config classes inheriting from ConfigBase
            services.Scan(scan => scan
                .FromAssemblyOf<UsersInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            // Register binder once for all configs inheriting ConfigBase
            services.AddTransient(typeof(IConfigureOptions<>), typeof(ConfigurationBinderSetup<>));

            services.Scan(scan => scan
                .FromAssemblyOf<AuditableEntityInterceptor>()
                .AddClasses(classes => classes.AssignableTo<ISaveChangesInterceptor>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddScoped<IUserUnitOfWork, UserUnitOfWork>();
            services.Scan(scan => scan
                .FromAssemblyOf<UsersInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddDbContext<UserModuleDbContext>((sp, options) =>
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
            services.AddScoped<IGooglePayloadValidator, GooglePayloadValidatorService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            services.AddHttpContextAccessor();
            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            app.UseMigration<UserModuleDbContext>();
            return app;
        }
    }
}
