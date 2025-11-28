using MediatR;

namespace Shared.Domain.Common.DDD;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
