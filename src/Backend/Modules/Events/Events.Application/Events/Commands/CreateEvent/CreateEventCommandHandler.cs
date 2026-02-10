using Events.Domain.Entities;
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

        RuleFor(x => x.HashtagIds)
            .NotEmpty().WithMessage("At least one hashtag is required");

        RuleFor(x => x.EventCategoryId)
            .GreaterThan(0).WithMessage("Event category is required");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}

internal sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork) : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        // Create event
        var @event = Event.Create(
            organizerId: command.OrganizerId,
            title: command.Title,
            bannerUrl: command.BannerUrl,
            location: command.Location,
            mapUrl: command.MapUrl,
            description: command.Description,
            eventCategoryId: command.EventCategoryId);

        // Add hashtags
        foreach (var hashtagId in command.HashtagIds)
        {
            var eventHashtag = EventHashtag.Create(@event.Id, hashtagId);
            @event.AddHashtag(eventHashtag);
        }

        eventRepository.Add(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(@event.Id);
    }
}