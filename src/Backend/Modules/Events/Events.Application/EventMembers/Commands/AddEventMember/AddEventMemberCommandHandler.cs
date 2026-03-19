using Events.Application.Abstractions.Caching;
using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;
using Users.PublicApi.Services;

namespace Events.Application.EventMembers.Commands.AddEventMember;

internal sealed class AddEventMemberCommandHandler(
    IEventRepository eventRepository,
    IUserPublicApi userPublicApi,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork,
    IEventMemberPermissionCacheInvalidator permissionCacheInvalidator) : ICommandHandler<AddEventMemberCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddEventMemberCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithMembersAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<Guid>(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure<Guid>(EventErrors.Event.NotOwner);

        var userInfo = await userPublicApi.GetByEmailAsync(command.Email, cancellationToken);

        if (userInfo is null)
            return Result.Failure<Guid>(EventErrors.EventMemberErrors.UserNotFound(command.Email));

        if (!userInfo.Roles.Contains(Roles.Attendee))
            return Result.Failure<Guid>(EventErrors.EventMemberErrors.UserNotEligible);

        var alreadyMember = @event.Members.Any(m => m.UserId == userInfo.Id);
        if (alreadyMember)
            return Result.Failure<Guid>(EventErrors.EventMemberErrors.AlreadyExists(command.Email));

        var member = EventMember.Create(
            command.EventId,
            userInfo.Id,
            command.Permissions,
            currentUserService.UserId);

        @event.AddMember(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await permissionCacheInvalidator.InvalidateAsync(command.EventId, userInfo.Id, cancellationToken);

        return Result.Success(member.Id);
    }
}
