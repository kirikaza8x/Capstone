using Events.Domain.Entities;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed class CreateActorImageItemValidator : AbstractValidator<CreateActorImageItem>
{
    public CreateActorImageItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Actor name is required.")
            .MaximumLength(200).WithMessage("Actor name must not exceed 200 characters.");

        RuleFor(x => x.Major)
            .MaximumLength(200).WithMessage("Actor major must not exceed 200 characters.");
    }
}

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

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("At least one category is required");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(500).WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");

        RuleForEach(x => x.ActorImages)
            .SetValidator(new CreateActorImageItemValidator());
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
            description: command.Description);

        // Add hashtags
        foreach (var hashtagId in command.HashtagIds)
        {
            var eventHashtag = EventHashtag.Create(@event.Id, hashtagId);
            @event.AddHashtag(eventHashtag);
        }

        foreach (var categoryId in command.CategoryIds)
        {
            var eventCategory = EventCategory.Create(@event.Id, categoryId);
            @event.AddCategories(eventCategory);
        }

        foreach (var actor in command.ActorImages)
        {
            @event.AddActorImage(EventActorImage.Create(@event.Id, actor.Name, actor.Major, actor.Image));
        }

        eventRepository.Add(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(@event.Id);
    }
}