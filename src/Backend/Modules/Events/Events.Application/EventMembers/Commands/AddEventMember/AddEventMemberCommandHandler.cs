using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.PublicApi.PublicApi;

namespace Events.Application.EventMembers.Commands.AddEventMember;

internal sealed class AddEventMemberCommandHandler(
    IEventRepository eventRepository,
    IUserPublicApi userPublicApi,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<AddEventMemberCommand, Guid>
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

        var existingMember = @event.Members.FirstOrDefault(m => m.UserId == userInfo.Id);
        if (existingMember is not null)
        {
            if (existingMember.Status == EventMemberStatus.Pending)
                return Result.Failure<Guid>(EventErrors.EventMemberErrors.AlreadyInvited(command.Email));

            if (existingMember.Status == EventMemberStatus.Active)
                return Result.Failure<Guid>(EventErrors.EventMemberErrors.AlreadyExists(command.Email));
        }

        var member = @event.InviteMember(
                    userInfo.Id,
                    command.Email,
                    command.Permissions,
                    currentUserService.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(member.Id);
    }
}
