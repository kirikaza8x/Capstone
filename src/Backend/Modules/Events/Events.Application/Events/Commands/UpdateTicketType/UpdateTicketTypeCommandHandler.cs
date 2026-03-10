using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.Events.Commands.UpdateTicketType;

public sealed class UpdateTicketTypeCommandValidator : AbstractValidator<UpdateTicketTypeCommand>
{
    public UpdateTicketTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(200).WithMessage("Ticket type name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
    }
}

internal sealed class UpdateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateTicketTypeCommand>
{
    public async Task<Result> Handle(UpdateTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var session = await eventRepository.GetEventSessionWithTicketTypesAsync(command.SessionId, cancellationToken);

        if (session is null)
            return Result.Failure(EventSessionErrors.NotFound(command.SessionId));

        var @event = await eventRepository.GetByIdAsync(session.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(session.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var ticketType = session.GetTicketType(command.TicketTypeId);

        if (ticketType is null)
            return Result.Failure(TicketTypeErrors.NotFound(command.TicketTypeId));

        ticketType.Update(command.Name, command.Price, command.Quantity);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}