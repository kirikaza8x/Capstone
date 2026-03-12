using Shared.Domain.DDD;

namespace Users.Domain.Events
{
    public record UserCreatedEvent(Guid UserId, string Email, string UserName) : DomainEventBase
    {

    }
}
