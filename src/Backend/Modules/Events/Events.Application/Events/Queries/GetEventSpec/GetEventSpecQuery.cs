using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Queries.GetEventSpec;

public sealed record GetEventSpecQuery(Guid EventId) : IQuery<GetEventSpecResponse>;

public sealed record GetEventSpecResponse(Guid EventId, string? Spec);
