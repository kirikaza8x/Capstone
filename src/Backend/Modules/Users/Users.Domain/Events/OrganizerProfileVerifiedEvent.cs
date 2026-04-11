using Shared.Domain.DDD;

namespace Users.Domain.Events
{
    public record OrganizerProfileVerifiedEvent(Guid UserId) : DomainEventBase
    {

    }
}
