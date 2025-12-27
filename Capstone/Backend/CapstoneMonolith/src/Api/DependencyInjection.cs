

using Shared.Presentation.Common.ModelBinder;
using Shared.Presentation.Configs.Swagger;
using Shared.Presentation.Handler;

namespace Users.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            // Register swagger gen with default v1 document
                    // services.AddSwaggerGen(options =>
                    // {
                    //     options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                    //     {
                    //         Title = "User API",
                    //         Version = "v1",
                    //         Description = "API documentation for User Service"
                    //     });
                    // });
            services.AddSwaggerGen();
            services.ConfigureOptions<SwaggerConfigSetup>();        
            services.AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new CurrentUserModelBinderProvider());
            });
            services.AddCors(options =>
           {
               options.AddPolicy("AllowAll", policy =>
               {
                   policy
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials();
               });
           });
            services.AddExceptionHandler<CustomExceptionHandler>();
            return services;
        }
    }
}