using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Events.Infrastructure.Jobs;

public static class EventsQuartzConfiguration
{
    private const int AutoCompleteIntervalMinutes = 5;

    public static IServiceCollection AddEventsQuartzJobs(this IServiceCollection services)
    {
        var autoCompleteJobKey = new JobKey("events.auto-complete-published");
        var reminderJobKey = new JobKey("events.send-reminder-24h");

        services.AddQuartz(options =>
        {
            options.AddJob<AutoCompletePublishedEventsJob>(job => job
                .WithIdentity(autoCompleteJobKey)
                .StoreDurably());

            options.AddTrigger(trigger => trigger
                .ForJob(autoCompleteJobKey)
                .WithIdentity("events.auto-complete-published.trigger")
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(AutoCompleteIntervalMinutes)
                    .RepeatForever()));

            options.AddJob<SendEventReminderJob>(job => job
                .WithIdentity(reminderJobKey)
                .StoreDurably());

            options.AddTrigger(trigger => trigger
                .ForJob(reminderJobKey)
                .WithIdentity("events.send-reminder-24h.trigger")
                .StartNow()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(AutoCompleteIntervalMinutes)
                    .RepeatForever()));
        });

        return services;
    }
}