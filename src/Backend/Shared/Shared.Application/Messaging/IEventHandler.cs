using MediatR;
using Shared.Domain.DDD;

namespace Shared.Application.Messaging;

public interface IEventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
{
}
