using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Queries.GetOrganizerSeatMaps;

public sealed record GetOrganizerSeatMapsQuery : IQuery<IReadOnlyList<OrganizerSeatMapResponse>>;

public sealed record OrganizerSeatMapResponse(
    Guid EventId,
    string Title,
    string? Spec);
