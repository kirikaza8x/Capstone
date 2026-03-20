using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure.Configs;
using Notifications.Infrastructure.Services;
using Shared.Application.Abstractions.Notifications;

namespace Notifications.Infrastructure;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName));

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }

    public static IApplicationBuilder UseNotificationModule(this IApplicationBuilder app)
    {
        return app;
    }
}
