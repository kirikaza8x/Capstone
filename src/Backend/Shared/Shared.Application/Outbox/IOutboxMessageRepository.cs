namespace Shared.Application.Outbox;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedMessagesAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    void MarkAsProcessed(OutboxMessage message);

    void MarkAsFailed(OutboxMessage message, string error);
}