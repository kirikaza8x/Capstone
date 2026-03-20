using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.EventImages.DeleteEventImage;

internal sealed class DeleteEventImageCommandHandler(
    IEventRepository eventRepository,
    IStorageService storageService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteEventImageCommand>
{
    public async Task<Result> Handle(DeleteEventImageCommand command, CancellationToken cancellationToken)
    {
        // Load Event aggregate with images
        var @event = await eventRepository.GetByIdWithImagesAsync(command.EventId, cancellationToken);
        if (@event is null)
            return Result.Failure(Event.NotFound(command.EventId));

        // Get image 
        var eventImage = @event.GetImage(command.ImageId);
        if (eventImage is null)
            return Result.Failure(EventImageErrors.NotFound(command.ImageId));

        // Delete from storage
        if (!string.IsNullOrEmpty(eventImage.ImageUrl))
        {
            await storageService.DeleteAsync(eventImage.ImageUrl, cancellationToken);
        }

        // Remove
        @event.RemoveImage(command.ImageId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
