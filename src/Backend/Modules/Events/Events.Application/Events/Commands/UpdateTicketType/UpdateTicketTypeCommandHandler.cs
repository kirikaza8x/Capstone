using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

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
    }
}

internal sealed class UpdateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateTicketTypeCommand>
{
    public async Task<Result> Handle(UpdateTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var ticketType = @event.TicketTypes.FirstOrDefault(t => t.Id == command.TicketTypeId);

        if (ticketType is null)
            return Result.Failure(EventErrors.TicketTypeErrors.NotFound(command.TicketTypeId));

        ticketType.Update(command.Name, command.Quantity, command.Price);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
