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
using Shared.Infrastructure.Data.Interceptors;
using Shared.Infrastructure.UnitOfWork;
using Users.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
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
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventInterceptor>();
            services.AddDbContext<UserDbContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

                var interceptors = sp.GetServices<ISaveChangesInterceptor>().ToArray();
                if (interceptors.Any())
                {
                    options.AddInterceptors(interceptors);
                }
            });

            // OPTION 1: Register all repositories by marker interface (RECOMMENDED)
            // Requires: Create IRepository marker interface in Domain layer
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes
                    .AssignableTo(typeof(IRepository<>))) // Generic repository interface
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // OPTION 2: Register by naming convention
            // services.Scan(scan => scan
            //     .FromAssemblies(infrasAssembly)
            //     .AddClasses(classes => classes
            //         .Where(type => type.Name.EndsWith("Repository") && !type.IsAbstract))
            //     .AsImplementedInterfaces()

            // OPTION 3: Register by base class
            // Requires: Create RepositoryBase<T> base class
            // services.Scan(scan => scan
            //     .FromAssemblies(infrasAssembly)
            //     .AddClasses(classes => classes
            //         .AssignableTo(typeof(RepositoryBase<>)))
            //     .AsImplementedInterfaces()

            // OPTION 4: Register by specific namespace pattern
            // services.Scan(scan => scan
            //     .FromAssemblies(infrasAssembly)
            //     .AddClasses(classes => classes
            //         .InNamespaces("ClothingStore.Infrastructure.Repositories"))
            //     .AsImplementedInterfaces()

            // Scrutor: Auto-register all IDbContextUnitOfWork implementations
            services.Scan(scan => scan
                .FromAssemblies(infrasAssembly)
                .AddClasses(classes => classes
                    .AssignableTo<IDbContextUnitOfWork>()
                    .Where(type => !type.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Manually register CompositeUnitOfWork (avoid circular dependency)
            services.AddScoped<ICompositeUnitOfWork, CompositeUnitOfWork>();

           
            //shared application 
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

           

            // Controllers + model binders
            services.AddHttpContextAccessor();
            // services.AddControllers(options =>
            // {
            //     options.ModelBinderProviders.Insert(0, new CurrentUserModelBinderProvider());
            // });

            

            // // Exception handling
            // services.AddExceptionHandler<CustomExceptionHandler>();
            services.AddProblemDetails();
            
            return services;
        }
    }
}