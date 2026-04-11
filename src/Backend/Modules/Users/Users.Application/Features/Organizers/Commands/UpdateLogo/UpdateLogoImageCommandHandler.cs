using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using Users.Application.Storage;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Organizers.Commands.UpdateLogo;

internal sealed class UpdateLogoImageCommandHandler(
    IUserRepository userRepository,
    IStorageService storageService,
    IUserUnitOfWork unitOfWork
) : ICommandHandler<UpdateLogoImageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        UpdateLogoImageCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository
            .GetByIdWithOrganizerProfileAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound("User.NotFound", "User not found."));
        }

        var profile = user.DraftProfile;

        if (profile is null)
        {
            return Result.Failure<Guid>(
                Error.Failure(
                    "Organizer.Profile.NotFound",
                    "Draft profile not found."));
        }

        if (!string.IsNullOrEmpty(profile.Logo))
        {
            await storageService.DeleteAsync(profile.Logo, cancellationToken);
        }

        await using var stream = command.File.OpenReadStream();

        var folder = $"{StoragePath.UserProfileImages}/{command.UserId}/organizer";

        var imageUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        user.UpdateDraftLogo(imageUrl);

        userRepository.Update(user);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(profile.Id);
    }
}