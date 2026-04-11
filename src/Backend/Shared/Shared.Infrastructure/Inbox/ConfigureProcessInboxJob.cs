using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Shared.Infrastructure.Inbox;

public sealed class ConfigureProcessInboxJob<TDbContext> : IConfigureOptions<QuartzOptions>
    where TDbContext : DbContext
{
    private readonly InboxOptions _inboxOptions;

    public ConfigureProcessInboxJob(IOptions<InboxOptions> inboxOptions)
    {
        _inboxOptions = inboxOptions.Value;
    }

    public void Configure(QuartzOptions options)
    {
        var moduleName = typeof(TDbContext).Name.Replace("DbContext", "");
        var jobName = $"{moduleName}.ProcessInbox";

        options
            .AddJob<ProcessInboxJob<TDbContext>>(configure =>
                configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInSeconds(_inboxOptions.IntervalInSeconds)
                            .RepeatForever()));
    }
}
