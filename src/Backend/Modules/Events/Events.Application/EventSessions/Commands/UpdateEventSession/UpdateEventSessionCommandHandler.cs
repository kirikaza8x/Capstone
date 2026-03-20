using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.EventSessions.Commands.UpdateEventSession;

public sealed class UpdateEventSessionCommandValidator : AbstractValidator<UpdateEventSessionCommand>
{
    public UpdateEventSessionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Session title is required.")
            .MaximumLength(500).WithMessage("Session title must not exceed 500 characters.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}

internal sealed class UpdateEventSessionCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventSessionCommand>
{
    public async Task<Result> Handle(UpdateEventSessionCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithSessionsAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(Event.NotOwner);

        var session = @event.Sessions.FirstOrDefault(s => s.Id == command.SessionId);

        if (session is null)
            return Result.Failure(EventSessionErrors.NotFound(command.SessionId));

        session.Update(command.Title, command.Description, command.StartTime, command.EndTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
