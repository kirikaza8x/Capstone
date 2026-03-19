using Events.Application.EventMembers.Queries.GetEventMembers;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Services;

namespace Events.Application.Events.Queries.GetEventMembers;

internal sealed class GetEventMembersQueryHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IUserPublicApi userPublicApi) : IQueryHandler<GetEventMembersQuery, IReadOnlyList<EventMemberResponse>>
{
    public async Task<Result<IReadOnlyList<EventMemberResponse>>> Handle(
        GetEventMembersQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithMembersAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<IReadOnlyList<EventMemberResponse>>(EventErrors.Event.NotFound(query.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure<IReadOnlyList<EventMemberResponse>>(EventErrors.Event.NotOwner);

        var userIds = @event.Members.Select(m => m.UserId).Distinct().ToList();
        var userInfos = await Task.WhenAll(
            userIds.Select(id => userPublicApi.GetByIdAsync(id, cancellationToken)));

        var userMap = userInfos
            .Where(u => u is not null)
            .ToDictionary(u => u!.Id, u => u!);

        var response = @event.Members.Select(m =>
        {
            userMap.TryGetValue(m.UserId, out var user);
            return new EventMemberResponse(
                m.Id,
                m.UserId,
                user?.FullName ?? string.Empty,
                user?.Email ?? string.Empty,
                m.Permissions,
                m.Status);
        }).ToList();

        return Result.Success<IReadOnlyList<EventMemberResponse>>(response);
    }
}
