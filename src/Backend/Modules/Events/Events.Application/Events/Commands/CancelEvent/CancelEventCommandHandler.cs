using Events.Application.Abstractions;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Events.IntegrationEvents;
using FluentValidation;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.CancelEvent;

public sealed class CancelEventCommandValidator : AbstractValidator<CancelEventCommand>
{
    public CancelEventCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.")
            .When(x => x.Reason is not null);
    }
}

internal sealed class CancelEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ISeatLockService seatLockService,
    IEventBus eventBus) : ICommandHandler<CancelEventCommand>
{
    public async Task<Result> Handle(CancelEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        var result = @event.Cancel(command.Reason);

        if (result.IsFailure)
            return result;

        eventRepository.Update(@event);

        // Release seat locks for the event
        await seatLockService.ReleaseAllLocksForEventAsync(command.EventId, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
