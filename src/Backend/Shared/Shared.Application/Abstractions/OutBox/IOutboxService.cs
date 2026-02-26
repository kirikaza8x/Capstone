namespace Shared.Application.Abstractions.Outbox;

public interface IOutboxService
{
    void Preserve<T>(T integrationEvent) where T : class;
}