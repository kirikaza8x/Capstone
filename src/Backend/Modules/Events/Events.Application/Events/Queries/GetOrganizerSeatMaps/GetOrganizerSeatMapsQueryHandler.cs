using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Queries.GetOrganizerSeatMaps;

internal sealed class GetOrganizerSeatMapsQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetOrganizerSeatMapsQuery, IReadOnlyList<OrganizerSeatMapResponse>>
{
    public async Task<Result<IReadOnlyList<OrganizerSeatMapResponse>>> Handle(
        GetOrganizerSeatMapsQuery query,
        CancellationToken cancellationToken)
    {
        var organizerId = currentUserService.UserId;

        var events = await eventRepository.GetByOrganizerIdAsync(organizerId, cancellationToken);

        var result = events
            .Where(e => !string.IsNullOrWhiteSpace(e.Spec))
            .Select(e => new OrganizerSeatMapResponse(e.Id, e.Title, e.Spec))
            .ToList();

        return Result.Success<IReadOnlyList<OrganizerSeatMapResponse>>(result);
    }
}
