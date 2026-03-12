using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.SessionTicketQuotas.Commands.SetSessionTicketQuota;

public sealed class SetSessionTicketQuotaValidator : AbstractValidator<SetSessionTicketQuotaCommand>
{
    public SetSessionTicketQuotaValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}

internal sealed class SetSessionTicketQuotaCommandHandler(
    IEventRepository eventRepository,
    ISessionTicketQuotaRepository quotaRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<SetSessionTicketQuotaCommand>
{
    public async Task<Result> Handle(SetSessionTicketQuotaCommand command, CancellationToken cancellationToken)
    {
        // Validate event ownership
        var @event = await eventRepository.GetByIdWithTicketTypesAndAreasAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var session = await eventRepository.GetEventSessionByIdAsync(command.SessionId, cancellationToken);
        if (session is null || session.EventId != command.EventId)
            return Result.Failure(EventErrors.EventSessionErrors.NotFound(command.SessionId));

        // Validate ticket type belongs to event and has Zone type
        var ticketType = @event.TicketTypes.FirstOrDefault(t => t.Id == command.TicketTypeId);
        if (ticketType is null)
            return Result.Failure(EventErrors.TicketTypeErrors.NotFound(command.TicketTypeId));

        var area = @event.Areas.FirstOrDefault(a => a.Id == ticketType.AreaId);
        if (area is null || area.Type != AreaType.Zone)
            return Result.Failure(EventErrors.SessionTicketQuotaErrors.TicketTypeNotZone());

        // Upsert quota
        var existing = await quotaRepository.GetBySessionAndTicketTypeAsync(
            command.SessionId, command.TicketTypeId, cancellationToken);

        if (existing is null)
        {
            var quota = SessionTicketQuota.Create(command.SessionId, command.TicketTypeId, command.Quantity);
            quotaRepository.Add(quota);
        }
        else
        {
            existing.UpdateQuantity(command.Quantity);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}