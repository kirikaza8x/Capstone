using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventMembers.Commands.UpdateEventMemberPermissions;

internal sealed class UpdateEventMemberPermissionsCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventMemberPermissionsCommand>
{
    public async Task<Result> Handle(UpdateEventMemberPermissionsCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithMembersAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var member = @event.Members.FirstOrDefault(m => m.Id == command.MemberId);
        if (member is null)
            return Result.Failure(EventErrors.EventMemberErrors.NotFound(command.MemberId));

        member.UpdatePermissions(command.Permissions);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}