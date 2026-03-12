using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.AssignAreaToTicketType;

internal sealed class AssignAreaToTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<AssignAreaToTicketTypeCommand>
{
    public async Task<Result> Handle(AssignAreaToTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAndAreasAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var ticketType = @event.TicketTypes.FirstOrDefault(t => t.Id == command.TicketTypeId);
        if (ticketType is null)
            return Result.Failure(EventErrors.TicketTypeErrors.NotFound(command.TicketTypeId));

        var area = @event.Areas.FirstOrDefault(a => a.Id == command.AreaId);
        if (area is null)
            return Result.Failure(EventErrors.TicketTypeErrors.AreaNotBelongToEvent(command.AreaId, command.EventId));

        ticketType.AssignArea(command.AreaId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}