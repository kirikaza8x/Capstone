using Shared.Presentation.Common.ModelBinder;
using Shared.Presentation.Configs.Swagger;
using Shared.Presentation.Handler;

namespace ConfigsDB.Api
{
    public static class PresentationAssemblyReference { }
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureOptions<SwaggerConfigSetup>();
            services.AddSwaggerGen();
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
