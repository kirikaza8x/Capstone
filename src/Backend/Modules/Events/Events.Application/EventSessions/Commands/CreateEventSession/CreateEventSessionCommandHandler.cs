using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventSessions.Commands.CreateEventSession;

public sealed class CreateEventSessionValidator : AbstractValidator<CreateEventSessionCommand>
{
    public CreateEventSessionValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Session title is required.")
            .MaximumLength(500).WithMessage("Session title must not exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.")
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}

internal sealed class CreateEventSessionCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateEventSessionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventSessionCommand command, CancellationToken cancellationToken)
    {
        // Check if event exists
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure<Guid>(EventErrors.Event.NotFound(command.EventId));
        }

        // Create session
        var session = EventSession.Create(
            command.EventId,
            command.Title,
            command.Description,
            command.StartTime,
            command.EndTime);

        @event.AddSession(session);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(session.Id);
    }
}