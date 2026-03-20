using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.CreateTicketType;

public sealed class CreateTicketTypeValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(200).WithMessage("Ticket type name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");
    }
}

internal sealed class CreateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateTicketTypeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketTypeCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<Guid>(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure<Guid>(EventErrors.Event.NotOwner);

        var ticketType = TicketType.Create(command.EventId, command.Name, command.Quantity, command.Price);

        @event.AddTicketType(ticketType);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ticketType.Id);
    }
}
