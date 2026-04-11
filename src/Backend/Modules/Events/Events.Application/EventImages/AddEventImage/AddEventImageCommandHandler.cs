using Events.Domain.Entities;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using static Events.Domain.Errors.EventErrors;

namespace Events.Application.EventImages.AddEventImage;

internal sealed class AddEventImageCommandHandler(
    IEventRepository eventRepository,
    IStorageService storageService,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<AddEventImageCommand, Guid>
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public async Task<Result<Guid>> Handle(AddEventImageCommand command, CancellationToken cancellationToken)
    {
        if (command.File is null || command.File.Length == 0)
            return Result.Failure<Guid>(EventImageErrors.FileRequired());

        if (command.File.Length > MaxFileSize)
            return Result.Failure<Guid>(EventImageErrors.FileTooLarge(10));

        if (!AllowedContentTypes.Contains(command.File.ContentType.ToLowerInvariant()))
            return Result.Failure<Guid>(EventImageErrors.InvalidFileType());

        var @event = await eventRepository.GetByIdWithImagesAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<Guid>(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure<Guid>(EventErrors.Event.NotOwner);

        await using var stream = command.File.OpenReadStream();
        var folder = $"{StorageFolders.EventImages}/{command.EventId}";

        var imageUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        var eventImage = @event.AddImage(imageUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(eventImage.Id);
    }
}
