using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateEventSettings;

public sealed class UpdateEventSettingsCommandValidator : AbstractValidator<UpdateEventSettingsCommand>
{
    private const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    public UpdateEventSettingsCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required.");

        When(x => !string.IsNullOrWhiteSpace(x.UrlPath), () =>
        {
            RuleFor(x => x.UrlPath)
                .MinimumLength(3).WithMessage("URL path must be at least 3 characters long.")
                .MaximumLength(100).WithMessage("URL path must not exceed 100 characters.")
                .Matches(SlugPattern).WithMessage("URL path can only contain lowercase letters, numbers, and hyphens.");
        });

        When(x => x.TicketSaleStartAt.HasValue && x.TicketSaleEndAt.HasValue, () =>
        {
            RuleFor(x => x.TicketSaleStartAt)
                .LessThan(x => x.TicketSaleEndAt)
                .WithMessage("Ticket sale start time must be before ticket sale end time.");
        });

        When(x => x.EventStartAt.HasValue && x.EventEndAt.HasValue, () =>
        {
            RuleFor(x => x.EventStartAt)
                .LessThan(x => x.EventEndAt)
                .WithMessage("Event start time must be before event end time.");
        });

        When(x => x.TicketSaleEndAt.HasValue && x.EventEndAt.HasValue, () =>
        {
            RuleFor(x => x.TicketSaleEndAt)
                .LessThan(x => x.EventEndAt)
                .WithMessage("Ticket sale end time must be before event end time.");
        });
    }
}

internal sealed class UpdateEventSettingsCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateEventSettingsCommand>
{
    public async Task<Result> Handle(UpdateEventSettingsCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        if (!string.IsNullOrWhiteSpace(command.UrlPath))
        {
            var urlPath = command.UrlPath.Trim().ToLowerInvariant();
            var existingEvent = await eventRepository.GetByUrlPathAsync(urlPath, cancellationToken);

            if (existingEvent is not null && existingEvent.Id != command.EventId)
                return Result.Failure(EventErrors.Event.UrlPathAlreadyExists(urlPath));
        }

        var updateSettingsResult = @event.UpdateSettings(command.IsEmailReminderEnabled, command.UrlPath);
        if (updateSettingsResult.IsFailure)
            return updateSettingsResult;

        if (command.TicketSaleStartAt.HasValue &&
            command.TicketSaleEndAt.HasValue &&
            command.EventStartAt.HasValue &&
            command.EventEndAt.HasValue)
        {
            var updateScheduleResult = @event.UpdateSchedule(
                command.TicketSaleStartAt.Value,
                command.TicketSaleEndAt.Value,
                command.EventStartAt.Value,
                command.EventEndAt.Value);

            if (updateScheduleResult.IsFailure)
                return updateScheduleResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}