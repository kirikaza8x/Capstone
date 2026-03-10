using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.Events.Commands.DeleteTicketType;

internal sealed class DeleteTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteTicketTypeCommand>
{
    public async Task<Result> Handle(DeleteTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var session = await eventRepository.GetEventSessionWithTicketTypesAsync(command.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure(EventSessionErrors.NotFound(command.SessionId));

        var @event = await eventRepository.GetByIdAsync(session.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(Event.NotFound(session.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(Event.NotOwner);

        var ticketType = session.GetTicketType(command.TicketTypeId);

        if (ticketType is null)
            return Result.Failure(TicketTypeErrors.NotFound(command.TicketTypeId));

        session.RemoveTicketType(ticketType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}