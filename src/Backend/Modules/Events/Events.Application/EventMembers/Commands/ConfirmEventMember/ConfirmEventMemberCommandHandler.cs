using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventMembers.Commands.ConfirmEventMember;

internal sealed class ConfirmEventMemberCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<ConfirmEventMemberCommand>
{
    public async Task<Result> Handle(ConfirmEventMemberCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithMembersAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        var member = @event.Members.FirstOrDefault(m => m.Id == command.MemberId);
        if (member is null)
            return Result.Failure(EventErrors.EventMemberErrors.NotFound(member.Id));

        if (member.UserId != currentUserService.UserId)
        {
            return Result.Failure(Error.Forbidden(
                "EventMember.Confirm.Forbidden",
                "You are not authorized to confirm this invitation."));
        }

        var result = member.Confirm();
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
