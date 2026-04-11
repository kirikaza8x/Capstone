using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.DeleteTicketType;

internal sealed class DeleteTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteTicketTypeCommand>
{
    public async Task<Result> Handle(DeleteTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var ticketType = @event.TicketTypes.FirstOrDefault(t => t.Id == command.TicketTypeId);

        if (ticketType is null)
            return Result.Failure(EventErrors.TicketTypeErrors.NotFound(command.TicketTypeId));

        @event.RemoveTicketType(ticketType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
