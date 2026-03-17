using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.SuspendEvent;

public sealed class SuspendEventCommandValidator : AbstractValidator<SuspendEventCommand>
{
    public SuspendEventCommandValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

internal sealed class SuspendEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<SuspendEventCommand>
{
    public async Task<Result> Handle(SuspendEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        var result = @event.Suspend(command.Reason);

        if (result.IsFailure)
            return result;

        eventRepository.Update(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}