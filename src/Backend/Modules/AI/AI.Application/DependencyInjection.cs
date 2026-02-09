
using AI.Application;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;

namespace Users.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AiAddApplication(this IServiceCollection services)
        {
            services.AddApplication(new[]
            {
                AiApplicationAssemblyReference.Assembly,
                typeof(ApplicationConfiguration).Assembly
            });
            return services;
        }
    }
}

