using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.RejectPublishEvent;

public sealed class RejectPublishEventCommandValidator : AbstractValidator<RejectPublishEventCommand>
{
    public RejectPublishEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reject reason is required.")
            .MaximumLength(1000).WithMessage("Reject reason must not exceed 1000 characters.");
    }
}

internal sealed class RejectPublishEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<RejectPublishEventCommand>
{
    public async Task<Result> Handle(RejectPublishEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        var result = @event.RejectPublishRequest(command.Reason);

        if (result.IsFailure)
            return result;

        eventRepository.Update(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
