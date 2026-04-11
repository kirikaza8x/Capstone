using AI.IntegrationEvents.IntergrationEvents;
using AI.PublicApi.Enums;
using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Application.Events.Queries.SearchEvents;

internal sealed class SearchEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IEventBus eventBus) : IQueryHandler<SearchEventsQuery, PagedResult<EventSearchResponse>>
{
    public async Task<Result<PagedResult<EventSearchResponse>>> Handle(
        SearchEventsQuery request,
        CancellationToken cancellationToken)
    {
        var pagedEvents = await eventRepository.SearchEventsAsync(
            request.Keyword,
            request,
            cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventSearchResponse>>(pagedEvents.Items);

        var result = PagedResult<EventSearchResponse>.Create(
            responseItems,
            pagedEvents.PageNumber,
            pagedEvents.PageSize,
            pagedEvents.TotalCount);

        var userId = currentUserService.UserId;
        var isBehaviorActor = userId != Guid.Empty &&
            (currentUserService.Roles.Contains(Roles.Attendee) ||
             currentUserService.Roles.Contains(Roles.Organizer));

        if (isBehaviorActor)
        {
            var trackEvent = TrackUserActivityIntegrationEvent.Create(
                userId: userId,
                actionType: ActionTypes.View,
                targetId: TrackingTargetIds.EventSearch,
                targetType: TargetType.EventList,
                metadata: new Dictionary<string, string>
                {
                    ["keyword"] = request.Keyword ?? string.Empty,
                    ["page"] = request.PageNumber.ToString(),
                    ["pageSize"] = request.PageSize.ToString(),
                    ["resultCount"] = result.Items.Count.ToString()
                });

            await eventBus.PublishAsync(trackEvent, cancellationToken);
        }

        return Result.Success(result);
    }
}
