
using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.CreateTicketType;

public sealed class CreateTicketTypeValidator : AbstractValidator<CreateTicketTypeCommand>
{
    public CreateTicketTypeValidator()
    {
        RuleFor(x => x.EventSessionId)
            .NotEmpty().WithMessage("Event session ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ticket type name is required.")
            .MaximumLength(200).WithMessage("Ticket type name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid ticket allocation type.");

        RuleFor(x => x.AreaId)
            .NotEmpty()
            .When(x => x.Type == AreaType.Seat)
            .WithMessage("Area ID is required when ticket type is Seat.");
    }
}

internal sealed class CreateTicketTypeCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateTicketTypeCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketTypeCommand command, CancellationToken cancellationToken)
    {
        // Check if event session exists
        var session = await eventRepository.GetEventSessionByIdAsync(command.EventSessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<Guid>(EventErrors.EventSession.NotFound(command.EventSessionId));
        }

        // Create ticket type
        var ticketType = TicketType.Create(
            command.EventSessionId,
            command.Name,
            command.Price,
            command.Quantity,
            command.Type,
            command.AreaId);

        session.AddTicketType(ticketType);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ticketType.Id);
    }
}
