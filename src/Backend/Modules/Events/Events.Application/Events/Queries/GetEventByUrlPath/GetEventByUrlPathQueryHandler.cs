using System.Text.Json;
using AI.IntegrationEvents.IntergrationEvents;
using AI.PublicApi.Enums;
using AutoMapper;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;

namespace Events.Application.Events.Queries.GetEventByUrlPath;

internal sealed class GetEventByUrlPathQueryHandler(
    IEventRepository eventRepository,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IEventBus eventBus) : IQueryHandler<GetEventByUrlPathQuery, GetEventByUrlPathResponse>
{
    public async Task<Result<GetEventByUrlPathResponse>> Handle(
        GetEventByUrlPathQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByUrlPathAsync(
            query.UrlPath,
            cancellationToken);

        if (@event is null)
            return Result.Failure<GetEventByUrlPathResponse>(
                EventErrors.Event.NotFoundByUrlPath(query.UrlPath));

        var response = mapper.Map<GetEventByUrlPathResponse>(@event);

        var userId = currentUserService.UserId;
        var isBehaviorActor = userId != Guid.Empty &&
            (currentUserService.Roles.Contains(Roles.Attendee) ||
             currentUserService.Roles.Contains(Roles.Organizer));

        if (isBehaviorActor)
        {
            var trackEvent = TrackUserActivityIntegrationEvent.Create(
                userId: userId,
                actionType: ActionTypes.View,
                targetId: @event.Id.ToString(),
                targetType: TargetType.Event,
                metadata: new Dictionary<string, string>
                {
                    ["urlPath"] = query.UrlPath,
                    ["categories"] = JsonSerializer.Serialize(@event.EventCategories.Select(c => c.Category.Name).ToList()),
                    ["hashtags"] = JsonSerializer.Serialize(@event.EventHashtags.Select(h => h.Hashtag.Name).ToList()),
                });

            await eventBus.PublishAsync(trackEvent, cancellationToken);
        }

        return Result.Success(response);
    }
}
