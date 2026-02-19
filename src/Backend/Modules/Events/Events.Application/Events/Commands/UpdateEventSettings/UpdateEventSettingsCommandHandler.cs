using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using System.Text.RegularExpressions;

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
                .MinimumLength(3)
                .WithMessage("URL path must be at least 3 characters long.")
                .MaximumLength(100)
                .WithMessage("URL path must not exceed 100 characters.")
                .Matches(SlugPattern)
                .WithMessage("URL path can only contain lowercase letters, numbers, and hyphens.");
        });
    }
}

internal sealed class UpdateEventSettingsCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventSettingsCommand>
{
    public async Task<Result> Handle(UpdateEventSettingsCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);
        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        // Check unique URL path
        if (!string.IsNullOrWhiteSpace(command.UrlPath))
        {
            var urlPath = command.UrlPath.Trim().ToLowerInvariant();
            var existingEvent = await eventRepository.GetByUrlPathAsync(urlPath, cancellationToken);

            if (existingEvent is not null && existingEvent.Id != command.EventId)
                return Result.Failure(EventErrors.Event.UrlPathAlreadyExists(urlPath));
        }

        @event.UpdateSettings(command.IsEmailReminderEnabled, command.UrlPath);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}