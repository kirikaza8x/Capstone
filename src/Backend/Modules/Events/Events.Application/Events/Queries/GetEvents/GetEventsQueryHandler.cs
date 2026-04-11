using AI.IntegrationEvents.IntergrationEvents;
using AI.PublicApi.Enums;
using AutoMapper;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Caching;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.PublicApi.Constants;

namespace Events.Application.Events.Queries.GetEvents;

internal sealed class GetEventsQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICacheService cacheService,
    ICurrentUserService currentUserService,
    IEventBus eventBus) : IQueryHandler<GetEventsQuery, PagedResult<EventResponse>>
{
    public async Task<Result<PagedResult<EventResponse>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var pagedEvents = await eventRepository.GetPublishedWithCategoriesAsync(
            request,
            request.CategoryId,
            cancellationToken);

        var responseItems = mapper.Map<IReadOnlyList<EventResponse>>(pagedEvents.Items);

        var result = PagedResult<EventResponse>.Create(
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
                targetId: TrackingTargetIds.EventList,
                targetType: TargetType.EventList,
                metadata: new Dictionary<string, string>
                {
                    ["page"] = request.PageNumber.ToString(),
                    ["pageSize"] = request.PageSize.ToString(),
                    ["categoryId"] = request.CategoryId?.ToString() ?? string.Empty,
                    ["resultCount"] = result.Items.Count.ToString()
                });

            await eventBus.PublishAsync(trackEvent, cancellationToken);
        }

        return Result.Success(result);
    }
}
