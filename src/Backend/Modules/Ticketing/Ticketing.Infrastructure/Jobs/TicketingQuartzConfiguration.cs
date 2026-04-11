using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Ticketing.Infrastructure.Jobs;

internal static class TicketingQuartzConfiguration
{
    private const int ScanIntervalMinutes = 1;

    public static IServiceCollection AddTicketingQuartzJobs(this IServiceCollection services)
    {
        var jobKey = new JobKey("ticketing.cancel-expired-pending-orders");

        services.AddQuartz(options =>
        {
            options.AddJob<CancelExpiredPendingOrdersJob>(job => job
                .WithIdentity(jobKey)
                .StoreDurably());

            options.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("ticketing.cancel-expired-pending-orders.trigger")
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(ScanIntervalMinutes)
                    .RepeatForever()));
        });

        return services;
    }
}
