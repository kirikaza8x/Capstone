using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;

namespace Events.Application.EventMembers.Queries.GetAssignedEvents;

internal class GetAssignedEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetAssignedEventsQuery, IReadOnlyCollection<AssignedEventResponse>>
{
    public async Task<Result<IReadOnlyCollection<AssignedEventResponse>>> Handle(
        GetAssignedEventsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var isOrganizer = currentUserService.Roles.Contains(Roles.Organizer);

        var events = isOrganizer
            ? await eventRepository.GetPublishedUpcomingOrOngoingByOrganizerAsync(
                userId,
                DateTime.UtcNow,
                cancellationToken)
            : await eventRepository.GetAssignedEventsAsync(userId, cancellationToken);

        var response = mapper.Map<IReadOnlyCollection<AssignedEventResponse>>(events);

        return Result.Success(response);
    }
}

