using MediatR;
using Shared.Domain.Common.DDD;

namespace Shared.Application.Abstractions.Messaging
{
    public interface IEventHandler<TEvent> : INotificationHandler<TEvent>
        where TEvent : IDomainEvent
    {
    }
}
