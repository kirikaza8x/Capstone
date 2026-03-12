using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateEvent;

public sealed class UpdateActorImageItemValidator : AbstractValidator<UpdateActorImageItem>
{
    public UpdateActorImageItemValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Actor name is required.")
            .MaximumLength(200).WithMessage("Actor name must not exceed 200 characters.");

        RuleFor(x => x.Major)
            .MaximumLength(200).WithMessage("Actor major must not exceed 200 characters.");
    }
}

public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Event title is required.")
                .MaximumLength(500).WithMessage("Event title must not exceed 500 characters.");
        });

        When(x => x.HashtagIds is not null, () =>
        {
            RuleFor(x => x.HashtagIds)
                .NotEmpty().WithMessage("At least one hashtag is required.");
        });

        When(x => x.CategoryIds is not null, () =>
        {
            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("At least one category is required.");
        });

        When(x => x.Location is not null, () =>
        {
            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                .MaximumLength(500).WithMessage("Location must not exceed 500 characters.");
        });

        When(x => x.Description is not null, () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        });

        When(x => x.ActorImages is not null, () =>
        {
            RuleForEach(x => x.ActorImages)
                .SetValidator(new UpdateActorImageItemValidator());
        });
    }
}

internal sealed class UpdateEventCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventCommand>
{
    public async Task<Result> Handle(UpdateEventCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetDetailsByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        if (command.Title is not null || command.Location is not null || command.Description is not null)
        {
            @event.UpdateInfo(
                command.Title ?? @event.Title,
                command.Location ?? @event.Location,
                command.MapUrl ?? @event.MapUrl,
                command.Description ?? @event.Description);
        }

        if (command.HashtagIds is not null)
        {
            @event.ReplaceHashtags(
                command.HashtagIds.Select(id => EventHashtag.Create(@event.Id, id)));
        }

        if (command.CategoryIds is not null)
        {
            @event.ReplaceCategories(
                command.CategoryIds.Select(id => EventCategory.Create(@event.Id, id)));
        }

        if (command.ActorImages is not null)
        {
            @event.ReplaceActorImages(
                command.ActorImages.Select(a => EventActorImage.Create(@event.Id, a.Name, a.Major, a.Image)));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}