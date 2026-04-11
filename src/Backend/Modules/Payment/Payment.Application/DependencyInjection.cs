
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application;

namespace Payments.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
        {
            services.AddApplication(new[]
            {
                PaymentsApplicationAssemblyReference.Assembly,
                typeof(ApplicationConfiguration).Assembly
            });
            return services;
        }
    }
}

