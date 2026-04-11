using Marketing.Domain.Enums;
using Shared.Domain.DDD;

namespace Marketing.Domain.Events;

public record PostQueuedForDistributionDomainEvent(
    Guid PostId,
    ExternalPlatform Platform,
    DateTime QueuedAt) : DomainEventBase;

public record PostDistributedToPlatformDomainEvent(
    Guid PostId,
    ExternalPlatform Platform,
    string ExternalUrl,
    DateTime DistributedAt) : DomainEventBase;