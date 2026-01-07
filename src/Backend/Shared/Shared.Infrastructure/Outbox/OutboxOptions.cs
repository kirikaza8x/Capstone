namespace Shared.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int IntervalInSeconds { get; set; } = 3;

    public int BatchSize { get; set; } = 30;

    public int MaxRetryCount { get; set; } = 3;
}