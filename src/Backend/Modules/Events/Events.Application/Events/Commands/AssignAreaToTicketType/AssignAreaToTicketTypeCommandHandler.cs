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
        if (command.Mappings is null || command.Mappings.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "TicketTypeAreaMapping.Empty",
                "At least one ticket type-area mapping is required."));
        }

        var @event = await eventRepository.GetByIdWithTicketTypesAndAreasAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var duplicateTicketTypeId = command.Mappings
            .GroupBy(x => x.TicketTypeId)
            .FirstOrDefault(g => g.Count() > 1)?
            .Key;

        if (duplicateTicketTypeId.HasValue)
        {
            return Result.Failure(Error.Validation(
                "TicketTypeAreaMapping.DuplicateTicketType",
                $"Ticket type '{duplicateTicketTypeId.Value}' appears more than once."));
        }

        foreach (var mapping in command.Mappings)
        {
            var ticketType = @event.TicketTypes.FirstOrDefault(t => t.Id == mapping.TicketTypeId);
            if (ticketType is null)
                return Result.Failure(EventErrors.TicketTypeErrors.NotFound(mapping.TicketTypeId));

            var area = @event.Areas.FirstOrDefault(a => a.Id == mapping.AreaId);
            if (area is null)
                return Result.Failure(EventErrors.TicketTypeErrors.AreaNotBelongToEvent(mapping.AreaId, command.EventId));

            ticketType.AssignArea(mapping.AreaId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}