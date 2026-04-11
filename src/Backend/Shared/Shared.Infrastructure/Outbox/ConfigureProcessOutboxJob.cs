using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Shared.Infrastructure.Outbox;

public sealed class ConfigureProcessOutboxJob<TDbContext> : IConfigureOptions<QuartzOptions>
    where TDbContext : DbContext
{
    private readonly OutboxOptions _outboxOptions;

    public ConfigureProcessOutboxJob(IOptions<OutboxOptions> outboxOptions)
    {
        _outboxOptions = outboxOptions.Value;
    }

    public void Configure(QuartzOptions options)
    {
        var moduleName = typeof(TDbContext).Name.Replace("DbContext", "");
        var jobName = $"{moduleName}.ProcessOutbox";

        options
            .AddJob<ProcessOutboxJob<TDbContext>>(configure =>
                configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithIntervalInSeconds(_outboxOptions.IntervalInSeconds)
                            .RepeatForever()));
    }
}
