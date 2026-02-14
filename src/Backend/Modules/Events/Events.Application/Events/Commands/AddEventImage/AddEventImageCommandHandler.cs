using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Storage;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.Events.Commands.AddEventImage;

internal sealed class AddEventImageCommandHandler(
    IEventRepository eventRepository,
    IStorageService storageService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<AddEventImageCommand, Guid>
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public async Task<Result<Guid>> Handle(AddEventImageCommand command, CancellationToken cancellationToken)
    {
        // Validate file
        if (command.File is null || command.File.Length == 0)
            return Result.Failure<Guid>(EventImageErrors.FileRequired());

        if (command.File.Length > MaxFileSize)
            return Result.Failure<Guid>(EventImageErrors.FileTooLarge(10));

        if (!AllowedContentTypes.Contains(command.File.ContentType.ToLowerInvariant()))
            return Result.Failure<Guid>(EventImageErrors.InvalidFileType());

        // Check event exists
        var @event = await eventRepository.GetByIdWithImagesAsync(command.EventId, cancellationToken);
        if (@event is null)
            return Result.Failure<Guid>(EventErrors.Event.NotFound(command.EventId));

        // Upload to storage
        await using var stream = command.File.OpenReadStream();
        var folder = $"{StorageFolders.EventImages}/{command.EventId}";

        var imageUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        // Create and add image entity
        var eventImage = @event.AddImage(imageUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(eventImage.Id);
    }
}
