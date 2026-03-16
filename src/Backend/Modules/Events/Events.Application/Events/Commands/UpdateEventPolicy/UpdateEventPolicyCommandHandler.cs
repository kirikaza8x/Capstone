using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateEventPolicy;

public sealed class UpdateEventPolicyCommandValidator : AbstractValidator<UpdateEventPolicyCommand>
{
    public UpdateEventPolicyCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required.");

        RuleFor(x => x.Policy)
            .NotEmpty()
            .WithMessage("Policy is required.")
            .MaximumLength(10000)
            .WithMessage("Policy must not exceed 10000 characters.");
    }
}

internal sealed class UpdateEventPolicyCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateEventPolicyCommand>
{
    public async Task<Result> Handle(UpdateEventPolicyCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        @event.UpdatePolicy(command.Policy);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}