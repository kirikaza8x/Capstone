using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.EventSessions.Commands.DeleteEventSession;

internal sealed class DeleteEventSessionCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteEventSessionCommand>
{
    public async Task<Result> Handle(DeleteEventSessionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithSessionsAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(Event.NotOwner);

        var session = @event.Sessions.FirstOrDefault(s => s.Id == command.SessionId);

        if (session is null)
            return Result.Failure(EventSessionErrors.NotFound(command.SessionId));

        @event.RemoveSession(session);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
