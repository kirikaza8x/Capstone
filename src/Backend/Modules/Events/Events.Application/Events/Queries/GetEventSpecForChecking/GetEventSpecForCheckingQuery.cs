using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Queries.GetEventSpecForChecking;

public sealed record GetEventSpecForCheckingQuery(
    Guid EventId) : IQuery<GetEventSpecForCheckingResponse>;

public sealed record GetEventSpecForCheckingResponse(
    string? Spec,     
    string? SpecImage);
