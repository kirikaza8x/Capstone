using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Roles.Domain.UOW;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Shared.Domain.Data.Repositories;
using Shared.Infrastructure.Configs;
using Shared.Infrastructure.Configs.Database;
using Shared.Infrastructure.Data.Seeds;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Service.Authentication;
using Shared.Infrastructure.Service.Report;
using System.Text;
using Users.Application.Abstractions.Authentication;
using Users.Application.Abstractions.Sms;
using Users.Domain.Entities;
using Users.Domain.UOW;
using Users.Infrastructure.Data.UOW;
using Users.Infrastructure.Persistence.Contexts;
using Users.Infrastructure.PublicApi;
using Users.Infrastructure.Services.Report;
using Users.Infrastructure.Services.Authentication;
using Users.Infrastructure.Services.Sms;
using Users.PublicApi.Services;

namespace Users.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Scan(scan => scan
                .FromAssemblyOf<UsersInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo<ConfigBase>())
                .AsSelf()
                .WithSingletonLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<UsersInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(scan => scan
                .FromAssemblyOf<UsersInfrastructureAssemblyReference>()
                .AddClasses(classes => classes.AssignableTo(typeof(IDataSeeder<>)))
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
                // token validation
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // custom events for handling authentication and authorization failures
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        var error = Error.Unauthorized("Authentication.Failed", "You are not authenticated.");
                        var result = Result.Failure(error);

                        await CustomResults.Problem(result, context.HttpContext)
                            .ExecuteAsync(context.HttpContext);
                    },
                    OnForbidden = async context =>
                    {
                        var error = Error.Forbidden("Authorization.Failed", "You do not have permission to perform this action.");
                        var result = Result.Failure(error);

                        await CustomResults.Problem(result, context.HttpContext)
                            .ExecuteAsync(context.HttpContext);
                    }
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

            services.AddScoped<ISheetMappings<User>, UserExcelMappings>();
            services.AddScoped<IFileImportExportService<User>>(sp =>
            {
                var mappings = sp.GetRequiredService<ISheetMappings<User>>();
                return new ClosedXmlImportExportService<User>(
                    mappings.GetRowMapper(),
                    mappings.Exporter
                );
            });

            services.AddScoped<IUserNotificationService, GmailNotificationService>();
            services.AddScoped<IUserUnitOfWork, UserUnitOfWork>();
            services.AddScoped<IRoleUnitOfWork, RoleUnitOfWork>();
            services.AddScoped<IGooglePayloadValidator, GooglePayloadValidatorService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();

            // Public Api
            services.AddScoped<IUserPublicApi, UserPublicApi>();
            return services;
        }

        public static IApplicationBuilder UseUserInfrastructure(this IApplicationBuilder app)
        {
            app.UseMigration<UserModuleDbContext>();
            return app;
        }
    }
}



