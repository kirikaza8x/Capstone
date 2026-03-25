using Shared.Domain.DDD;

namespace Marketing.Domain.Events;

public record PostCreatedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId,
    DateTime CreatedAt) : DomainEventBase;

public record PostSubmittedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId,
    DateTime SubmittedAt) : DomainEventBase;

public record PostApprovedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId,
    Guid AdminId,
    DateTime ApprovedAt) : DomainEventBase;

public record PostRejectedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId,
    Guid AdminId,
    string Reason,
    DateTime RejectedAt) : DomainEventBase;

public record PostPublishedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId,
    DateTime PublishedAt) : DomainEventBase;

public record PostArchivedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid OrganizerId) : DomainEventBase;

/// <summary>
/// Raised when Admin force-removes a published post for policy violation.
/// Distinct from organizer-initiated Archive so consumers can react differently
/// (e.g. send a different notification to the organizer).
/// </summary>
public record PostForceRemovedDomainEvent(
    Guid PostId,
    Guid @eventId,
    Guid AdminId,
    string Reason,
    DateTime RemovedAt) : DomainEventBase;