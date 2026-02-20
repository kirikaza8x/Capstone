using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Shared.Application.Abstractions.Storage;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Events.Application;

namespace Users.Application.Features.Users.Commands.Handlers;

internal sealed class UpdateProfileImageCommandHandler(
    IUserRepository userRepository,
    IStorageService storageService,
    IUserUnitOfWork unitOfWork
    //,
    //IMapper mapper
    ) : ICommandHandler<UpdateProfileImageCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdateProfileImageCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<Guid>(Error.NotFound("User.NotFound", "User not found.")
);

        await using var stream = command.File.OpenReadStream();
        var folder = $"{StoragePath.UserProfileImages}/{command.UserId}";

        var imageUrl = await storageService.UploadAsync(
            stream,
            command.File.FileName,
            command.File.ContentType,
            folder,
            cancellationToken);

        user.UpdateProfileImage(imageUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
