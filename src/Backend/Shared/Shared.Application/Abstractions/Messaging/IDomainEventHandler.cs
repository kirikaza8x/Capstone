using MediatR;
using Shared.Domain.DDD;

namespace Shared.Application.Abstractions.Messaging;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}
