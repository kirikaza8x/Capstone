using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.EventSessions.Commands.CreateEventSession;

public sealed class CreateEventSessionItemValidator : AbstractValidator<CreateEventSessionItem>
{
    public CreateEventSessionItemValidator()
    {
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

public sealed class CreateEventSessionValidator : AbstractValidator<CreateEventSessionCommand>
{
    public CreateEventSessionValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Sessions)
            .NotEmpty().WithMessage("At least one session is required.");

        RuleForEach(x => x.Sessions)
            .SetValidator(new CreateEventSessionItemValidator());
    }
}

internal sealed class CreateEventSessionCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICommandHandler<CreateEventSessionCommand, List<Guid>>
{
    public async Task<Result<List<Guid>>> Handle(CreateEventSessionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<List<Guid>>(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure<List<Guid>>(EventErrors.Event.NotOwner);

        var sessions = command.Sessions
            .Select(s => EventSession.Create(
                command.EventId,
                s.Title,
                s.Description,
                s.StartTime,
                s.EndTime))
            .ToList();

        sessions.ForEach(@event.AddSession);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(sessions.Select(s => s.Id).ToList());
    }
}
