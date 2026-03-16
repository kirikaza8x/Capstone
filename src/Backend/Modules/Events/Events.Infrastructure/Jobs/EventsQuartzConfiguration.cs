using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Events.Infrastructure.Jobs;

public static class EventsQuartzConfiguration
{
    private const int AutoCompleteIntervalMinutes = 5;

    public static IServiceCollection AddEventsQuartzJobs(this IServiceCollection services)
    {
        var jobKey = new JobKey("events.auto-complete-published");

        services.AddQuartz(options =>
        {
            options.AddJob<AutoCompletePublishedEventsJob>(job => job
                .WithIdentity(jobKey)
                .StoreDurably());

            options.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("events.auto-complete-published.trigger")
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(AutoCompleteIntervalMinutes)
                    .RepeatForever()));
        });

        return services;
    }
}