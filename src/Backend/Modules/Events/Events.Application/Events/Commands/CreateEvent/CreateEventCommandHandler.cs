using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.OrganizerId)
            .NotEmpty().WithMessage("Organizer ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Event title is required")
            .MaximumLength(500).WithMessage("Event title must not exceed 500 characters");

        RuleFor(x => x.TicketSaleStartAt)
            .NotEmpty().WithMessage("Ticket sale start date is required")
            .LessThan(x => x.TicketSaleEndAt).WithMessage("Ticket sale start date must be before end date");

        RuleFor(x => x.TicketSaleEndAt)
            .NotEmpty().WithMessage("Ticket sale end date is required");

        RuleFor(x => x.EventStartAt)
            .NotEmpty().WithMessage("Event start date is required")
            .LessThan(x => x.EventEndAt).WithMessage("Event start date must be before end date");

        RuleFor(x => x.EventEndAt)
            .NotEmpty().WithMessage("Event end date is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Policy)
            .NotEmpty().WithMessage("Policy is required");

        RuleFor(x => x.UrlPath)
            .NotEmpty().WithMessage("URL path is required")
            .MaximumLength(255).WithMessage("URL path must not exceed 255 characters")
            .Matches(@"^[a-z0-9-]+$").WithMessage("URL path can only contain lowercase letters, numbers, and hyphens");

        RuleFor(x => x.EventTypeId)
            .GreaterThan(0).WithMessage("Event type is required");

        RuleFor(x => x.EventCategoryId)
            .GreaterThan(0).WithMessage("Event category is required");
    }
}

internal sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        // Check if URL path already exists
        var urlPathExists = await eventRepository.IsUrlPathExistsAsync(command.UrlPath, cancellationToken);
        if (urlPathExists)
        {
            return Result.Failure<Guid>(EventErrors.Event.UrlPathAlreadyExists(command.UrlPath));
        }

        // Create event
        var @event = Event.Create(
            command.OrganizerId,
            command.Title,
            command.TicketSaleStartAt,
            command.TicketSaleEndAt,
            command.EventStartAt,
            command.EventEndAt,
            command.Description,
            command.BannerUrl,
            command.Location,
            command.MapUrl,
            command.Policy,
            command.UrlPath,
            command.EventTypeId,
            command.EventCategoryId);

        eventRepository.Add(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(@event.Id);
    }
}