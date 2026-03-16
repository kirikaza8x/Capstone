using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.RejectCancelEvent;

public sealed class RejectCancelEventCommandValidator : AbstractValidator<RejectCancelEventCommand>
{
    public RejectCancelEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reject reason is required.")
            .MaximumLength(1000).WithMessage("Reject reason must not exceed 1000 characters.");
    }
}

internal sealed class RejectCancelEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<RejectCancelEventCommand>
{
    public async Task<Result> Handle(RejectCancelEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        var result = @event.RejectCancellationRequest(command.Reason);

        if (result.IsFailure)
            return result;

        eventRepository.Update(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}