using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateEventBanner;

internal sealed class UpdateEventBannerCommandHandler(
    IEventRepository eventRepository,
    IStorageService storageService,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventBannerCommand>
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public async Task<Result> Handle(UpdateEventBannerCommand command, CancellationToken cancellationToken)
    {
        if (command.File is null || command.File.Length == 0)
            return Result.Failure(EventErrors.EventImageErrors.FileRequired());

        if (command.File.Length > MaxFileSize)
            return Result.Failure(EventErrors.EventImageErrors.FileTooLarge(10));

        if (!AllowedContentTypes.Contains(command.File.ContentType.ToLowerInvariant()))
            return Result.Failure(EventErrors.EventImageErrors.InvalidFileType());

        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        if (!string.IsNullOrEmpty(@event.BannerUrl))
            await storageService.DeleteAsync(@event.BannerUrl, cancellationToken);

        await using var stream = command.File.OpenReadStream();
        var newBannerUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            StorageFolders.EventBanners,
            cancellationToken);

        @event.UpdateBannerUrl(newBannerUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}