using Events.Application.Abstractions.Caching;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventMembers.Commands.RemoveEventMember;

internal sealed class RemoveEventMemberCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork,
    IEventMemberPermissionCacheInvalidator permissionCacheInvalidator) : ICommandHandler<RemoveEventMemberCommand>
{
    public async Task<Result> Handle(RemoveEventMemberCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithMembersAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var member = @event.Members.FirstOrDefault(m => m.Id == command.MemberId);
        if (member is null)
            return Result.Failure(EventErrors.EventMemberErrors.NotFound(command.MemberId));

        var memberUserId = member.UserId;

        @event.RemoveMember(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await permissionCacheInvalidator.InvalidateAsync(command.EventId, memberUserId, cancellationToken);

        return Result.Success();
    }
}
