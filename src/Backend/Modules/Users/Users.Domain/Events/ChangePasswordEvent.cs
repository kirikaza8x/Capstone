using Shared.Domain.DDD;

namespace Users.Domain.Events
{
    public record OtpCreatedEvent(Guid UserId, string OtpCode) : DomainEventBase;
    public record PasswordChangedEvent(Guid UserId) : DomainEventBase;
}
