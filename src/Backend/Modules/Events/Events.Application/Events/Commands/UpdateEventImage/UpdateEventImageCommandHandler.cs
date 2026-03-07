using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.Events.Commands.UpdateEventImage;

internal sealed class UpdateEventImageCommandHandler(
    IEventRepository eventRepository,
    IStorageService storageService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventImageCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public async Task<Result> Handle(UpdateEventImageCommand command, CancellationToken cancellationToken)
    {
        // Validate file
        if (command.File is null || command.File.Length == 0)
            return Result.Failure(EventImageErrors.FileRequired());

        if (command.File.Length > MaxFileSize)
            return Result.Failure(EventImageErrors.FileTooLarge(10));

        if (!AllowedContentTypes.Contains(command.File.ContentType.ToLowerInvariant()))
            return Result.Failure(EventImageErrors.InvalidFileType());

        // Load Event aggregate with images
        var @event = await eventRepository.GetByIdWithImagesAsync(command.EventId, cancellationToken);
        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        // Get image through Aggregate Root
        var eventImage = @event.GetImage(command.ImageId);
        if (eventImage is null)
            return Result.Failure(EventImageErrors.NotFound(command.ImageId));

        // Delete old image from storage
        if (!string.IsNullOrEmpty(eventImage.ImageUrl))
        {
            await storageService.DeleteAsync(eventImage.ImageUrl, cancellationToken);
        }

        // Upload new image
        await using var stream = command.File.OpenReadStream();
        var folder = $"{StorageFolders.EventImages}/{command.EventId}";

        var newImageUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        // Update entity
        @event.UpdateImage(command.ImageId, newImageUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}