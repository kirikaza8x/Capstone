using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Queries.GetEventSpec;

public sealed record GetEventSpecQuery(
    Guid EventId,
    Guid EventSessionId) : IQuery<GetEventSpecResponse>;

public sealed record GetEventSpecResponse(
    Guid EventId,
    Guid EventSessionId,
    string? Spec);
